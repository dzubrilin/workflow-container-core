using System;

namespace Diadem.Workflow.Core.Model
{
    public interface IWorkflowInstanceActivityLog
    {
        Guid WorkflowInstanceId { get; }

        string ActivityCode { get; }

        DateTime Started { get; }

        int Duration { get; }

        int TryCount { get; }
    }
}