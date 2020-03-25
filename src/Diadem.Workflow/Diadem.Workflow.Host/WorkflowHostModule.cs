using Autofac;
using Diadem.Core.Configuration;
using Diadem.Messaging.Core;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.Runtime;
using Diadem.Workflow.Host.RabbitMq;
using Diadem.Workflow.Host.Workflow;
using MassTransit;

namespace Diadem.Workflow.Host
{
    public class WorkflowHostModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConfigurationProvider>()
                   .As<IConfigurationProvider>()
                   .SingleInstance();

            builder.RegisterType<RabbitMqBusControlFactory>()
                   .As<IBusControlFactory>()
                   .SingleInstance();

            builder.RegisterType<WorkflowMessageConsumer>()
                   .As<IConsumer<IAsynchronousTransitionWorkflowMessage>>()
                   .SingleInstance();

            builder.RegisterType<WorkflowMessageConsumer>()
                   .As<IConsumer<IEventRequestWorkflowMessage>>()
                   .SingleInstance();

            builder.RegisterType<WorkflowMessageConsumer>()
                   .As<IConsumer<IDelayedTransitionWorkflowMessage>>()
                   .SingleInstance();

            builder.RegisterType<RemoteActivityFactory>()
                   .As<IActivityFactory>()
                   .SingleInstance();

            builder.RegisterType<RemoteEventHandlerFactory>()
                   .As<IEventHandlerFactory>()
                   .SingleInstance();

            builder.RegisterType<WorkflowRuntimeConfigurationFactory>()
                   .As<IWorkflowRuntimeConfigurationFactory>()
                   .SingleInstance();

            builder.RegisterType<WorkflowMessageTransportFactoryProvider>()
                   .As<IWorkflowMessageTransportFactoryProvider>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<WorkflowEngineFactory>()
                   .As<IWorkflowEngineFactory>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<RemoteWorkflowDomainStore>()
                   .As<IWorkflowDomainStore>()
                   .InstancePerLifetimeScope();
        }
    }
}