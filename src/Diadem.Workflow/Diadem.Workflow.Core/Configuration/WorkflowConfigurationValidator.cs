using System;
using System.Linq;

namespace Diadem.Workflow.Core.Configuration
{
    internal static class WorkflowConfigurationValidator
    {
        /// <summary>
        ///     Validate workflow configuration against a number of invariants
        /// </summary>
        internal static void ValidateWorkflowConfiguration(WorkflowConfiguration workflowConfiguration)
        {
            WorkflowConfigurationException workflowConfigurationException = null;

            // 1. make sure that workflow has exactly one initial state
            var initialStateCount = workflowConfiguration.States.Count(s => s.Type == StateTypeConfiguration.Initial);
            if (initialStateCount != 1)
            {
                AddValidationMessage(ref workflowConfigurationException, $"Workflow [{workflowConfiguration.Code}] must have exactly one initial state");
            }

            // 2. make sure that workflow has exactly one failed state
            var failedStateCount = workflowConfiguration.States.Count(s => s.Type == StateTypeConfiguration.Initial);
            if (failedStateCount != 1)
            {
                AddValidationMessage(ref workflowConfigurationException, $"Workflow [{workflowConfiguration.Code}] must have exactly one failed state");
            }

            // 3. there are no state code duplicates
            var hasDuplicateState = workflowConfiguration.States
                .Any(s => workflowConfiguration.States.Any(si => si != s && string.Equals(si.Code, s.Code, StringComparison.OrdinalIgnoreCase)));
            if (hasDuplicateState)
            {
                AddValidationMessage(ref workflowConfigurationException, $"Workflow [{workflowConfiguration.Code}] must have unique states");
            }

            // 4. make sure that workflow has zero or exactly one parentToNestedInitial event
            var parentToNestedInitialEventCount = workflowConfiguration.Events.Count(e => e.Type == EventTypeConfiguration.ParentToNestedInitial);
            if (parentToNestedInitialEventCount > 1)
            {
                AddValidationMessage(ref workflowConfigurationException,
                    $"Workflow [{workflowConfiguration.Code}] must have zero or exactly one [{EventTypeConfiguration.ParentToNestedInitial:G}] event");
            }

            // 5. there are no event code duplicates
            var hasDuplicateEvents = workflowConfiguration.Events
                .Any(e => workflowConfiguration.Events.Any(ei => ei != e && string.Equals(ei.Code, e.Code, StringComparison.OrdinalIgnoreCase)));
            if (hasDuplicateEvents)
            {
                AddValidationMessage(ref workflowConfigurationException, $"Workflow [{workflowConfiguration.Code}] must have unique events");
            }

            // 6. do not allow state to have events and incoming asynchronousImmediate transitions simultaneously
            var statesWithEvents = workflowConfiguration.States.Where(s => s.Events.Any()).ToList();
            var transitionsToStateWithEvents = workflowConfiguration.States.SelectMany(s => s.Transitions)
                .Where(t => t.Type == TransitionTypeConfiguration.AsynchronousImmediate)
                .Where(t => statesWithEvents.Any(s => s.Code == t.MoveToState));
            if (transitionsToStateWithEvents.Any())
            {
                AddValidationMessage(ref workflowConfigurationException,
                    $"Workflow [{workflowConfiguration.Code}] has state(s) with events and incoming async transitions");
            }
            
            // 7. do not allow duplicated event declaration for the same state
            var hasStatesWithDuplicatedEvent = statesWithEvents
                .Any(s => s.Events.Any(e => s.Events
                    .Any(ei => ei != e && string.Equals(ei.Code, e.Code, StringComparison.OrdinalIgnoreCase))));
            if (hasStatesWithDuplicatedEvent)
            {
                AddValidationMessage(ref workflowConfigurationException, $"Workflow [{workflowConfiguration.Code}] has state(s) with duplicated events");
            }
            
            // 8. make sure that activity -> retryPolicy -> onFailureTransitionToUse refers to an existing transition within the state
            var statesWithCustomFailureTransitions = workflowConfiguration.States
                .Where(s => s.Activities.Any(a => !string.IsNullOrEmpty(a.RetryPolicy.OnFailureTransitionToUse))).ToList();
            if (statesWithCustomFailureTransitions.Any())
            {
                var hasMissConfiguredCustomFailureTransitions = statesWithCustomFailureTransitions
                    .Any(s => s.Activities
                        .Any(a => !string.IsNullOrEmpty(a.RetryPolicy.OnFailureTransitionToUse) && !s.Transitions
                            .Any(t => string.Equals(t.MoveToState, a.RetryPolicy.OnFailureTransitionToUse))));
                if (hasMissConfiguredCustomFailureTransitions)
                {
                    AddValidationMessage(ref workflowConfigurationException,
                        $"Workflow [{workflowConfiguration.Code}] has activity(s) with configured 'onFailureTransitionToUse' but state has no such transition");
                }
            }

            if (null != workflowConfigurationException)
            {
                throw workflowConfigurationException;
            }
        }

        private static void AddValidationMessage(ref WorkflowConfigurationException workflowConfigurationException, string validationMessage)
        {
            if (null == workflowConfigurationException)
            {
                workflowConfigurationException = new WorkflowConfigurationException();
            }

            workflowConfigurationException.ValidationMessages.Add(validationMessage);
        }
    }
}