namespace Diadem.Workflow.Core.Execution.Activities
{
    public sealed class ActivityExecutionResult
    {
        public ActivityExecutionResult(ActivityExecutionStatus status)
        {
            Status = status;
        }

        public ActivityExecutionResult(ActivityExecutionStatus status, string transitionToUse)
        {
            Status = status;
            TransitionToUse = transitionToUse;
        }

        public ActivityExecutionStatus Status { get; }
        
        public string TransitionToUse { get; set; }
    }
}