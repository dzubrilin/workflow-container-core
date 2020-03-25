using Diadem.Workflow.Core.Execution.Events;

namespace Diadem.Workflow.Core.Model
{
    public interface IEventResponseWorkflowMessage : IWorkflowMessage
    {
        EventExecutionResult EventExecutionResult { get; }
    }
}