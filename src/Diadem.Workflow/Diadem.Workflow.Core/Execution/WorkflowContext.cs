using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.Runtime;

namespace Diadem.Workflow.Core.Execution
{
    public sealed class WorkflowContext
    {
        private readonly WorkflowEngineBuilder _workflowEngineBuilder;

        public WorkflowContext(IRuntimeWorkflowEngine workflowEngine, WorkflowEngineBuilder workflowEngineBuilder,
            IWorkflowInstance workflowInstance, WorkflowConfiguration workflowConfiguration)
        {
            _workflowEngineBuilder = workflowEngineBuilder;
            WorkflowEngine = workflowEngine;
            WorkflowInstance = workflowInstance;
            WorkflowConfiguration = workflowConfiguration;
            WorkflowExecutionState = new JsonState();
        }

        public WorkflowContext(IRuntimeWorkflowEngine workflowEngine, WorkflowEngineBuilder workflowEngineBuilder,
            IWorkflowInstance workflowInstance, WorkflowConfiguration workflowConfiguration, JsonState workflowExecutionState)
        {
            _workflowEngineBuilder = workflowEngineBuilder;
            WorkflowEngine = workflowEngine;
            WorkflowInstance = workflowInstance;
            WorkflowConfiguration = workflowConfiguration;
            WorkflowExecutionState = workflowExecutionState ?? new JsonState();
        }

        public IRuntimeWorkflowEngine WorkflowEngine { get; }

        public JsonState WorkflowExecutionState { get; }

        public IWorkflowInstance WorkflowInstance { get; }

        public WorkflowConfiguration WorkflowConfiguration { get; }

        public IWorkflowMessageTransportFactoryProvider WorkflowMessageTransportFactoryProvider => _workflowEngineBuilder.WorkflowMessageTransportFactoryProvider;
    }
}