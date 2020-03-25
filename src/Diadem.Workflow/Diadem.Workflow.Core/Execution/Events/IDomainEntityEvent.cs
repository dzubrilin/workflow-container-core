using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Core.Execution.Events
{
    public interface IDomainEntityEvent : IDomainEvent
    {
        IEntity Entity { get; }
    }
}