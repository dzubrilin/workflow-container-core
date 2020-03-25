using System;

namespace Diadem.Workflow.Core.Model
{
    public class ActivityRequestWorkflowMessage : WorkflowMessage, IActivityRequestWorkflowMessage
    {
        public ActivityRequestWorkflowMessage(string code)
        {
            Code = code;
        }

        public ActivityRequestWorkflowMessage(Guid workflowId, Guid? workflowInstanceId, string code, string jsonState) : base(workflowId, workflowInstanceId, jsonState)
        {
            Code = code;
        }

        public string Code { get; }
    }
}