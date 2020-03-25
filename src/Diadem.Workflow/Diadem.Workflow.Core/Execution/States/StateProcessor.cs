using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution.Activities;
using Diadem.Workflow.Core.Execution.Transitions;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.Runtime;
using Serilog;

namespace Diadem.Workflow.Core.Execution.States
{
    public class StateProcessor : IStateProcessor
    {
        private readonly IActivityProcessor _activityProcessor;

        private readonly ITransitionProcessor _transitionProcessor;

        private readonly WorkflowEngine _workflowEngine;

        public StateProcessor(WorkflowEngine workflowEngine)
        {
            _workflowEngine = workflowEngine;
            _activityProcessor = new ActivityProcessor(workflowEngine);
            _transitionProcessor = new TransitionProcessor(workflowEngine);
        }

        public async Task<StateExecutionResult> Process(StateExecutionContext stateExecutionContext, CancellationToken cancellationToken = default)
        {
            // 1. log starting
            // 2. listener --> before processing
            // 3. run activities
            //    3.1. execute all synchronous activities
            //    3.2. TODO: schedule all asynchronous activities
            // 4. listener --> after activities
            // 5. if state is final --> report and exit
            // 6. evaluate non-delayed transitions
            //    6.1. coded transitions
            //    6.2. compiled conditions
            // 7. schedule all delayed transitions
            // 8. log finishing

            IStateListener stateListener = null;
            StateExecutionResult stateExecutionResult = null;
            var workflowInstance = stateExecutionContext.WorkflowContext.WorkflowInstance;
            try
            {
                Log.Verbose("Starting processing state [{type}::{state}] [{workflowInstanceId}]", stateExecutionContext.StateConfiguration.Type,
                    stateExecutionContext.StateConfiguration.Code, stateExecutionContext.WorkflowContext.WorkflowInstance.Id);

                workflowInstance.CurrentStateCode = stateExecutionContext.StateConfiguration.Code;
                workflowInstance.CurrentStateProgress = StateExecutionProgress.Started;
                await _workflowEngine.SaveWorkflowInstance(workflowInstance, cancellationToken).ConfigureAwait(false);

                stateListener = GetStateListener(stateExecutionContext.WorkflowContext);
                workflowInstance.CurrentStateProgress = StateExecutionProgress.BeforeActivities;
                await InvokeListenerOnBeforeExecutingState(stateExecutionContext, stateListener, cancellationToken).ConfigureAwait(false);

                var activityExecutionResult = await ExecuteSynchronousActivities(stateExecutionContext, cancellationToken).ConfigureAwait(false);
                if (activityExecutionResult.Status.IsFailed())
                {
                    workflowInstance.CurrentStateProgress = StateExecutionProgress.Completed;
                    
                    if (!string.IsNullOrEmpty(activityExecutionResult.TransitionToUse))
                    {
                        var transitionConfiguration = stateExecutionContext.StateConfiguration.Transitions.FirstOrDefault(tc =>
                            string.Equals(tc.MoveToState, activityExecutionResult.TransitionToUse, StringComparison.OrdinalIgnoreCase));

                        if (null == transitionConfiguration)
                        {
                            throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                                "Cannot not find transition [{0}] for failed activity in status [{1}], [workflow ID={2:D}]",
                                activityExecutionResult.TransitionToUse,
                                stateExecutionContext.StateConfiguration.Code,
                                stateExecutionContext.WorkflowContext.WorkflowInstance.Id));
                        }
                        
                        Log.Verbose("[{type}::{state}] has been failed, transitioning to custom {failureState} [{workflowInstanceId}]",
                            stateExecutionContext.StateConfiguration.Type, stateExecutionContext.StateConfiguration.Code,
                            transitionConfiguration.MoveToState, stateExecutionContext.WorkflowContext.WorkflowInstance.Id);

                        // stateExecutionResult is being used later in the final block
                        stateExecutionResult = new StateExecutionResult(StateExecutionStatus.Failed, new StateExecutionTransition(StateTypeConfiguration.Application, transitionConfiguration));
                        return stateExecutionResult;
                    }
                    
                    Log.Verbose("[{type}::{state}] has been failed [{workflowInstanceId}]", stateExecutionContext.StateConfiguration.Type,
                        stateExecutionContext.StateConfiguration.Code, stateExecutionContext.WorkflowContext.WorkflowInstance.Id);
                    
                    // stateExecutionResult is being used later in the final block
                    stateExecutionResult = new StateExecutionResult(StateExecutionStatus.Failed, new StateExecutionTransition(StateTypeConfiguration.Failed, null));
                    return stateExecutionResult;
                }

                workflowInstance.CurrentStateProgress = StateExecutionProgress.AfterActivitiesProcessed;
                await _workflowEngine.SaveWorkflowInstance(workflowInstance, cancellationToken).ConfigureAwait(false);
                await InvokeListenerOnAfterExecutionActivities(stateExecutionContext, stateListener, cancellationToken).ConfigureAwait(false);

                if (stateExecutionContext.StateConfiguration.Type == StateTypeConfiguration.Failed ||
                    stateExecutionContext.StateConfiguration.Type == StateTypeConfiguration.Final)
                {
                    Log.Verbose("[{type}::{state}] has been completed [{workflowInstanceId}]", stateExecutionContext.StateConfiguration.Type,
                        stateExecutionContext.StateConfiguration.Code, stateExecutionContext.WorkflowContext.WorkflowInstance.Id);

                    workflowInstance.CurrentStateProgress = StateExecutionProgress.Completed;
                    await _workflowEngine.SaveWorkflowInstance(workflowInstance, cancellationToken).ConfigureAwait(false);

                    // stateExecutionResult is being used later in the final block
                    stateExecutionResult = new StateExecutionResult(StateExecutionStatus.Finished, new StateExecutionTransition(StateTypeConfiguration.Undefined, null));
                    return stateExecutionResult;
                }

                workflowInstance.CurrentStateProgress = StateExecutionProgress.BeforeTransitions;
                await _workflowEngine.SaveWorkflowInstance(workflowInstance, cancellationToken).ConfigureAwait(false);

                var stateExecutionTransition = await EvaluateNonDelayTransitions(stateExecutionContext, cancellationToken).ConfigureAwait(false);
                var delayedStateExecutionTransitions = GetDelayedTransitions(stateExecutionContext);

                if (stateExecutionTransition.NextStateType == StateTypeConfiguration.Undefined && null == delayedStateExecutionTransitions)
                {
                    throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                        "Cannot evaluate transition(s) for non final state [{0}], [workflow ID={1:D}]",
                        stateExecutionContext.StateConfiguration.Code,
                        stateExecutionContext.WorkflowContext.WorkflowInstance.Id));
                }

                if (null != delayedStateExecutionTransitions)
                {
                    await ScheduleDelayedTransitions(stateExecutionContext, delayedStateExecutionTransitions, cancellationToken)
                        .ConfigureAwait(false);
                }

                workflowInstance.CurrentStateProgress = StateExecutionProgress.Completed;
                await _workflowEngine.SaveWorkflowInstance(workflowInstance, cancellationToken).ConfigureAwait(false);

                stateExecutionResult = new StateExecutionResult(StateExecutionStatus.Completed, stateExecutionTransition);
                return stateExecutionResult;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred during processing state [{type}::{state}] [{workflowInstanceId}]",
                    stateExecutionContext.StateConfiguration.Type, stateExecutionContext.StateConfiguration.Code, stateExecutionContext.WorkflowContext.WorkflowInstance.Id);

                workflowInstance.CurrentStateProgress = StateExecutionProgress.Completed;
                if (stateExecutionContext.StateConfiguration.Type == StateTypeConfiguration.Failed)
                {
                    stateExecutionResult = new StateExecutionResult(StateExecutionStatus.Failed, new StateExecutionTransition(StateTypeConfiguration.Undefined, null));
                    await _workflowEngine.SaveWorkflowInstance(workflowInstance, cancellationToken).ConfigureAwait(false);
                    return stateExecutionResult;
                }

                stateExecutionResult = new StateExecutionResult(StateExecutionStatus.Failed, new StateExecutionTransition(StateTypeConfiguration.Failed, null));
                await _workflowEngine.SaveWorkflowInstance(workflowInstance, cancellationToken).ConfigureAwait(false);
                return stateExecutionResult;
            }
            finally
            {
                await InvokeListenerOnAfterExecutionState(stateExecutionContext, stateListener, stateExecutionResult, cancellationToken).ConfigureAwait(false);

                Log.Debug("Finished processing state [{type}::{code}], {status}, {nextState} [{workflowInstanceId}]",
                    stateExecutionContext.StateConfiguration.Type, stateExecutionContext.StateConfiguration.Code,
                    stateExecutionResult?.Status, stateExecutionResult?.Transition.TransitionConfiguration?.Code, stateExecutionContext.WorkflowContext.WorkflowInstance.Id);
            }
        }

        private static IList<StateExecutionTransition> GetDelayedTransitions(StateExecutionContext stateExecutionContext)
        {
            var delayedTransitions = stateExecutionContext.StateConfiguration.Transitions
                .Where(t => t.Type == TransitionTypeConfiguration.AsynchronousWithDelay)
                .ToList();

            if (delayedTransitions.Count == 0)
            {
                return null;
            }

            var stateExecutionTransitions = new List<StateExecutionTransition>(delayedTransitions.Count);
            foreach (var transitionConfiguration in delayedTransitions)
            {
                stateExecutionTransitions.Add(new StateExecutionTransition(StateTypeConfiguration.Application, transitionConfiguration));
            }

            return stateExecutionTransitions;
        }

        private async Task ScheduleDelayedTransitions(StateExecutionContext stateExecutionContext,
            IEnumerable<StateExecutionTransition> delayedTransitions, CancellationToken cancellationToken)
        {
            foreach (var delayedTransition in delayedTransitions)
            {
                var delay = TimeSpan.FromSeconds(delayedTransition.TransitionConfiguration.Delay.GetValueOrDefault());
                var workflowMessage = new DelayedTransitionWorkflowMessage(
                    stateExecutionContext.WorkflowContext.WorkflowConfiguration.Id, stateExecutionContext.WorkflowContext.WorkflowInstance.Id, delay,
                    stateExecutionContext.StateConfiguration.Code, delayedTransition.TransitionConfiguration.MoveToState);

                var endpointConfiguration = stateExecutionContext.WorkflowContext.WorkflowConfiguration.RuntimeConfiguration.EndpointConfiguration;
                await _workflowEngine.WorkflowEngineBuilder.WorkflowMessageTransportFactoryProvider
                    .CreateMessageTransportFactory(endpointConfiguration.Type)
                    .CreateMessageTransport(endpointConfiguration.Address)
                    .SendWithDelay(endpointConfiguration, workflowMessage, cancellationToken)
                    .ConfigureAwait(false);

                Log.Debug("Scheduled delayed transition for {workflowInstanceId} to {state}",
                    stateExecutionContext.WorkflowContext.WorkflowInstance.Id, delayedTransition.TransitionConfiguration.MoveToState);
            }
        }

        private async Task<StateExecutionTransition> EvaluateNonDelayTransitions(StateExecutionContext stateExecutionContext, CancellationToken cancellationToken)
        {
            // evaluate transitions in the same order they came from configuration  
            var nonDelayTransitions = stateExecutionContext.StateConfiguration.Transitions
                .Where(t => t.Type != TransitionTypeConfiguration.AsynchronousWithDelay)
                .ToList();

            if (nonDelayTransitions.Any())
            {
                foreach (var transitionConfiguration in nonDelayTransitions)
                {
                    var transitionEvaluationResult = await _transitionProcessor.Evaluate(stateExecutionContext, transitionConfiguration, cancellationToken).ConfigureAwait(false);
                    if (transitionEvaluationResult.Status == TransitionEvaluationStatus.EvaluatedTrue)
                    {
                        // first evaluated to true transition wins
                        return new StateExecutionTransition(StateTypeConfiguration.Application, transitionConfiguration);
                    }
                }
            }
            else
            {
                Log.Verbose("State [{state}] does not have any synchronous activity to execute [{workflowInstanceId}]",
                    stateExecutionContext.StateConfiguration.Code, stateExecutionContext.WorkflowContext.WorkflowInstance.Id);
            }

            return new StateExecutionTransition(StateTypeConfiguration.Undefined, null);
        }

        private async Task<ActivityExecutionResult> ExecuteSynchronousActivities(StateExecutionContext stateExecutionContext, CancellationToken cancellationToken = default)
        {
            // take only synchronous activities in order they are defined in configuration
            var synchronousActivities = stateExecutionContext.StateConfiguration.Activities
                .Where(a => a.Type == ActivityTypeConfiguration.Synchronous)
                .ToList();

            if (synchronousActivities.Any())
            {
                foreach (var activityConfiguration in synchronousActivities)
                {
                    var activityExecutionContext = new ActivityExecutionContext(
                        stateExecutionContext, activityConfiguration);

                    var activityExecutionResult = await _activityProcessor.Process(activityExecutionContext, cancellationToken).ConfigureAwait(false);
                    if (activityExecutionResult.Status == ActivityExecutionStatus.Failed || activityExecutionResult.Status == ActivityExecutionStatus.FailedNoRetry)
                    {
                        return activityExecutionResult;
                    }
                }
            }
            else
            {
                Log.Verbose("State [{state}] does not have any synchronous activity to execute [{workflowInstanceId}]",
                    stateExecutionContext.StateConfiguration.Code, stateExecutionContext.WorkflowContext.WorkflowInstance.Id);
            }

            return new ActivityExecutionResult(ActivityExecutionStatus.Completed);
        }

        private static async Task InvokeListenerOnBeforeExecutingState(StateExecutionContext stateExecutionContext, IStateListener stateListener, CancellationToken cancellationToken = default)
        {
            if (null == stateListener)
            {
                return;
            }

            Log.Verbose("State processing {state} listener [before activities] [{workflowInstanceId}]",
                stateExecutionContext.StateConfiguration.Code, stateExecutionContext.WorkflowContext.WorkflowInstance.Id);

            try
            {
                await stateListener.BeforeExecutingState(stateExecutionContext, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                    "An error has occurred during execution of an state [{0}] listener [before activities] for workflow instance [{1}]",
                    stateExecutionContext.StateConfiguration.Code,
                    stateExecutionContext.WorkflowContext.WorkflowInstance.Id), ex);
            }
        }

        private static async Task InvokeListenerOnAfterExecutionActivities(StateExecutionContext stateExecutionContext, IStateListener stateListener, CancellationToken cancellationToken = default)
        {
            if (null == stateListener)
            {
                return;
            }

            Log.Verbose("State processing [{state}] listener [after activities] [{workflowInstanceId}]",
                stateExecutionContext.StateConfiguration.Code, stateExecutionContext.WorkflowContext.WorkflowInstance.Id);

            try
            {
                await stateListener.AfterExecutionActivities(stateExecutionContext, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                    "An error has occurred during execution of an state [{0}] listener [after activities] for workflow instance [{1}]",
                    stateExecutionContext.StateConfiguration.Code,
                    stateExecutionContext.WorkflowContext.WorkflowInstance.Id), ex);
            }
        }

        private static async Task InvokeListenerOnAfterExecutionState(StateExecutionContext stateExecutionContext,
            IStateListener stateListener, StateExecutionResult stateExecutionResult, CancellationToken cancellationToken = default)
        {
            if (null == stateListener)
            {
                return;
            }

            Log.Verbose("State has been processed [{state}] listener [after state] [{workflowInstanceID}]",
                stateExecutionContext.StateConfiguration.Code, stateExecutionContext.WorkflowContext.WorkflowInstance.Id);

            try
            {
                await stateListener.AfterExecutionState(stateExecutionContext, stateExecutionResult, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                    "An error has occurred during execution of an state [{0}] listener [after state] for workflow instance [{1}]",
                    stateExecutionContext.StateConfiguration.Code,
                    stateExecutionContext.WorkflowContext.WorkflowInstance.Id), ex);
            }
        }

        private IStateListener GetStateListener(WorkflowContext workflowContext)
        {
            return _workflowEngine.WorkflowEngineBuilder.ListenerFactory?.CreateStateListener(
                workflowContext.WorkflowConfiguration.Class, workflowContext.WorkflowConfiguration.Id);
        }
    }
}