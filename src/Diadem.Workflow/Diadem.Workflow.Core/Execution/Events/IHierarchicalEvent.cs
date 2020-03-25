using Diadem.Workflow.Core.Model;

namespace Diadem.Workflow.Core.Execution.Events
{
    internal interface IHierarchicalEvent : IEvent
    {
        IWorkflowInstance TargetWorkflowInstance { get; }

        WorkflowHierarchyEventType HierarchyEventType { get; }
    }
}