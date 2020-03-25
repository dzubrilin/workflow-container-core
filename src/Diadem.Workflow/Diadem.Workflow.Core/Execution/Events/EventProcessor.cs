using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution.EventHandlers;
using Serilog;

namespace Diadem.Workflow.Core.Execution.Events
{
    internal class EventProcessor : IEventProcessor
    {
        private readonly WorkflowEngine _workflowEngine;

        public EventProcessor(WorkflowEngine workflowEngine)
        {
            _workflowEngine = workflowEngine;
        }

        public async Task<EventExecutionResult> Process(EventExecutionContext eventExecutionContext, CancellationToken cancellationToken)
        {
            Log.Verbose("Starting processing event {code} for {workflowInstanceId}",
                eventExecutionContext.Event.Code,
                eventExecutionContext.WorkflowContext.WorkflowInstance.Id);

            // special event from nested workflow with status [Failed]  
            if (eventExecutionContext.Event is NestedToParentEvent nestedToParentEvent &&
                nestedToParentEvent.HierarchyEventType == WorkflowHierarchyEventType.Failed)
            {
                return new EventExecutionResult(EventExecutionStatus.FailedNoRetry);
            }

            // special event from parent workflow with status [Failed]  
            if (eventExecutionContext.Event is ParentToNestedEntityEvent parentToNestedEvent &&
                parentToNestedEvent.HierarchyEventType == WorkflowHierarchyEventType.Failed)
            {
                return new EventExecutionResult(EventExecutionStatus.FailedNoRetry);
            }

            if (null == eventExecutionContext.StateEventConfiguration)
            {
                throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                    "Only system event are allowed to not to be included into state [{0}] configuration workflow ID = [{1:D}]",
                    eventExecutionContext.EventConfiguration.Code, eventExecutionContext.WorkflowContext.WorkflowConfiguration.Id));
            }

            try
            {
                var codeEventHandler = _workflowEngine.WorkflowEngineBuilder
                    .EventHandlerFactory.CreateEventHandler(eventExecutionContext.StateEventConfiguration.HandlerCode);

                if (null == codeEventHandler
                    && !string.IsNullOrEmpty(eventExecutionContext.StateEventConfiguration.Script)
                    && eventExecutionContext.StateEventConfiguration.ScriptType == ScriptTypeConfiguration.CSharp)
                {
                    codeEventHandler = new CSharpScriptEventHandler(eventExecutionContext.EventConfiguration.Code,
                        eventExecutionContext.StateEventConfiguration.ScriptType, eventExecutionContext.StateEventConfiguration.Script);
                }

                if (null == codeEventHandler)
                {
                    return new EventExecutionResult(EventExecutionStatus.Completed);
                }

                Log.Verbose("Starting event handler {code} for {workflowInstanceId}",
                    eventExecutionContext.Event.Code,
                    eventExecutionContext.WorkflowContext.WorkflowInstance.Id);

                return await codeEventHandler.Execute(eventExecutionContext, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred during processing event {code} for {workflowInstanceId}]",
                    eventExecutionContext.Event.Code, eventExecutionContext.WorkflowContext.WorkflowInstance.Id);

                return new EventExecutionResult(EventExecutionStatus.Failed);
            }
        }
    }
}