using System;
using Diadem.Core;
using Diadem.Workflow.Core.Model;

namespace Diadem.Workflow.Core.Execution.Events
{
    public class NestedToParentEvent : IHierarchicalEvent
    {
        public NestedToParentEvent(string code, WorkflowHierarchyEventType hierarchyEventType, Guid workflowInstanceId, Guid nestedWorkflowInstanceId)
        {
            Guard.ArgumentNotNull(code, nameof(code));

            Code = code;
            HierarchyEventType = hierarchyEventType;
            WorkflowInstanceId = workflowInstanceId;
            NestedWorkflowInstanceId = nestedWorkflowInstanceId;
        }

        public NestedToParentEvent(string code, WorkflowHierarchyEventType hierarchyEventType, IWorkflowInstance workflowInstance, Guid nestedWorkflowInstanceId)
        {
            Guard.ArgumentNotNull(code, nameof(code));
            Guard.ArgumentNotNull(workflowInstance, nameof(workflowInstance));

            Code = code;
            HierarchyEventType = hierarchyEventType;
            WorkflowInstanceId = workflowInstance.Id;
            TargetWorkflowInstance = workflowInstance;
            NestedWorkflowInstanceId = nestedWorkflowInstanceId;
        }

        public string Code { get; }

        public WorkflowHierarchyEventType HierarchyEventType { get; }

        public Guid NestedWorkflowInstanceId { get; }

        public IWorkflowInstance TargetWorkflowInstance { get; }

        public Guid WorkflowInstanceId { get; }
    }
}