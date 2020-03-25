using System;

namespace Diadem.Workflow.Core.Model
{
    public interface IWorkflowInstanceMessageLog
    {
        int Duration { get; }

        DateTime Started { get; }

        string Type { get; }

        Guid WorkflowInstanceId { get; }

        Guid WorkflowMessageId { get; }
    }
}