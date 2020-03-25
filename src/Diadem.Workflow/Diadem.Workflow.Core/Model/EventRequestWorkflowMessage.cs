using System;
using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Core.Model
{
    public class EventRequestWorkflowMessage : WorkflowMessage, IEventRequestWorkflowMessage
    {
        private EventRequestWorkflowMessage()
        {
            WorkflowMessageId = Guid.NewGuid();
        }

        public EventRequestWorkflowMessage(string entityType, string entityId, string eventCode) : this()
        {
            EntityId = entityId;
            EntityType = entityType;
            EventCode = eventCode;
        }

        public EventRequestWorkflowMessage(string entityType, string entityId, string eventCode, string jsonState) : this()
        {
            EntityId = entityId;
            EntityType = entityType;
            EventCode = eventCode;
            State = new WorkflowMessageState(jsonState);
        }

        public EventRequestWorkflowMessage(Guid workflowId, string entityType, string entityId, string eventCode)
            : this(entityType, entityId, eventCode, JsonState.EmptySerializedJsonState)
        {
            WorkflowId = workflowId;
        }

        public EventRequestWorkflowMessage(Guid workflowId, string entityType, string entityId, string eventCode, string jsonState)
            : this(entityType, entityId, eventCode, jsonState)
        {
            WorkflowId = workflowId;
        }

        public EventRequestWorkflowMessage(Guid workflowId, Guid workflowInstanceId, string eventCode, string jsonState)
            : base(workflowId, workflowInstanceId, jsonState)
        {
            EventCode = eventCode;
        }

        public EventRequestWorkflowMessage(Guid workflowId, Guid workflowInstanceId, string eventCode, JsonState jsonState)
            : base(workflowId, workflowInstanceId, jsonState)
        {
            EventCode = eventCode;
        }

        public string EntityId { get; }

        public string EntityType { get; }

        public string EventCode { get; }
    }
}