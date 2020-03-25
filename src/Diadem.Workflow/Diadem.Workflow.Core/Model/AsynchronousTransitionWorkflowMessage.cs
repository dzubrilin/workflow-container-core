using System;

namespace Diadem.Workflow.Core.Model
{
    public class AsynchronousTransitionWorkflowMessage : WorkflowMessage, IAsynchronousTransitionWorkflowMessage
    {
        public AsynchronousTransitionWorkflowMessage(Guid workflowId, Guid? workflowInstanceId) : base(workflowId, workflowInstanceId)
        {
        }

        public AsynchronousTransitionWorkflowMessage(Guid workflowId, Guid? workflowInstanceId, string jsonState) : base(workflowId, workflowInstanceId, jsonState)
        {
        }
    }
}