namespace Diadem.Workflow.Core.Model
{
    public interface ITransitionEvaluationRequestWorkflowMessage : IWorkflowMessage
    {
        string Code { get; }
    }
}