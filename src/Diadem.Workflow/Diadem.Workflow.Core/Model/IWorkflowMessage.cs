using System;

namespace Diadem.Workflow.Core.Model
{
    public interface IWorkflowMessage
    {
        Guid WorkflowId { get; set; }

        Guid? WorkflowInstanceId { get; set; }

        Guid WorkflowMessageId { get; set; }

        IWorkflowMessageState State { get; set; }
    }
}