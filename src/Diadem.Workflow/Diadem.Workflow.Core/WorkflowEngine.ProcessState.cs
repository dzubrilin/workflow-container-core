using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution;
using Diadem.Workflow.Core.Execution.States;
using Diadem.Workflow.Core.Model;
using Serilog;

namespace Diadem.Workflow.Core
{
    public partial class WorkflowEngine
    {
        private async Task ProcessState(WorkflowContext workflowContext, StateConfiguration stateConfiguration, CancellationToken cancellationToken = default)
        {
            if (stateConfiguration.Type == StateTypeConfiguration.Initial || stateConfiguration.Type == StateTypeConfiguration.Application)
            {
                var cycleDetected = DetectCycle(workflowContext, stateConfiguration);
                if (cycleDetected)
                {
                    await TransitionToState(workflowContext, workflowContext.WorkflowConfiguration.GetFailedStateConfiguration(), null, cancellationToken)
                       .ConfigureAwait(false);
                    Log.Warning("Cycle has been detected for {workflowInstanceId} moving to failed state", workflowContext.WorkflowInstance.Id);

                    return;
                }
            }

            StateExecutionResult stateExecutionResult;
            var stateExecutionContext = new StateExecutionContext(workflowContext, stateConfiguration);

            var started = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                stateExecutionResult = await _stateProcessorLazy.Value.Process(stateExecutionContext, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                stopwatch.Stop();
                Log.Debug("Finished execution of {state}, {duration}, {workflowInstanceId}",
                    stateConfiguration.Code, stopwatch.Elapsed, workflowContext.WorkflowInstance.Id);

                var workflowInstanceStateLog = new WorkflowInstanceStateLog(workflowContext.WorkflowInstance.Id, stateConfiguration.Code, started, (int) stopwatch.ElapsedMilliseconds);
                await WorkflowEngineBuilder.WorkflowStore.SaveWorkflowInstanceStateLog(workflowInstanceStateLog, cancellationToken).ConfigureAwait(false);
            }

            if (stateExecutionResult.Status == StateExecutionStatus.Finished)
            {
                Log.Debug("{workflowInstanceId} has been finished processing", workflowContext.WorkflowInstance.Id);

                // if (stateExecutionContext.StateConfiguration.Type == StateTypeConfiguration.Final)
                // {
                //    await PulseFinalizedToParentWorkflow(workflowContext, cancellationToken).ConfigureAwait(false);
                // }
                // else
                if (stateExecutionContext.StateConfiguration.Type == StateTypeConfiguration.Failed)
                {
                    await PulseFailedToParentWorkflow(workflowContext, cancellationToken).ConfigureAwait(false);
                    await PulseFailedToNestedWorkflows(workflowContext, cancellationToken).ConfigureAwait(false);
                }

                return;
            }

            if (stateExecutionResult.Status == StateExecutionStatus.Failed || stateExecutionResult.Status == StateExecutionStatus.Undefined)
            {
                // check if current state is already 'Failed' then just ignore state activity execution failure
                if (stateConfiguration.Type == StateTypeConfiguration.Failed)
                {
                    Log.Error("Activities execution error for failed state of {workflowInstanceId} has occurred, ignoring failure...",
                        workflowContext.WorkflowInstance.Id);
                    return;
                }

                // state/activity failure could have custom transition to use in case of failure
                if (null != stateExecutionResult.Transition.TransitionConfiguration)
                {
                    await TransitionAfterStateExecution(workflowContext, stateExecutionResult, cancellationToken).ConfigureAwait(false);
                    Log.Verbose("{workflowInstanceId} has been moved to {state}",
                        workflowContext.WorkflowInstance.Id, stateExecutionResult.Transition.TransitionConfiguration.MoveToState);
                    return;
                }
                
                await TransitionToState(workflowContext, workflowContext.WorkflowConfiguration.GetFailedStateConfiguration(), null, cancellationToken)
                   .ConfigureAwait(false);
                Log.Verbose("{workflowInstanceId} has been moved to failed state", workflowContext.WorkflowInstance.Id);
                return;
            }

            if (stateExecutionResult.Status == StateExecutionStatus.Completed)
            {
                await TransitionAfterStateExecution(workflowContext, stateExecutionResult, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task TransitionAfterStateExecution(WorkflowContext workflowContext, StateExecutionResult stateExecutionResult, CancellationToken cancellationToken)
        {
            if (stateExecutionResult.Transition.NextStateType == StateTypeConfiguration.Application)
            {
                var nextStateConfiguration = workflowContext.WorkflowConfiguration
                    .GetStateConfigurationByCode(stateExecutionResult.Transition.TransitionConfiguration.MoveToState);

                if (null == nextStateConfiguration)
                {
                    throw new WorkflowException(string.Format(CultureInfo.InvariantCulture, "Workflow [ID={0:D}] can not be moved to state [{1}]",
                        workflowContext.WorkflowInstance.Id, stateExecutionResult.Transition.TransitionConfiguration.MoveToState));
                }

                await TransitionToState(workflowContext, nextStateConfiguration, stateExecutionResult.Transition.TransitionConfiguration, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private async Task TransitionToState(WorkflowContext workflowContext, StateConfiguration stateConfiguration,
            TransitionConfiguration transitionConfiguration, CancellationToken cancellationToken = default)
        {
            // move to failed state with no additional checks in synchronous fashion
            if (stateConfiguration.Type == StateTypeConfiguration.Failed)
            {
                await ProcessState(workflowContext, stateConfiguration, cancellationToken).ConfigureAwait(false);
                return;
            }

            // in case if transition is trying to been done to the same state where workflow instance is ==> move to failed
            if (string.Equals(workflowContext.WorkflowInstance.CurrentStateCode, stateConfiguration.Code, StringComparison.OrdinalIgnoreCase))
            {
                Log.Warning("{workflowInstanceId} is attempted to be moved to the same state as it is now [{state}], moving it to failed state",
                    workflowContext.WorkflowInstance.Id, stateConfiguration.Code);

                await ProcessState(workflowContext, workflowContext.WorkflowConfiguration.GetFailedStateConfiguration(), cancellationToken).ConfigureAwait(false);
                return;
            }

            // if state has any event ==> move to awaiting event sub-state
            if (stateConfiguration.Events.Any())
            {
                // [assumption] state cannot have both event and incoming async transition (this check is done during WF configuration loading)
                workflowContext.WorkflowInstance.CurrentStateCode = stateConfiguration.Code;
                workflowContext.WorkflowInstance.CurrentStateProgress = StateExecutionProgress.AwaitingEvent;
                await SaveWorkflowInstance(workflowContext.WorkflowInstance, cancellationToken).ConfigureAwait(false);

                Log.Debug("{workflowInstanceId} has been moved to [{state}::{subState}]",
                    workflowContext.WorkflowInstance.Id, stateConfiguration.Code, StateExecutionProgress.AwaitingEvent);

                return;
            }

            // process synchronous / delayed transition
            if (transitionConfiguration.Type.In(TransitionTypeConfiguration.Synchronous, TransitionTypeConfiguration.AsynchronousWithDelay))
            {
                // save current transition as a checkpoint
                workflowContext.WorkflowInstance.CurrentStateCode = stateConfiguration.Code;
                workflowContext.WorkflowInstance.CurrentStateProgress = StateExecutionProgress.Started;
                await SaveWorkflowInstance(workflowContext.WorkflowInstance, cancellationToken).ConfigureAwait(false);

                Log.Debug("{workflowInstanceId} is moved to [{state}::{subState}]",
                    workflowContext.WorkflowInstance.Id, stateConfiguration.Code, StateExecutionProgress.Started);

                await ProcessState(workflowContext, stateConfiguration, cancellationToken).ConfigureAwait(false);
                return;
            }

            // process asynchronous-immediate transition
            if (transitionConfiguration.Type == TransitionTypeConfiguration.AsynchronousImmediate)
            {
                workflowContext.WorkflowInstance.CurrentStateCode = stateConfiguration.Code;
                workflowContext.WorkflowInstance.CurrentStateProgress = StateExecutionProgress.AwaitingAsyncTransition;
                await SaveWorkflowInstance(workflowContext.WorkflowInstance, cancellationToken).ConfigureAwait(false);

                var workflowMessage = new AsynchronousTransitionWorkflowMessage(workflowContext.WorkflowConfiguration.Id, workflowContext.WorkflowInstance.Id);
                await WorkflowEngineBuilder.WorkflowMessageTransportFactoryProvider
                                           .CreateMessageTransportFactory(workflowContext.WorkflowConfiguration.RuntimeConfiguration.EndpointConfiguration.Type)
                                           .CreateMessageTransport(workflowContext.WorkflowConfiguration.RuntimeConfiguration.EndpointConfiguration.Address)
                                           .Send(workflowContext.WorkflowConfiguration.RuntimeConfiguration.EndpointConfiguration, workflowMessage, cancellationToken)
                                           .ConfigureAwait(false);

                // TODO: make sure that asynchronous-immediate transition will not have race conditions with current workflow engine
                //    ==> send them as delayed workflow messages with default delay interval (1 second)
//                var delay = TimeSpan.FromSeconds(1D);
//                var currentStateCode = workflowContext.WorkflowInstance.CurrentStateCode;
//                var workflowMessage = new DelayedTransitionWorkflowMessage(workflowContext.WorkflowConfiguration.Id,
//                    workflowContext.WorkflowInstance.Id, delay, currentStateCode, transitionConfiguration.MoveToState);
//                await WorkflowEngineBuilder.WorkflowMessageTransportFactory
//                    .CreateMessageTransport(workflowContext.WorkflowConfiguration.RuntimeConfiguration.EndpointConfiguration.Address)
//                    .SendWithDelay(workflowContext.WorkflowConfiguration.RuntimeConfiguration.EndpointConfiguration, workflowMessage, cancellationToken).ConfigureAwait(false);

                Log.Debug("{workflowInstanceId} has been moved to [{state}::{subState}]",
                    workflowContext.WorkflowInstance.Id, stateConfiguration.Code, StateExecutionProgress.AwaitingAsyncTransition);

                return;
            }

            throw new WorkflowException("Non synchronous/asynchronous-immediate transitions are not supported");
        }

        private static bool DetectCycle(WorkflowContext workflowContext, StateConfiguration stateConfiguration)
        {
            // try detecting cycle (repetitive re-entrance into the same state)
            // use the following rules to detect the cycle:
            //    1. every time workflow engine calls [ProcessState] --> add state code into workflow instance state visit chain
            //    2. workflow instance is coming into the same state for the third time
            //    3. sequence of intermediate steps between 1st-to-2nd and 2nd-to-3rd re-entrances matches
            var workflowInstanceInternal = (IWorkflowInstanceInternal) workflowContext.WorkflowInstance;
            workflowInstanceInternal.VisitedStatesInternal.Add(stateConfiguration.Code);
            if (workflowInstanceInternal.VisitedStatesInternal.Count == 1)
            {
                return false;
            }

            var lastVisitIndex = -1;
            var nextToLastVisitIndex = -1;
            for (var i = workflowInstanceInternal.VisitedStatesInternal.Count - 2; i >= 0; i--)
            {
                if (!string.Equals(workflowInstanceInternal.VisitedStatesInternal[i], stateConfiguration.Code, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (lastVisitIndex == -1)
                {
                    lastVisitIndex = i;
                }
                else
                {
                    nextToLastVisitIndex = i;
                    break;
                }
            }

            if (lastVisitIndex == -1 || nextToLastVisitIndex == -1)
            {
                return false;
            }

            var firstCycleLength = lastVisitIndex - nextToLastVisitIndex;
            var secondCycleLength = workflowInstanceInternal.VisitedStatesInternal.Count - lastVisitIndex - 1;
            if (firstCycleLength != secondCycleLength)
            {
                return false;
            }

            for (var i = 0; i < firstCycleLength; i++)
            {
                if (!string.Equals(
                    workflowInstanceInternal.VisitedStatesInternal[lastVisitIndex + i],
                    workflowInstanceInternal.VisitedStatesInternal[nextToLastVisitIndex + i],
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}