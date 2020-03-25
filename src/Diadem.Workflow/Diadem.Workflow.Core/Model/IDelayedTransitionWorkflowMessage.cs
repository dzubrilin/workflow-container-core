namespace Diadem.Workflow.Core.Model
{
    public interface IDelayedTransitionWorkflowMessage : IDelayedWorkflowMessage
    {
        string MoveFromState { get; }

        string MoveToState { get; }
    }
}