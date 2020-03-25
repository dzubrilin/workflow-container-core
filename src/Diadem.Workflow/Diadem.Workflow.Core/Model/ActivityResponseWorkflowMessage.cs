using System;
using Diadem.Workflow.Core.Execution.Activities;

namespace Diadem.Workflow.Core.Model
{
    public class ActivityResponseWorkflowMessage : WorkflowMessage, IActivityResponseWorkflowMessage
    {
        public ActivityResponseWorkflowMessage()
        {
        }

        public ActivityResponseWorkflowMessage(Guid workflowId, Guid? workflowInstanceId) : base(workflowId, workflowInstanceId)
        {
        }

        public ActivityResponseWorkflowMessage(Guid workflowId, Guid? workflowInstanceId, string jsonState) : base(workflowId, workflowInstanceId, jsonState)
        {
        }

        public ActivityExecutionResult ActivityExecutionResult { get; set; }
    }
}