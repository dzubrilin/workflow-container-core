namespace Diadem.Workflow.Core.Model
{
    public interface IActivityRequestWorkflowMessage : IWorkflowMessage
    {
        string Code { get; }
    }
}