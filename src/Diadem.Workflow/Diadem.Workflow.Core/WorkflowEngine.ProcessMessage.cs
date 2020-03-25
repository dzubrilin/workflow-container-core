using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution;
using Diadem.Workflow.Core.Execution.Events;
using Diadem.Workflow.Core.Execution.Messages;
using Diadem.Workflow.Core.Execution.States;
using Diadem.Workflow.Core.Execution.Transitions;
using Diadem.Workflow.Core.Model;
using Serilog;
using Serilog.Events;

namespace Diadem.Workflow.Core
{
    public partial class WorkflowEngine
    {
        Task<WorkflowProcessingResult> IWorkflowEngine.ProcessMessage(IWorkflowMessage workflowMessage)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                return ((IWorkflowEngine) this).ProcessMessage(workflowMessage, cancellationTokenSource.Token);
            }
        }

        async Task<WorkflowProcessingResult> IWorkflowEngine.ProcessMessage(IWorkflowMessage workflowMessage, CancellationToken cancellationToken)
        {
            var started = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            WorkflowProcessingResult workflowProcessingResult = null;
            try
            {
                switch (workflowMessage)
                {
                    case IEventRequestWorkflowMessage eventWorkflowMessage:
                        workflowProcessingResult = await ProcessMessageInternal(eventWorkflowMessage, cancellationToken).ConfigureAwait(false);
                        break;

                    case IAsynchronousTransitionWorkflowMessage asynchronousTransitionWorkflowMessage:
                        workflowProcessingResult = await ProcessMessageInternal(asynchronousTransitionWorkflowMessage, cancellationToken).ConfigureAwait(false);
                        break;

                    case IDelayedTransitionWorkflowMessage delayedTransitionWorkflowMessage:
                        workflowProcessingResult = await ProcessMessageInternal(delayedTransitionWorkflowMessage, cancellationToken).ConfigureAwait(false);
                        break;

                    default:
                        workflowProcessingResult = await ProcessMessageInternal(workflowMessage, cancellationToken).ConfigureAwait(false);
                        break;
                }

                return workflowProcessingResult;
            }
            finally
            {
                stopwatch.Stop();
                if (Log.IsEnabled(LogEventLevel.Verbose))
                {
                    Log.Verbose("Finished processing of a {messageID}, {duration}, {workflowInstanceId}",
                        workflowMessage.WorkflowMessageId, stopwatch.Elapsed, workflowProcessingResult?.WorkflowInstance?.Id);
                }

                if (null != workflowProcessingResult?.WorkflowInstance)
                {
                    // 1. save workflow message execution log into store
                    var workflowInstanceMessageLog = new WorkflowInstanceMessageLog(workflowProcessingResult.WorkflowInstance.Id, workflowMessage.WorkflowMessageId,
                        workflowMessage.GetType().Name, started, (int) stopwatch.ElapsedMilliseconds);
                    await WorkflowEngineBuilder.WorkflowStore.SaveWorkflowInstanceMessageLog(workflowInstanceMessageLog, cancellationToken).ConfigureAwait(false);

                    // 2. make sure that workflow instance lock is removed
                    if (workflowProcessingResult.WorkflowInstance.Lock.LockedAt < workflowProcessingResult.WorkflowInstance.Lock.LockedUntil)
                    {
                        await UnlockWorkflowInstance(workflowProcessingResult.WorkflowInstance, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<WorkflowProcessingResult> ProcessMessageInternal(IEventRequestWorkflowMessage eventWorkflowMessage, CancellationToken cancellationToken)
        {
            Guard.ArgumentNotNull(eventWorkflowMessage, nameof(eventWorkflowMessage));
            if (null == eventWorkflowMessage.State && string.IsNullOrEmpty(eventWorkflowMessage.EntityType) && string.IsNullOrEmpty(eventWorkflowMessage.EntityId))
            {
                throw new ArgumentException("Event has to have either [State] or [EntityType && EntityID]");
            }

            var workflowId = eventWorkflowMessage.WorkflowId;
            var workflowInstanceId = eventWorkflowMessage.WorkflowInstanceId;
            if (eventWorkflowMessage.WorkflowId == Guid.Empty)
            {
                if (string.IsNullOrEmpty(eventWorkflowMessage.EntityType) || string.IsNullOrEmpty(eventWorkflowMessage.EntityId))
                {
                    throw new WorkflowException($"Can not process event message [{eventWorkflowMessage.WorkflowMessageId:D}] since it does not contain neither WorkflowId nor EntityType & EntityId");
                }

                var workflowInstance = await WorkflowEngineBuilder.WorkflowStore
                    .GetWorkflowInstance(eventWorkflowMessage.EntityType, eventWorkflowMessage.EntityId, cancellationToken).ConfigureAwait(false);
                if (null == workflowInstance)
                {
                    throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                        "Can not process event message [{0:D}] because no workflow instance has been found for [EntityType={1}] and [EntityId={2}]",
                        eventWorkflowMessage.WorkflowMessageId, eventWorkflowMessage.EntityType, eventWorkflowMessage.EntityId));
                }

                workflowId = workflowInstance.WorkflowId;
                workflowInstanceId = workflowInstance.Id;
            }

            var statefulEvent = new StatefulDomainEvent(eventWorkflowMessage.EventCode, workflowInstanceId ?? Guid.Empty,
                eventWorkflowMessage.EntityType, eventWorkflowMessage.EntityId, eventWorkflowMessage.State?.JsonState.AsJsonState());
            var result = await ((IWorkflowEngine) this).ProcessEvent(workflowId, statefulEvent, cancellationToken).ConfigureAwait(false);
            return result;
        }

        private async Task<WorkflowProcessingResult> ProcessMessageInternal(IAsynchronousTransitionWorkflowMessage asynchronousTransitionWorkflowMessage, CancellationToken cancellationToken)
        {
            Guard.ArgumentNotNull(asynchronousTransitionWorkflowMessage, nameof(asynchronousTransitionWorkflowMessage));
            Guard.ArgumentNotNull(asynchronousTransitionWorkflowMessage.WorkflowInstanceId, nameof(asynchronousTransitionWorkflowMessage.WorkflowInstanceId));
            Debug.Assert(asynchronousTransitionWorkflowMessage.WorkflowInstanceId != null, "workflowMessage.WorkflowInstanceId != null");

            var workflowInstance = await GetWorkflowInstanceWithLock(asynchronousTransitionWorkflowMessage.WorkflowInstanceId.Value, cancellationToken).ConfigureAwait(false);
            if (null == workflowInstance)
            {
                throw new WorkflowException($"Workflow message [{asynchronousTransitionWorkflowMessage.WorkflowMessageId:D}] is referring to non existing workflow instance");
            }

            var workflowConfiguration = await GetWorkflowConfiguration(workflowInstance.WorkflowId, cancellationToken).ConfigureAwait(false);
            Guard.ArgumentNotNull(workflowConfiguration, nameof(workflowConfiguration));

            var stateConfiguration = workflowConfiguration.GetStateConfigurationByCode(workflowInstance.CurrentStateCode);
            Guard.ArgumentNotNull(stateConfiguration, nameof(stateConfiguration));

            var workflowContext = new WorkflowContext(this, WorkflowEngineBuilder, workflowInstance, workflowConfiguration);
            if (workflowInstance.CurrentStateProgress != StateExecutionProgress.AwaitingAsyncTransition)
            {
                Log.Error("Can not process message [{messageId}] for {workflowInstanceId} because workflow instance is not in [{subState}]",
                    asynchronousTransitionWorkflowMessage.WorkflowMessageId, workflowInstance.Id, StateExecutionProgress.AwaitingAsyncTransition);

                await TransitionToState(workflowContext, workflowConfiguration.GetFailedStateConfiguration(), null, cancellationToken).ConfigureAwait(false);
                return new WorkflowProcessingResult(workflowInstance, workflowContext.WorkflowExecutionState);
            }
            
            // ensure that domain entity is available for all/any coded logic for read-only purposes
            await TryGetDomainEntity(workflowContext, cancellationToken).ConfigureAwait(false);

            workflowContext.WorkflowInstance.CurrentStateProgress = StateExecutionProgress.AfterAsyncTransition;
            await ProcessState(workflowContext, stateConfiguration, cancellationToken).ConfigureAwait(false);
            return new WorkflowProcessingResult(workflowInstance, workflowContext.WorkflowExecutionState);
        }

        private async Task<WorkflowProcessingResult> ProcessMessageInternal(IDelayedTransitionWorkflowMessage delayedTransitionWorkflowMessage, CancellationToken cancellationToken)
        {
            Guard.ArgumentNotNull(delayedTransitionWorkflowMessage, nameof(delayedTransitionWorkflowMessage));
            Guard.ArgumentNotNull(delayedTransitionWorkflowMessage.WorkflowInstanceId, nameof(delayedTransitionWorkflowMessage.WorkflowInstanceId));
            Debug.Assert(delayedTransitionWorkflowMessage.WorkflowInstanceId != null, "workflowMessage.WorkflowInstanceId != null");

            var workflowInstance = await GetWorkflowInstanceWithLock(delayedTransitionWorkflowMessage.WorkflowInstanceId.Value, cancellationToken).ConfigureAwait(false);
            if (null == workflowInstance)
            {
                throw new WorkflowException($"Workflow message [{delayedTransitionWorkflowMessage.WorkflowMessageId:D}] is referring to non existing workflow instance");
            }

            var workflowConfiguration = await GetWorkflowConfiguration(workflowInstance.WorkflowId, cancellationToken).ConfigureAwait(false);
            Guard.ArgumentNotNull(workflowConfiguration, nameof(workflowConfiguration));

            var stateConfiguration = workflowConfiguration.GetStateConfigurationByCode(workflowInstance.CurrentStateCode);
            Guard.ArgumentNotNull(stateConfiguration, nameof(stateConfiguration));

            if (stateConfiguration.Type == StateTypeConfiguration.Final || stateConfiguration.Type == StateTypeConfiguration.Failed)
            {
                Log.Debug("Skipping delayed transition message [{messageId}] because {workflowInstanceId} is in [{state}]",
                    delayedTransitionWorkflowMessage.WorkflowMessageId, delayedTransitionWorkflowMessage.WorkflowInstanceId, stateConfiguration.Code);

                return new WorkflowProcessingResult(workflowInstance);
            }

            var workflowContext = new WorkflowContext(this, WorkflowEngineBuilder, workflowInstance, workflowConfiguration);
            
            // ensure that domain entity is available for all/any coded logic for read-only purposes
            await TryGetDomainEntity(workflowContext, cancellationToken).ConfigureAwait(false);

            var stateExecutionContext = new StateExecutionContext(workflowContext, stateConfiguration);
            var fromStateConfiguration = workflowConfiguration.GetStateConfigurationByCode(delayedTransitionWorkflowMessage.MoveFromState);
            var transitionConfiguration = fromStateConfiguration.Transitions
                .Single(t => string.Equals(t.MoveToState, delayedTransitionWorkflowMessage.MoveToState, StringComparison.OrdinalIgnoreCase));
            
            var transitionEvaluationResult = await _transitionProcessorLazy.Value.Evaluate(stateExecutionContext, transitionConfiguration, cancellationToken).ConfigureAwait(false);
            if (transitionEvaluationResult.Status == TransitionEvaluationStatus.EvaluatedTrue)
            {
                await TransitionToState(workflowContext, stateConfiguration, transitionConfiguration, cancellationToken).ConfigureAwait(false);
                return new WorkflowProcessingResult(workflowInstance, workflowContext.WorkflowExecutionState);
            }

            Log.Debug("Skipping delayed transition message [{messageId}] for {workflowInstanceId}] because transition was evaluated to {state}",
                delayedTransitionWorkflowMessage.WorkflowMessageId, delayedTransitionWorkflowMessage.WorkflowInstanceId, transitionEvaluationResult.Status);

            return new WorkflowProcessingResult(workflowInstance, workflowContext.WorkflowExecutionState);
        }

        private async Task<WorkflowProcessingResult> ProcessMessageInternal(IWorkflowMessage workflowMessage, CancellationToken cancellationToken)
        {
            // TODO: this need to be removed
            Guard.ArgumentNotNull(workflowMessage, nameof(workflowMessage));
            Guard.ArgumentNotNull(workflowMessage.WorkflowInstanceId, nameof(workflowMessage.WorkflowInstanceId));
            Debug.Assert(workflowMessage.WorkflowInstanceId != null, "workflowMessage.WorkflowInstanceId != null");

            var workflowInstance = await GetWorkflowInstanceWithLock(workflowMessage.WorkflowInstanceId.Value, cancellationToken).ConfigureAwait(false);
            if (null == workflowInstance)
            {
                throw new WorkflowException($"Workflow message [{workflowMessage.WorkflowMessageId:D}] is referring to non existing workflow instance");
            }

            var workflowConfiguration = await GetWorkflowConfiguration(workflowInstance.WorkflowId, cancellationToken).ConfigureAwait(false);
            Guard.ArgumentNotNull(workflowConfiguration, nameof(workflowConfiguration));

            var stateConfiguration = workflowConfiguration.GetStateConfigurationByCode(workflowInstance.CurrentStateCode);
            Guard.ArgumentNotNull(stateConfiguration, nameof(stateConfiguration));

            var workflowContext = new WorkflowContext(this, WorkflowEngineBuilder, workflowInstance, workflowConfiguration);
            
            // ensure that domain entity is available for all/any coded logic for read-only purposes
            await TryGetDomainEntity(workflowContext, cancellationToken).ConfigureAwait(false);

            var messageExecutionContext = new MessageExecutionContext(workflowContext, workflowMessage);
            var messageExecutionResult = await _messageProcessorLazy.Value.ProcessMessage(messageExecutionContext, cancellationToken).ConfigureAwait(false);
            if (messageExecutionResult.Status == MessageExecutionStatus.Failed)
            {
                Log.Error("An error has occurred during processing message [{messageId}] for {workflowInstanceId}",
                    workflowMessage.WorkflowMessageId, workflowInstance.Id);

                await TransitionToState(workflowContext, workflowConfiguration.GetFailedStateConfiguration(), null, cancellationToken).ConfigureAwait(false);
                return new WorkflowProcessingResult(workflowInstance, workflowContext.WorkflowExecutionState);
            }

            if (messageExecutionResult.Status == MessageExecutionStatus.ContinueAfterAsyncTransition)
            {
                if (workflowInstance.CurrentStateProgress != StateExecutionProgress.AwaitingAsyncTransition)
                {
                    Log.Error("Can not process message [{messageId}] for {workflowInstanceId} because workflow instance is not in [{subState}]",
                        workflowMessage.WorkflowMessageId, workflowInstance.Id, StateExecutionProgress.AwaitingAsyncTransition);

                    await TransitionToState(workflowContext, workflowConfiguration.GetFailedStateConfiguration(), null, cancellationToken).ConfigureAwait(false);
                    return new WorkflowProcessingResult(workflowInstance, workflowContext.WorkflowExecutionState);
                }

                workflowContext.WorkflowInstance.CurrentStateProgress = StateExecutionProgress.AfterAsyncTransition;
                await ProcessState(workflowContext, stateConfiguration, cancellationToken).ConfigureAwait(false);
            }

            return new WorkflowProcessingResult(workflowInstance, workflowContext.WorkflowExecutionState);
        }
    }
}