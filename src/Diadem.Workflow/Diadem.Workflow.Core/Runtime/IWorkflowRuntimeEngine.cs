using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Execution;
using Diadem.Workflow.Core.Execution.Activities;
using Diadem.Workflow.Core.Model;

namespace Diadem.Workflow.Core.Runtime
{
    public interface IRuntimeWorkflowEngine
    {
        Task<WorkflowProcessingResult> InitiateNestedWorkflow(ActivityExecutionContext activityExecutionContext, Guid nestedWorkflowId, IEntity entity, CancellationToken cancellationToken);

        Task<IEnumerable<WorkflowProcessingResult>> InitiateNestedWorkflows(ActivityExecutionContext activityExecutionContext,
            Guid nestedWorkflowId, IEnumerable<IEntity> nestedEntities, CancellationToken cancellationToken);

        Task SendEventToNestedWorkflows(ActivityExecutionContext activityExecutionContext, Guid workflowId, string code, CancellationToken cancellationToken);

        Task SendEventToParentWorkflow(ActivityExecutionContext activityExecutionContext, string code, CancellationToken cancellationToken);
    }
}