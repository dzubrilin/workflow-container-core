using System;
using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Core.Execution.Events
{
    public class StatefulDomainEvent : StatefulEvent, IDomainEvent
    {
        public StatefulDomainEvent(string code, Guid workflowInstanceId, string entityType, string entityId, JsonState state) : base(code, workflowInstanceId, state)
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        public string EntityType { get; }

        public string EntityId { get; }
    }
}