using System;
using System.Linq;
using System.Text.RegularExpressions;
using Diadem.Core;
using Diadem.Core.Configuration;

namespace Diadem.Workflow.Core.Configuration
{
    internal static class WorkflowConfigurationExtensions
    {
        internal static IEndpointConfiguration FindEndpointConfiguration(this WorkflowConfiguration workflowConfiguration, string code)
        {
            Guard.ArgumentNotNull(workflowConfiguration, nameof(workflowConfiguration));
            Guard.ArgumentNotNull(workflowConfiguration.RuntimeConfiguration, nameof(workflowConfiguration.RuntimeConfiguration));
            Guard.ArgumentNotNull(workflowConfiguration.RuntimeConfiguration.EndpointConfigurations, nameof(workflowConfiguration.RuntimeConfiguration.EndpointConfigurations));

            foreach (var endpointConfigurationKvp in workflowConfiguration.RuntimeConfiguration.EndpointConfigurations)
            {
                var regex = new Regex(endpointConfigurationKvp.Key, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (regex.Match(code).Success)
                {
                    return endpointConfigurationKvp.Value;
                }
            }

            throw new WorkflowException($"Endpoint configuration for code [${code}] was not found for workflow [ID=${workflowConfiguration.Id}]");
        }

        internal static string GetRemoteCode(this string code)
        {
            Guard.ArgumentNotNull(code, nameof(code));

            var openIndex = code.LastIndexOf('{');
            var closeIndex = code.LastIndexOf('}');

            if (openIndex == -1 || closeIndex == -1)
            {
                throw new WorkflowException($"Cannot find remote code in [{code}]");
            }

            return code.Substring(openIndex + 1, closeIndex - openIndex - 1);
        }

        internal static StateConfiguration GetFailedStateConfiguration(this WorkflowConfiguration workflowConfiguration)
        {
            return workflowConfiguration.States.Single(s => s.Type == StateTypeConfiguration.Failed);
        }

        internal static EventConfiguration GetEventConfigurationByCode(this WorkflowConfiguration workflowConfiguration, string code)
        {
            return workflowConfiguration.Events.FirstOrDefault(e => string.Equals(code, e.Code, StringComparison.OrdinalIgnoreCase));
        }

        internal static EventConfiguration GetEventConfigurationByType(this WorkflowConfiguration workflowConfiguration, EventTypeConfiguration eventType)
        {
            return workflowConfiguration.Events.FirstOrDefault(e => e.Type == eventType);
        }

        internal static StateConfiguration GetStateConfigurationByCode(this WorkflowConfiguration workflowConfiguration, string code)
        {
            var stateConfiguration = workflowConfiguration.States.SingleOrDefault(s => string.Equals(s.Code, code, StringComparison.OrdinalIgnoreCase));
            if (null == stateConfiguration)
            {
                throw new WorkflowException($"State [{code}] was not found in workflow [{workflowConfiguration.Code}]");
            }

            return stateConfiguration;
        }

        internal static StateConfiguration GetStateConfigurationByType(this WorkflowConfiguration workflowConfiguration, StateTypeConfiguration stateType)
        {
            return workflowConfiguration.States.Single(s => s.Type == stateType);
        }

        internal static bool IsFailed(this EventTypeConfiguration eventTypeConfiguration)
        {
            switch (eventTypeConfiguration)
            {
                case EventTypeConfiguration.NestedToParentFailed:
                case EventTypeConfiguration.ParentToNestedFailed:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool In(this TransitionTypeConfiguration transitionTypeConfiguration, TransitionTypeConfiguration value1, TransitionTypeConfiguration value2)
        {
            return transitionTypeConfiguration == value1 || transitionTypeConfiguration == value2;
        }

        internal static bool HasScriptWithEntityUse(this WorkflowConfiguration workflowConfiguration)
        {
            var hasActivityScriptWithEntityUse = workflowConfiguration.States
                .SelectMany(s => s.Activities).Any(a => !string.IsNullOrEmpty(a.Script) && a.Script.Contains("entity"));
            if (hasActivityScriptWithEntityUse)
            {
                return true;
            }

            var hasEventHandlerScriptWithEntityUse = workflowConfiguration.States
                .SelectMany(s => s.Events).Any(e => !string.IsNullOrEmpty(e.Script) && e.Script.Contains("entity"));
            if (hasEventHandlerScriptWithEntityUse)
            {
                return true;
            }

            var hasTransitionScriptWithEntityUse = workflowConfiguration.States
                .SelectMany(s => s.Transitions).Any(t => !string.IsNullOrEmpty(t.ConditionScript) && t.ConditionScript.Contains("entity"));
            if (hasTransitionScriptWithEntityUse)
            {
                return true;
            }

            return false;
        }
    }
}