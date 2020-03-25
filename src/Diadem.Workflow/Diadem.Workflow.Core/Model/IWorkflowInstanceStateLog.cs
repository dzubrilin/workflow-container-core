using System;

namespace Diadem.Workflow.Core.Model
{
    public interface IWorkflowInstanceStateLog
    {
        Guid WorkflowInstanceId { get; }

        string StateCode { get; }

        DateTime Started { get; }

        int Duration { get; }
    }
}