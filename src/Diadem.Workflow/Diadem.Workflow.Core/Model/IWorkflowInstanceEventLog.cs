using System;

namespace Diadem.Workflow.Core.Model
{
    public interface IWorkflowInstanceEventLog
    {
        Guid WorkflowInstanceId { get; }

        string EventCode { get; }

        DateTime Started { get; }

        int Duration { get; }
    }
}