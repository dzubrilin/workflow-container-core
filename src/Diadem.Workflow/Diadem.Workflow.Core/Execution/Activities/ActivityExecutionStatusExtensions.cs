namespace Diadem.Workflow.Core.Execution.Activities
{
    public static class ActivityExecutionStatusExtensions
    {
        public static bool IsFailed(this ActivityExecutionStatus activityExecutionStatus)
        {
            return activityExecutionStatus == ActivityExecutionStatus.Failed || activityExecutionStatus == ActivityExecutionStatus.FailedNoRetry;
        }
    }
}