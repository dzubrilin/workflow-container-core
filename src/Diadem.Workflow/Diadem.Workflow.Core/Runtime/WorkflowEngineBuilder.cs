namespace Diadem.Workflow.Core.Runtime
{
    public sealed class WorkflowEngineBuilder
    {
        public IActivityFactory ActivityFactory { get; private set; }

        public IEventHandlerFactory EventHandlerFactory { get; private set; }

        public IWorkflowDomainStore WorkflowDomainStore { get; private set; }

        public IListenerFactory ListenerFactory { get; private set; }

        public ITransitionFactory TransitionFactory { get; private set; }

        public IWorkflowMessageTransportFactoryProvider WorkflowMessageTransportFactoryProvider { get; private set; }

        public IWorkflowStore WorkflowStore { get; private set; }

        public IWorkflowRuntimeConfigurationFactory WorkflowRuntimeConfigurationFactory { get; private set; }

        public WorkflowEngineBuilder WithWorkflowStore(IWorkflowStore workflowStore)
        {
            WorkflowStore = workflowStore;
            return this;
        }

        public WorkflowEngineBuilder WithActivityFactory(IActivityFactory activityFactory)
        {
            ActivityFactory = activityFactory;
            return this;
        }

        public WorkflowEngineBuilder WithEventHandlerFactory(IEventHandlerFactory eventHandlerFactory)
        {
            EventHandlerFactory = eventHandlerFactory;
            return this;
        }

        public WorkflowEngineBuilder WithDomainStore(IWorkflowDomainStore workflowDomainStore)
        {
            WorkflowDomainStore = workflowDomainStore;
            return this;
        }

        public WorkflowEngineBuilder WithListenerFactory(IListenerFactory listenerFactory)
        {
            ListenerFactory = listenerFactory;
            return this;
        }

        public WorkflowEngineBuilder WithTransitionFactory(ITransitionFactory transitionFactory)
        {
            TransitionFactory = transitionFactory;
            return this;
        }

        public WorkflowEngineBuilder WithMessageTransportFactoryProvider(IWorkflowMessageTransportFactoryProvider workflowMessageTransportFactoryProvider)
        {
            WorkflowMessageTransportFactoryProvider = workflowMessageTransportFactoryProvider;
            return this;
        }

        public WorkflowEngineBuilder WithWorkflowRuntimeConfigurationFactory(IWorkflowRuntimeConfigurationFactory workflowRuntimeConfigurationFactory)
        {
            WorkflowRuntimeConfigurationFactory = workflowRuntimeConfigurationFactory;
            return this;
        }
    }
}