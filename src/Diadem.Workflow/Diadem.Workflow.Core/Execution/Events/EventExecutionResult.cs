namespace Diadem.Workflow.Core.Execution.Events
{
    public class EventExecutionResult
    {
        public EventExecutionResult(EventExecutionStatus status)
        {
            Status = status;
        }

        public EventExecutionStatus Status { get; }
    }
}