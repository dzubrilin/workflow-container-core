using System;

namespace Diadem.Workflow.Core.Execution.Events
{
    public interface IEvent
    {
        string Code { get; }

        Guid WorkflowInstanceId { get; }
    }
}