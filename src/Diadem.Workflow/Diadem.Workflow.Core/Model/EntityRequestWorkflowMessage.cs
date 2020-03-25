using System;

namespace Diadem.Workflow.Core.Model
{
    public class EntityRequestWorkflowMessage : WorkflowMessage, IEntityRequestWorkflowMessage
    {
        public EntityRequestWorkflowMessage(string entityType, string entityId)
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        public EntityRequestWorkflowMessage(Guid workflowId, Guid? workflowInstanceId, string entityType, string entityId, string jsonState)
            : base(workflowId, workflowInstanceId, jsonState)
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        public string EntityType { get; }
        
        public string EntityId { get; }
    }
}