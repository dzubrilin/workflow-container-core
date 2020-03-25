namespace Diadem.Workflow.Core.Execution.Messages
{
    public class MessageExecutionResult
    {
        public MessageExecutionResult(MessageExecutionStatus status)
        {
            Status = status;
        }

        public MessageExecutionStatus Status { get; }
    }
}