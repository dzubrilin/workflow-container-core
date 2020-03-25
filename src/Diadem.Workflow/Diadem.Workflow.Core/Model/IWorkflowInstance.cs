using System;
using System.Collections.Generic;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Execution.States;

namespace Diadem.Workflow.Core.Model
{
    public interface IWorkflowInstance
    {
        DateTime Created { get; }

        string CurrentStateCode { get; set; }

        StateExecutionProgress CurrentStateProgress { get; set; }

        IEntity Entity { get; set; }

        string EntityId { get; set; }

        string EntityType { get; set; }

        Guid Id { get; }

        IWorkflowInstanceLock Lock { get; }

        IReadOnlyList<string> VisitedStates { get; }

        Guid ParentWorkflowInstanceId { get; }

        Guid RootWorkflowInstanceId { get; }

        Guid WorkflowId { get; }
    }
}