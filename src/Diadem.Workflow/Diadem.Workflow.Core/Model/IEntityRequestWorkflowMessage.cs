namespace Diadem.Workflow.Core.Model
{
    public interface IEntityRequestWorkflowMessage : IWorkflowMessage
    {
        string EntityType { get; }
        
        string EntityId { get; }
    }
}