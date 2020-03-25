using System;
using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Core.Model
{
    public abstract class WorkflowMessage : IWorkflowMessage
    {
        protected WorkflowMessage()
        {
        }

        protected WorkflowMessage(Guid workflowId, Guid? workflowInstanceId)
            : this(workflowId, workflowInstanceId, string.Empty)
        {
        }

        protected WorkflowMessage(Guid workflowId, Guid? workflowInstanceId, string jsonState)
        {
            WorkflowId = workflowId;
            WorkflowMessageId = Guid.NewGuid();
            WorkflowInstanceId = workflowInstanceId;
            State = new WorkflowMessageState(jsonState);
        }

        protected WorkflowMessage(Guid workflowId, Guid? workflowInstanceId, JsonState jsonState)
        {
            WorkflowId = workflowId;
            WorkflowMessageId = Guid.NewGuid();
            WorkflowInstanceId = workflowInstanceId;
            State = new WorkflowMessageState(jsonState);
        }

        public Guid WorkflowId { get; set; }

        public Guid? WorkflowInstanceId { get; set; }

        public Guid WorkflowMessageId { get; set; }

        public IWorkflowMessageState State { get; set; }
    }
}