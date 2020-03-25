using Autofac;
using Diadem.Http.DotNetCore;
using Diadem.Workflow.Provider.Http;
using Diadem.Workflow.Provider.MongoDb;
using Diadem.Workflow.Provider.RabbitMq;

namespace Diadem.Workflow.Host
{
    public static class AutofacContainerBuilder
    {
        public static IContainer CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<HttpClientFactoryModule>();
            
            containerBuilder.RegisterModule<WorkflowHostModule>();
            containerBuilder.RegisterModule<WorkflowMongoDbStoreModule>();
            containerBuilder.RegisterModule<WorkflowMessageHttpTransportModule>();
            containerBuilder.RegisterModule<WorkflowMessageRabbitMqTransportModule>();
            
            return containerBuilder.Build();
        }
    }
}