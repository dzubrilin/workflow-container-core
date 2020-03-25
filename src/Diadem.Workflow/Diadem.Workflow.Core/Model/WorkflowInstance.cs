using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Execution.States;

namespace Diadem.Workflow.Core.Model
{
    public class WorkflowInstance : IWorkflowInstanceInternal, IWorkflowInstance
    {
        private readonly IList<string> _previousStatesInternal;

        public WorkflowInstance(Guid workflowId, Guid id, Guid rootWorkflowInstanceId, Guid parentWorkflowInstanceId,
            string entityType, string entityId, IWorkflowInstanceLock workflowInstanceLock, IEntity entity = null)
        {
            Id = id;
            Entity = entity;
            Lock = workflowInstanceLock;
            _previousStatesInternal = new List<string>();
            VisitedStates = new ReadOnlyCollection<string>(_previousStatesInternal);
            ParentWorkflowInstanceId = parentWorkflowInstanceId;
            RootWorkflowInstanceId = rootWorkflowInstanceId;
            WorkflowId = workflowId;
            EntityType = entityType;
            EntityId = entityId;
        }

        public Guid Id { get; }

        public DateTime Created { get; set; }

        public string CurrentStateCode { get; set; }

        public StateExecutionProgress CurrentStateProgress { get; set; }

        public IEntity Entity { get; set; }

        public string EntityId { get; set; }

        public string EntityType { get; set; }

        public IWorkflowInstanceLock Lock { get; }

        public IReadOnlyList<string> VisitedStates { get; }

        IList<string> IWorkflowInstanceInternal.VisitedStatesInternal => _previousStatesInternal;

        public Guid ParentWorkflowInstanceId { get; }

        public Guid RootWorkflowInstanceId { get; }

        public Guid WorkflowId { get; set; }
    }
}