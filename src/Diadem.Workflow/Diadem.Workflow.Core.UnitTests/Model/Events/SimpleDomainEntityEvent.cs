using System;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Execution.Events;

namespace Diadem.Workflow.Core.UnitTests.Model.Events
{
    public class SimpleDomainEntityEvent : IDomainEntityEvent
    {
        public SimpleDomainEntityEvent(IEntity entity, string code)
        {
            Code = code;
            Entity = entity;
        }

        public SimpleDomainEntityEvent(IEntity entity, Guid workflowInstanceId, string code)
        {
            Code = code;
            Entity = entity;
            WorkflowInstanceId = workflowInstanceId;
        }

        public SimpleDomainEntityEvent(Guid workflowInstanceId, string code)
        {
            Code = code;
            WorkflowInstanceId = workflowInstanceId;
        }

        public string Code { get; }

        public IEntity Entity { get; }

        public string EntityType => Entity.EntityType;

        public string EntityId => Entity.EntityId;

        public Guid WorkflowInstanceId { get; }
    }
}