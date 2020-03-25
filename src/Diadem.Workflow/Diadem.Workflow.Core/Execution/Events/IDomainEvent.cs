namespace Diadem.Workflow.Core.Execution.Events
{
    public interface IDomainEvent : IEvent
    {
        string EntityType { get; }

        string EntityId { get; }
    }
}