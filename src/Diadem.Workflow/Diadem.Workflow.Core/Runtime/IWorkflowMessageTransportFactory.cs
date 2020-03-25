using System;

namespace Diadem.Workflow.Core.Runtime
{
    public interface IWorkflowMessageTransportFactory
    {
        IWorkflowMessageTransport CreateMessageTransport(Uri address);
    }
}