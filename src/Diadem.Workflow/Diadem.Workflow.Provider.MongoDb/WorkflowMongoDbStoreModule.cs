using Autofac;
using Diadem.Workflow.Core.Runtime;

namespace Diadem.Workflow.Provider.MongoDb
{
    public class WorkflowMongoDbStoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Workflow store
            builder.RegisterType<MongoDbWorkflowStoreFactory>().As<IWorkflowStoreFactory>();
        }
    }
}