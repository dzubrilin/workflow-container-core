namespace Diadem.Workflow.Core.Model
{
    public interface IEventRequestWorkflowMessage : IWorkflowMessage
    {
        string EntityId { get; }

        string EntityType { get; }

        string EventCode { get; }
    }
}