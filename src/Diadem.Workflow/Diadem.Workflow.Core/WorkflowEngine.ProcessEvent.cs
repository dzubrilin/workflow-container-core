using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution;
using Diadem.Workflow.Core.Execution.Events;
using Diadem.Workflow.Core.Execution.States;
using Diadem.Workflow.Core.Model;
using Serilog;

namespace Diadem.Workflow.Core
{
    public partial class WorkflowEngine
    {
        Task<WorkflowProcessingResult> IWorkflowEngine.ProcessEvent(Guid workflowId, IEvent @event)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                return ((IWorkflowEngine) this).ProcessEvent(workflowId, @event, cancellationTokenSource.Token);
            }
        }

        async Task<WorkflowProcessingResult> IWorkflowEngine.ProcessEvent(Guid workflowId, IEvent @event, CancellationToken cancellationToken)
        {
            var started = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            WorkflowProcessingResult workflowProcessingResult = null;
            try
            {
                workflowProcessingResult = await ProcessEventInternal(workflowId, @event, cancellationToken).ConfigureAwait(false);
                return workflowProcessingResult;
            }
            finally
            {
                stopwatch.Stop();
                Log.Debug("Finished execution of an event=[{code}] {duration}, {workflowInstanceId}",
                    @event.Code, stopwatch.Elapsed, workflowProcessingResult?.WorkflowInstance?.Id);

                if (null != workflowProcessingResult?.WorkflowInstance)
                {
                    // 1. save workflow event execution log into store
                    var workflowInstanceEventLog = new WorkflowInstanceEventLog(workflowProcessingResult.WorkflowInstance.Id, @event.Code, started, (int) stopwatch.ElapsedMilliseconds);
                    await WorkflowEngineBuilder.WorkflowStore.SaveWorkflowInstanceEventLog(workflowInstanceEventLog, cancellationToken).ConfigureAwait(false);

                    // 2. make sure that workflow instance lock is removed for non hierarchical events
                    if (!(@event is IHierarchicalEvent))
                    {
                        await UnlockWorkflowInstance(workflowProcessingResult.WorkflowInstance, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<WorkflowProcessingResult> ProcessEventInternal(Guid workflowId, IEvent @event, CancellationToken cancellationToken)
        {
            var isHierarchicalEvent = @event.IsHierarchicalEvent();
            var workflowConfiguration = await GetWorkflowConfiguration(workflowId, cancellationToken).ConfigureAwait(false);

            var eventConfiguration = workflowConfiguration.GetEventConfigurationByCode(@event.Code);
            if (null == eventConfiguration && isHierarchicalEvent)
            {
                if (!Enum.TryParse(@event.Code, out EventTypeConfiguration eventTypeConfiguration))
                {
                    throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                        "System event must have code of EventType but following value have been encountered [{0}] for workflow [{1}]",
                        @event.Code, workflowConfiguration.Code));
                }

                eventConfiguration = workflowConfiguration.GetEventConfigurationByType(eventTypeConfiguration);
            }

            if (null == eventConfiguration)
            {
                throw new WorkflowException($"Event with code [{@event.Code}] was not found in workflow [{workflowConfiguration.Code}]");
            }

            WorkflowContext workflowContext;
            IWorkflowInstance workflowInstance;
            StateConfiguration stateConfiguration;
            StateEventConfiguration stateEventConfiguration;
            if (eventConfiguration.Type == EventTypeConfiguration.Initial || eventConfiguration.Type == EventTypeConfiguration.ParentToNestedInitial)
            {
                stateConfiguration = workflowConfiguration.GetStateConfigurationByType(StateTypeConfiguration.Initial);
                stateEventConfiguration = stateConfiguration.Events
                    .Single(se => string.Equals(se.Code, eventConfiguration.Code, StringComparison.OrdinalIgnoreCase));

                var id = Guid.NewGuid();
                var workflowInstanceLock = CreateWorkflowInstanceLock(workflowConfiguration);
                workflowInstance = new WorkflowInstance(workflowId, id, @event.GetRootWorkflowInstanceIdOrUseCurrent(id),
                    @event.GetParentWorkflowInstanceIdOrUseCurrent(id), @event.GetDomainEntityTypeOrNull(),
                    @event.GetDomainEntityIdOrNull(), workflowInstanceLock, @event.GetDomainEntityOrNull())
                {
                    Created = DateTime.UtcNow,
                    CurrentStateCode = stateConfiguration.Code,
                    CurrentStateProgress = StateExecutionProgress.AwaitingEvent
                };

                await TryGetDomainEntity(workflowConfiguration, workflowInstance, cancellationToken).ConfigureAwait(false);
                await SaveWorkflowInstance(workflowInstance, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var hierarchicalEvent = @event as IHierarchicalEvent;
                if (null != hierarchicalEvent)
                {
                    // for system events:
                    //     1. intended to be used inside activities only
                    //     2. lock on a root workflow instance should have been obtained earlier
                    workflowInstance = hierarchicalEvent.TargetWorkflowInstance ?? await GetWorkflowInstanceWithNoLock(@event.WorkflowInstanceId, cancellationToken)
                        .ConfigureAwait(false);
                    Guard.ArgumentNotNull(workflowInstance, nameof(workflowInstance));
                }
                else
                {
                    // for other events: we need to obtain a lock on a root workflow instance
                    workflowInstance = await GetWorkflowInstanceWithLock(@event.WorkflowInstanceId, cancellationToken).ConfigureAwait(false);
                    Guard.ArgumentNotNull(workflowInstance, nameof(workflowInstance));
                }

                stateConfiguration = workflowConfiguration.GetStateConfigurationByCode(workflowInstance.CurrentStateCode);
                Guard.ArgumentNotNull(stateConfiguration, nameof(stateConfiguration));

                if (null != hierarchicalEvent)
                {
                    // break cycling
                    if (stateConfiguration.Type == StateTypeConfiguration.Failed && eventConfiguration.Type.IsFailed())
                    {
                        return new WorkflowProcessingResult(workflowInstance);
                    }

                    if (hierarchicalEvent.HierarchyEventType == WorkflowHierarchyEventType.Failed)
                    {
                        Log.Warning("Received event {code} into {workflowInstanceId}]", @event.Code, workflowInstance.Id);
                        return await TransitionToFailedStatus(@event, workflowInstance, workflowConfiguration, cancellationToken).ConfigureAwait(false);
                    }
                }

                // workflow instance must be awaiting for the event otherwise move it to failed
                if (workflowInstance.CurrentStateProgress != StateExecutionProgress.AwaitingEvent)
                {
                    Log.Error("Can not process event {code} for {workflowInstanceId} because workflow instance is not in [{subState}], it is in [{state}] and [{stateProgress}]",
                        @event.Code, workflowInstance.Id, StateExecutionProgress.AwaitingEvent, workflowInstance.CurrentStateCode, workflowInstance.CurrentStateProgress);
                    return await TransitionToFailedStatus(@event, workflowInstance, workflowConfiguration, cancellationToken).ConfigureAwait(false);
                }

                if (null == stateConfiguration)
                {
                    Log.Error("{workflowInstanceId} is in the {state} which does not belong to {workflow}",
                        workflowInstance.Id, workflowInstance.CurrentStateCode, workflowConfiguration.Code);
                    return await TransitionToFailedStatus(@event, workflowInstance, workflowConfiguration, cancellationToken).ConfigureAwait(false);
                }

                if (isHierarchicalEvent)
                {
                    stateEventConfiguration = stateConfiguration.Events
                        .SingleOrDefault(e => string.Equals(e.Code, @event.Code, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    if (!stateConfiguration.Events.Any(e => string.Equals(e.Code, @event.Code, StringComparison.OrdinalIgnoreCase)))
                    {
                        Log.Error("{workflowInstanceId} is in the {state} which does not accept {event}",
                            workflowInstance.Id, stateConfiguration.Code, @event.Code);
                        return await TransitionToFailedStatus(@event, workflowInstance, workflowConfiguration, cancellationToken).ConfigureAwait(false);
                    }

                    stateEventConfiguration = stateConfiguration.Events.Single(e => string.Equals(e.Code, @event.Code, StringComparison.OrdinalIgnoreCase));
                }

                workflowInstance.Entity = @event.GetDomainEntityOrNull();
                await TryGetDomainEntity(workflowConfiguration, workflowInstance, cancellationToken).ConfigureAwait(false);
            }

            workflowContext = new WorkflowContext(this, WorkflowEngineBuilder, workflowInstance, workflowConfiguration, @event.GetEventStateOrDefault());
            var eventExecutionContext = new EventExecutionContext(workflowContext, @event, eventConfiguration, stateEventConfiguration);
            var eventExecutionResult = await _eventProcessorLazy.Value.Process(eventExecutionContext, cancellationToken).ConfigureAwait(false);
            switch (eventExecutionResult.Status)
            {
                case EventExecutionStatus.KeepWaiting:
                {
                    Log.Information("After processing event [{code}] the {workflowInstanceId} keeps waiting in for another event of {nextCode}",
                        @event.Code, workflowInstance.Id, @event.Code);

                    break;
                }
                case EventExecutionStatus.Failed:
                case EventExecutionStatus.FailedNoRetry:
                {
                    Log.Error("An error has occurred during processing event [{code}] for {workflowInstanceId}]",@event.Code, workflowInstance.Id);

                    await TransitionToState(workflowContext, workflowConfiguration.GetFailedStateConfiguration(), null, cancellationToken).ConfigureAwait(false);
                    break;
                }
                default:
                {
                    workflowContext.WorkflowInstance.CurrentStateProgress = StateExecutionProgress.AfterEventProcessed;
                    await ProcessState(workflowContext, stateConfiguration, cancellationToken).ConfigureAwait(false);
                    break;
                }
            }

            return new WorkflowProcessingResult(workflowInstance, workflowContext.WorkflowExecutionState);
        }

        private async Task<WorkflowProcessingResult> TransitionToFailedStatus(IEvent @event,
            IWorkflowInstance workflowInstance, WorkflowConfiguration workflowConfiguration, CancellationToken cancellationToken)
        {
            var workflowContext = new WorkflowContext(this, WorkflowEngineBuilder, workflowInstance, workflowConfiguration, @event.GetEventStateOrDefault());
            await TransitionToState(workflowContext, workflowConfiguration.GetFailedStateConfiguration(), null, cancellationToken).ConfigureAwait(false);
            return new WorkflowProcessingResult(workflowInstance, workflowContext.WorkflowExecutionState);
        }
    }
}