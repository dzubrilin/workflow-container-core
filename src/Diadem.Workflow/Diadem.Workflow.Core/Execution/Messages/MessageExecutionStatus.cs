namespace Diadem.Workflow.Core.Execution.Messages
{
    public enum MessageExecutionStatus
    {
        Undefined = 0,

        Failed = 1,

        ContinueAfterAsyncTransition = 2,

        ContinueAfterDelayedTransition = 3,
    }
}