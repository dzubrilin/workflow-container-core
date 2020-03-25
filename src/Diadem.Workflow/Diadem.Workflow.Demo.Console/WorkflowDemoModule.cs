using Autofac;
using Diadem.Messaging.Core;
using Diadem.Workflow.Demo.Console.RabbitMq;

namespace Diadem.Workflow.Demo.Console
{
    // ReSharper disable once InconsistentNaming
    public class WorkflowDemoModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RabbitMqBusControlFactory>()
                .As<IBusControlFactory>()
                .SingleInstance();
        }
    }
}