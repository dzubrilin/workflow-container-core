using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution;
using Diadem.Workflow.Core.Execution.Activities;
using Diadem.Workflow.Core.Execution.Events;
using Diadem.Workflow.Core.Execution.States;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.Runtime;

namespace Diadem.Workflow.Core
{
    public partial class WorkflowEngine
    {
        async Task<WorkflowProcessingResult> IRuntimeWorkflowEngine.InitiateNestedWorkflow(ActivityExecutionContext activityExecutionContext,
            Guid nestedWorkflowId, IEntity nestedEntity, CancellationToken cancellationToken)
        {
            Guard.ArgumentNotNull(activityExecutionContext, nameof(activityExecutionContext));
            Guard.ArgumentNotNull(activityExecutionContext.StateExecutionContext, nameof(activityExecutionContext.StateExecutionContext));
            Guard.ArgumentNotNull(activityExecutionContext.StateExecutionContext.WorkflowContext,
                nameof(activityExecutionContext.StateExecutionContext.WorkflowContext));
            Guard.ArgumentNotNull(activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance,
                nameof(activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance));
            Guard.ArgumentNotNull(activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowConfiguration,
                nameof(activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowConfiguration));
            Guard.ArgumentNotNull(nestedEntity, nameof(nestedEntity));

            // [Assumption] Nested workflow configuration must have one and only one initial from parent event
            var nestedWorkflowConfiguration = await GetWorkflowConfiguration(nestedWorkflowId, cancellationToken).ConfigureAwait(false);
            Guard.ArgumentNotNull(nestedWorkflowConfiguration, nameof(nestedWorkflowConfiguration));

            var workflowInstance = activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance;
            var workflowProcessingResult = await InitiateNestedWorkflowInternal(workflowInstance, nestedWorkflowId, nestedEntity, cancellationToken).ConfigureAwait(false);
            return workflowProcessingResult;
        }

        async Task<IEnumerable<WorkflowProcessingResult>> IRuntimeWorkflowEngine.InitiateNestedWorkflows(ActivityExecutionContext activityExecutionContext,
            Guid nestedWorkflowId, IEnumerable<IEntity> nestedEntities, CancellationToken cancellationToken)
        {
            Guard.ArgumentNotNull(activityExecutionContext, nameof(activityExecutionContext));
            Guard.ArgumentNotNull(activityExecutionContext.StateExecutionContext, nameof(activityExecutionContext.StateExecutionContext));
            Guard.ArgumentNotNull(activityExecutionContext.StateExecutionContext.WorkflowContext,
                nameof(activityExecutionContext.StateExecutionContext.WorkflowContext));
            Guard.ArgumentNotNull(activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance,
                nameof(activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance));
            Guard.ArgumentNotNull(activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowConfiguration,
                nameof(activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowConfiguration));

            var domainEntities = nestedEntities?.ToArray() ?? new IEntity[0];
            if (!domainEntities.Any())
            {
                return Enumerable.Empty<WorkflowProcessingResult>();
            }

            // [Assumption] Nested workflow configuration must have one and only one initial from parent event
            var nestedWorkflowConfiguration = await GetWorkflowConfiguration(nestedWorkflowId, cancellationToken).ConfigureAwait(false);
            Guard.ArgumentNotNull(nestedWorkflowConfiguration, nameof(nestedWorkflowConfiguration));

            var nestedWorkflowInstances = new List<WorkflowProcessingResult>();
            var workflowInstance = activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance;
            foreach (var nestedEntity in domainEntities)
            {
                var workflowProcessingResult = await InitiateNestedWorkflowInternal(workflowInstance, nestedWorkflowId, nestedEntity, cancellationToken).ConfigureAwait(false);
                nestedWorkflowInstances.Add(workflowProcessingResult);
            }

            return nestedWorkflowInstances;
        }

        private Task<WorkflowProcessingResult> InitiateNestedWorkflowInternal(IWorkflowInstance workflowInstance, Guid nestedWorkflowId, IEntity nestedEntity, CancellationToken cancellationToken = default)
        {
            var parentToNestedEvent = new ParentToNestedEntityEvent(EventTypeConfiguration.ParentToNestedInitial.ToString("G"),
                WorkflowHierarchyEventType.Initialize,
                workflowInstance.RootWorkflowInstanceId, workflowInstance.Id, default(Guid), nestedEntity);
            return ((IWorkflowEngine) this).ProcessEvent(nestedWorkflowId, parentToNestedEvent, cancellationToken);
        }

        async Task IRuntimeWorkflowEngine.SendEventToNestedWorkflows(ActivityExecutionContext activityExecutionContext, Guid workflowId, string code, CancellationToken cancellationToken)
        {
            Guard.ArgumentNotNullOrEmpty(code, nameof(code));

            // obtain all the workflow instances of a specified nested workflow class
            var nestedWorkflowInstancesEnumerable = await WorkflowEngineBuilder.WorkflowStore.GetNestedWorkflowInstances(
                activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id, workflowId, cancellationToken).ConfigureAwait(false);
            if (null == nestedWorkflowInstancesEnumerable)
            {
                return;
            }

            var nestedWorkflowInstances = nestedWorkflowInstancesEnumerable.ToList();
            if (nestedWorkflowInstances.Count == 0)
            {
                return;
            }

            foreach (var nestedWorkflowInstance in nestedWorkflowInstances)
            {
                var parentToNestedEvent = new ParentToNestedEntityEvent(code, WorkflowHierarchyEventType.Application,
                    nestedWorkflowInstance.RootWorkflowInstanceId, nestedWorkflowInstance.ParentWorkflowInstanceId, nestedWorkflowInstance, null);
                await PulseToNestedWorkflow(activityExecutionContext.StateExecutionContext.WorkflowContext, parentToNestedEvent, cancellationToken).ConfigureAwait(false);
            }
        }

        Task IRuntimeWorkflowEngine.SendEventToParentWorkflow(ActivityExecutionContext activityExecutionContext, string code, CancellationToken cancellationToken)
        {
            var workflowContext = activityExecutionContext.StateExecutionContext.WorkflowContext;
            return PulseToParentWorkflow(workflowContext, WorkflowHierarchyEventType.Application, code, cancellationToken);
        }

        private async Task PulseFailedToNestedWorkflows(WorkflowContext workflowContext, CancellationToken cancellationToken)
        {
            // all the workflow instances of all the nested workflow classes ==> need to move to failed state
            var nestedWorkflowInstancesEnumerable = await WorkflowEngineBuilder.WorkflowStore.GetNestedWorkflowInstances(workflowContext.WorkflowInstance.Id, cancellationToken).ConfigureAwait(false);
            if (null == nestedWorkflowInstancesEnumerable)
            {
                return;
            }

            var nestedWorkflowInstances = nestedWorkflowInstancesEnumerable.ToList();
            if (nestedWorkflowInstances.Count == 0)
            {
                return;
            }

            foreach (var nestedWorkflowInstance in nestedWorkflowInstances)
            {
                var parentToNestedFailedEvent = new ParentToNestedEntityEvent(EventTypeConfiguration.ParentToNestedFailed.ToString("G"), WorkflowHierarchyEventType.Failed,
                    nestedWorkflowInstance.RootWorkflowInstanceId, nestedWorkflowInstance.ParentWorkflowInstanceId, nestedWorkflowInstance, null);
                await PulseToNestedWorkflow(workflowContext, parentToNestedFailedEvent, cancellationToken).ConfigureAwait(false);
            }
        }

        private Task PulseToNestedWorkflow(WorkflowContext workflowContext, ParentToNestedEntityEvent parentToNestedEntityEvent, CancellationToken cancellationToken = default)
        {
            // [Assumption #1] Failed event should be propagated with no additional checks for validity
            if (parentToNestedEntityEvent.HierarchyEventType == WorkflowHierarchyEventType.Failed)
            {
                return ((IWorkflowEngine) this).ProcessEvent(parentToNestedEntityEvent.TargetWorkflowInstance.WorkflowId, parentToNestedEntityEvent, cancellationToken);
            }

            // [Assumption #2] Nested workflow instance must be in [AwaitingEvent] state for application scope events to pass through
            if (parentToNestedEntityEvent.HierarchyEventType == WorkflowHierarchyEventType.Application &&
                parentToNestedEntityEvent.TargetWorkflowInstance.CurrentStateProgress != StateExecutionProgress.AwaitingEvent)
            {
                throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                    "Nested Workflow instance [ID={0:D}] must be waiting for an event from parent workflow instance [ID={1:D}], but it is in [State={2}..{3:G}]",
                    parentToNestedEntityEvent.TargetWorkflowInstance.Id, workflowContext.WorkflowInstance.Id, parentToNestedEntityEvent.TargetWorkflowInstance.CurrentStateCode,
                    parentToNestedEntityEvent.TargetWorkflowInstance.CurrentStateProgress));
            }

            return ((IWorkflowEngine) this).ProcessEvent(parentToNestedEntityEvent.TargetWorkflowInstance.WorkflowId, parentToNestedEntityEvent, cancellationToken);
        }

//        private Task PulseFinalizedToParentWorkflow(WorkflowContext workflowContext, CancellationToken cancellationToken)
//        {
//            return PulseToParentWorkflow(workflowContext, WorkflowHierarchyEventType.Finalize, EventTypeConfiguration.NestedToParentFinalized.ToString("G"), cancellationToken);
//        }

        private Task PulseFailedToParentWorkflow(WorkflowContext workflowContext, CancellationToken cancellationToken)
        {
            return PulseToParentWorkflow(workflowContext, WorkflowHierarchyEventType.Failed, EventTypeConfiguration.NestedToParentFailed.ToString("G"), cancellationToken);
        }

        private async Task PulseToParentWorkflow(WorkflowContext workflowContext, WorkflowHierarchyEventType hierarchyEventType, string code, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNotNullOrEmpty(code, nameof(code));
            if (workflowContext.WorkflowInstance.Id == workflowContext.WorkflowInstance.ParentWorkflowInstanceId)
            {
                return;
            }

            var parentWorkflowInstance = await GetWorkflowInstanceWithNoLock(workflowContext.WorkflowInstance.ParentWorkflowInstanceId, cancellationToken).ConfigureAwait(false);
            Guard.ArgumentNotNull(parentWorkflowInstance, nameof(parentWorkflowInstance));

            // [Assumption #1] Failed event should be propagated with no additional checks for validity
            if (hierarchyEventType == WorkflowHierarchyEventType.Failed)
            {
                var parentToNestedFailedEvent = new NestedToParentEvent(code, hierarchyEventType, parentWorkflowInstance.ParentWorkflowInstanceId, workflowContext.WorkflowInstance.Id);
                await ((IWorkflowEngine) this).ProcessEvent(parentWorkflowInstance.WorkflowId, parentToNestedFailedEvent, cancellationToken);
                return;
            }

            // [Assumption #2] Parent workflow instance must be in [AwaitingEvent] state for application scope events to pass through
            if (parentWorkflowInstance.CurrentStateProgress != StateExecutionProgress.AwaitingEvent)
            {
                throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                    "Parent Workflow instance [ID={0:D}] must be waiting for an event from nested workflow instance [ID={1:D}], but it is in [State={2}..{3:G}]",
                    parentWorkflowInstance.Id, workflowContext.WorkflowInstance.Id, parentWorkflowInstance.CurrentStateCode,
                    parentWorkflowInstance.CurrentStateProgress));
            }

            var nestedToParentEvent = new NestedToParentEvent(code, hierarchyEventType, parentWorkflowInstance.ParentWorkflowInstanceId, workflowContext.WorkflowInstance.Id);
            await ((IWorkflowEngine) this).ProcessEvent(parentWorkflowInstance.WorkflowId, nestedToParentEvent, cancellationToken).ConfigureAwait(false);
        }
    }
}