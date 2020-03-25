namespace Diadem.Workflow.Core.Execution.Transitions
{
    public class TransitionEvaluationResult
    {
        public TransitionEvaluationResult(TransitionEvaluationStatus status)
        {
            Status = status;
        }

        public TransitionEvaluationStatus Status { get; }
    }
}