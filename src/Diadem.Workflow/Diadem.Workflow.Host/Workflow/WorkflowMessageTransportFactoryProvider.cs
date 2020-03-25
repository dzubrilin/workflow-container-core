using System;
using Autofac;
using Autofac.Core.Lifetime;
using Diadem.Core.Configuration;
using Diadem.Workflow.Core.Runtime;

namespace Diadem.Workflow.Host.Workflow
{
    public class WorkflowMessageTransportFactoryProvider : IWorkflowMessageTransportFactoryProvider
    {
        private readonly ILifetimeScope _lifetimeScope;

        public WorkflowMessageTransportFactoryProvider(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public IWorkflowMessageTransportFactory CreateMessageTransportFactory(EndpointConfigurationType endpointConfigurationType)
        {
            var typeKey = endpointConfigurationType.ToString().ToLower();

            if (_lifetimeScope.TryResolveKeyed(typeKey, typeof(IWorkflowMessageTransportFactory), out var workflowMessageTransportFactory))
            {
                return (IWorkflowMessageTransportFactory) workflowMessageTransportFactory;
            }

            throw new NotSupportedException($"Workflow message transport {endpointConfigurationType} is not supported");
        }
    }
}