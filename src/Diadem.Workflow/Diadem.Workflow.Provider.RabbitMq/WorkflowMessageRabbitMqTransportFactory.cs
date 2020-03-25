using System;
using Diadem.Messaging.Core;
using Diadem.Workflow.Core.Runtime;

namespace Diadem.Workflow.Provider.RabbitMq
{
    // ReSharper disable once InconsistentNaming
    public class WorkflowMessageRabbitMqTransportFactory : IWorkflowMessageTransportFactory
    {
        private readonly IBusControlFactory _busControlFactory;

        public WorkflowMessageRabbitMqTransportFactory(IBusControlFactory busControlFactory)
        {
            _busControlFactory = busControlFactory;
        }

        public IWorkflowMessageTransport CreateMessageTransport(Uri address)
        {
            var busControl = _busControlFactory.CreateBusControl();
            return new WorkflowMessageRabbitMqTransport(busControl);
        }
    }
}