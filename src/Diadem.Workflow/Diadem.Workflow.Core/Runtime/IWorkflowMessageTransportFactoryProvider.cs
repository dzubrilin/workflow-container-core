using Diadem.Core.Configuration;

namespace Diadem.Workflow.Core.Runtime
{
    public interface IWorkflowMessageTransportFactoryProvider
    {
        IWorkflowMessageTransportFactory CreateMessageTransportFactory(EndpointConfigurationType endpointConfigurationType);
    }
}