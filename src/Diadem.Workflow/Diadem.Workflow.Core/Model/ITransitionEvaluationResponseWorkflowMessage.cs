using Diadem.Workflow.Core.Execution.Transitions;

namespace Diadem.Workflow.Core.Model
{
    public interface ITransitionEvaluationResponseWorkflowMessage : IWorkflowMessage
    {
        TransitionEvaluationStatus TransitionEvaluationStatus { get; set; }
    }
}