namespace Diadem.Workflow.Core.Execution.Events
{
    public enum EventExecutionStatus
    {
        Undefined = 0,

        Completed = 1,

        KeepWaiting = 2,

        Failed = 3,

        FailedNoRetry = 4
    }
}