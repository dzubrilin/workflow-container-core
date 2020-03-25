using System;
using Diadem.Core;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Model;

namespace Diadem.Workflow.Core.Execution.Events
{
    public class ParentToNestedEntityEvent : IDomainEntityEvent, IHierarchicalEvent
    {
        public ParentToNestedEntityEvent(string code, WorkflowHierarchyEventType hierarchyEventType,
            Guid rootWorkflowInstanceId, Guid parentWorkflowInstanceId, Guid workflowInstanceId, IEntity nestedEntity)
        {
            Guard.ArgumentNotNull(code, nameof(code));

            Code = code;
            Entity = nestedEntity;
            HierarchyEventType = hierarchyEventType;
            ParentWorkflowInstanceId = parentWorkflowInstanceId;
            RootWorkflowInstanceId = rootWorkflowInstanceId;
            WorkflowInstanceId = workflowInstanceId;
        }

        public ParentToNestedEntityEvent(string code, WorkflowHierarchyEventType hierarchyEventType,
            Guid rootWorkflowInstanceId, Guid parentWorkflowInstanceId, IWorkflowInstance workflowInstance, IEntity nestedEntity)
        {
            Guard.ArgumentNotNull(code, nameof(code));
            Guard.ArgumentNotNull(workflowInstance, nameof(workflowInstance));

            Code = code;
            Entity = nestedEntity;
            HierarchyEventType = hierarchyEventType;
            ParentWorkflowInstanceId = parentWorkflowInstanceId;
            RootWorkflowInstanceId = rootWorkflowInstanceId;
            TargetWorkflowInstance = workflowInstance;
            WorkflowInstanceId = workflowInstance.Id;
        }

        public string Code { get; }

        public IEntity Entity { get; }

        public string EntityType => Entity.EntityType;

        public string EntityId => Entity.EntityId;

        public WorkflowHierarchyEventType HierarchyEventType { get; }

        public Guid ParentWorkflowInstanceId { get; }

        public Guid RootWorkflowInstanceId { get; }

        public IWorkflowInstance TargetWorkflowInstance { get; }

        public Guid WorkflowInstanceId { get; }
    }
}