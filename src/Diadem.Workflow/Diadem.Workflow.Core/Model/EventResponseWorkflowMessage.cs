using Diadem.Workflow.Core.Execution.Events;

namespace Diadem.Workflow.Core.Model
{
    public class EventResponseWorkflowMessage : WorkflowMessage, IEventResponseWorkflowMessage
    {
        public EventResponseWorkflowMessage()
        {
        }

        public EventExecutionResult EventExecutionResult { get; set; }
    }
}