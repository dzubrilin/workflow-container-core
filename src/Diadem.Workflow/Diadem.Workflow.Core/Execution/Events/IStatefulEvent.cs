using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Core.Execution.Events
{
    public interface IStatefulEvent : IEvent
    {
        JsonState State { get; }
    }
}