using System.Security;
using Diadem.Core.Configuration;
using Diadem.Workflow.Core;
using Diadem.Workflow.Core.Runtime;
using Diadem.Workflow.Provider.MongoDb;
using MongoDB.Driver;

namespace Diadem.Workflow.Host.Workflow
{
    public class WorkflowEngineFactory : IWorkflowEngineFactory
    {
        private readonly IActivityFactory _activityFactory;

        private readonly IEventHandlerFactory _eventHandlerFactory;

        private readonly IWorkflowDomainStore _workflowDomainStore;
        
        private readonly IWorkflowStoreFactory _workflowStoreFactory;

        private readonly IWorkflowMessageTransportFactoryProvider _workflowMessageTransportFactoryProvider;

        private readonly IWorkflowRuntimeConfigurationFactory _workflowRuntimeConfigurationFactory;

        public WorkflowEngineFactory(
            IActivityFactory activityFactory,
            IEventHandlerFactory eventHandlerFactory,
            IWorkflowDomainStore workflowDomainStore,
            IWorkflowStoreFactory workflowStoreFactory,
            IWorkflowMessageTransportFactoryProvider workflowMessageTransportFactoryProvider,
            IWorkflowRuntimeConfigurationFactory workflowRuntimeConfigurationFactory)
        {
            _activityFactory = activityFactory;
            _eventHandlerFactory = eventHandlerFactory;
            _workflowDomainStore = workflowDomainStore;
            _workflowStoreFactory = workflowStoreFactory;
            _workflowRuntimeConfigurationFactory = workflowRuntimeConfigurationFactory;
            _workflowMessageTransportFactoryProvider = workflowMessageTransportFactoryProvider;
        }

        public IWorkflowEngine CreateWorkflowEngine()
        {
            var workflowStore = _workflowStoreFactory.Create();
            var workflowEngineBuilder = new WorkflowEngineBuilder();
            workflowEngineBuilder
                .WithActivityFactory(_activityFactory)
                .WithEventHandlerFactory(_eventHandlerFactory)
                .WithDomainStore(_workflowDomainStore)
                .WithMessageTransportFactoryProvider(_workflowMessageTransportFactoryProvider)
                .WithWorkflowRuntimeConfigurationFactory(_workflowRuntimeConfigurationFactory)
                .WithWorkflowStore(workflowStore);
            return new WorkflowEngine(workflowEngineBuilder);
        }
    }
}