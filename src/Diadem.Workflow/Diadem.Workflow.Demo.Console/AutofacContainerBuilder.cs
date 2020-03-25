using Autofac;
using Diadem.Workflow.Provider.MongoDb;
using Diadem.Workflow.Provider.RabbitMq;

namespace Diadem.Workflow.Demo.Console
{
    public static class AutofacContainerBuilder
    {
        public static IContainer CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<WorkflowDemoModule>();
            containerBuilder.RegisterModule<WorkflowMongoDbStoreModule>();
            containerBuilder.RegisterModule<WorkflowMessageRabbitMqTransportModule>();

            return containerBuilder.Build();
        }
    }
}