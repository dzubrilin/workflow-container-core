using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.Configuration;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.Runtime;
using MassTransit;

namespace Diadem.Workflow.Provider.RabbitMq
{
    public class WorkflowMessageRabbitMqTransport : IWorkflowMessageTransport
    {
        private readonly IBusControl _busControl;

        public WorkflowMessageRabbitMqTransport(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public async Task<TResponseWorkflowMessage> Request<TRequestWorkflowMessage, TResponseWorkflowMessage>(
            IEndpointConfiguration endpointConfiguration, TRequestWorkflowMessage workflowMessage, CancellationToken cancellationToken)
            where TRequestWorkflowMessage : class, IWorkflowMessage
            where TResponseWorkflowMessage : class, IWorkflowMessage
        {
            // TODO: need to find a way to dispose <ClientRequestHandle> under the hood to avoid <RequestTimeoutException> to be thrown
            var requestClient = CreateRequestClient<TRequestWorkflowMessage, TResponseWorkflowMessage>(endpointConfiguration);
            return await requestClient.Request(workflowMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task Send<TWorkflowMessage>(IEndpointConfiguration endpointConfiguration, TWorkflowMessage workflowMessage, CancellationToken cancellationToken)
            where TWorkflowMessage : class, IWorkflowMessage
        {
            var sendEndpoint = await _busControl.GetSendEndpoint(endpointConfiguration.Address).ConfigureAwait(false);
            await sendEndpoint.Send(workflowMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task SendWithDelay<TWorkflowMessage>(IEndpointConfiguration endpointConfiguration, TWorkflowMessage workflowMessage, CancellationToken cancellationToken)
            where TWorkflowMessage : class, IDelayedWorkflowMessage
        {
            var addressEndpoint = string.Format(CultureInfo.InvariantCulture,
                "{0}://{1}{2}/{3}?type=topic",
                endpointConfiguration.Address.Scheme,
                endpointConfiguration.Address.Host,
                endpointConfiguration.Address.Port == -1 ? string.Empty : ":" + endpointConfiguration.Address.Port.ToString(CultureInfo.InvariantCulture),
                workflowMessage.Delay.ToRabbitMqDelayedExchangeName("workflow.exchange.delay-"));
            var sendEndpoint = await _busControl.GetSendEndpoint(new Uri(addressEndpoint));
            await sendEndpoint.Send(workflowMessage, cancellationToken).ConfigureAwait(false);
        }

        private IRequestClient<TRequest, TResponse> CreateRequestClient<TRequest, TResponse>(IEndpointConfiguration endpointConfiguration)
            where TRequest : class, IWorkflowMessage
            where TResponse : class, IWorkflowMessage
        {
            var serviceAddress = endpointConfiguration.Address;
            return _busControl.CreateRequestClient<TRequest, TResponse>(serviceAddress, TimeSpan.FromSeconds(30));
        }
    }
}