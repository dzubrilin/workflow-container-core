namespace Diadem.Workflow.Core.Execution.Activities
{
    public enum ActivityExecutionStatus
    {
        Undefined = 0,

        Completed = 1,

        Failed = 2,

        FailedNoRetry = 3
    }
}