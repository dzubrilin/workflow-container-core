using System.Collections.Generic;
using System.Diagnostics;
using Diadem.Core;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Model;

namespace Diadem.Workflow.Core.Execution
{
    internal static class WorkflowContextExtensions
    {
        private static readonly IList<string> LocalStatePropertyNames = new List<string>(new[] {"Code", "EntityId", "EntityType"});

        internal static JsonState CreateRemoteExecutionState(this WorkflowContext workflowContext, string parameters, string remoteCode)
        {
            Guard.ArgumentNotNull(workflowContext, nameof(workflowContext));
            Guard.ArgumentNotNull(workflowContext.WorkflowInstance, nameof(workflowContext.WorkflowInstance));

            var jsonState = new JsonState();
            jsonState.SetProperty("Code", remoteCode);
            jsonState.SetProperty("EntityId", workflowContext.WorkflowInstance.EntityId);
            jsonState.SetProperty("EntityType", workflowContext.WorkflowInstance.EntityType);

            if (!string.IsNullOrEmpty(parameters))
            {
                var jsonParameters = new JsonState(parameters);
                jsonState.MergeExcept(jsonParameters, LocalStatePropertyNames);
            }

            // make sure that workflow context state is available for the remote activity during execution
            jsonState.MergeExcept(workflowContext.WorkflowExecutionState, LocalStatePropertyNames);
            return jsonState;
        }

        internal static void MergeRemoteExecutionState(this WorkflowContext workflowContext, IWorkflowMessageState workflowMessageState)
        {
            if (null == workflowMessageState || string.IsNullOrEmpty(workflowMessageState.JsonState))
            {
                return;
            }

            var responseJsonState = new JsonState(workflowMessageState.JsonState);
            workflowContext.WorkflowExecutionState.MergeExcept(responseJsonState, LocalStatePropertyNames);
        }
    }
}