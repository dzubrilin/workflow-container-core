using Autofac;
using Diadem.Workflow.Core.Runtime;

namespace Diadem.Workflow.Provider.RabbitMq
{
    public class WorkflowMessageRabbitMqTransportModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
//            builder.RegisterType<WorkflowMessageRabbitMqTransportFactory>()
//                   .As<IWorkflowMessageTransportFactory>()
//                   .SingleInstance();
            
            builder.RegisterType<WorkflowMessageRabbitMqTransportFactory>()
                   .As<IWorkflowMessageTransportFactory>()
                   .Keyed<IWorkflowMessageTransportFactory>("rabbitmq")
                   .InstancePerLifetimeScope();
            
        }
    }
}