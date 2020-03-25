using Autofac;
using Diadem.Workflow.Core.Runtime;

namespace Diadem.Workflow.Provider.Http
{
    public class WorkflowMessageHttpTransportModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WorkflowMessageHttpTransportFactory>()
                   .As<IWorkflowMessageTransportFactory>()
                   .Keyed<IWorkflowMessageTransportFactory>("http")
                   .SingleInstance();
        }
    }
}