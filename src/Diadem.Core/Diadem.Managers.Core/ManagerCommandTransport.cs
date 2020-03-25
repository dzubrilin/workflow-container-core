using System;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.Configuration;
using MassTransit;

namespace Diadem.Managers.Core
{
    public sealed class ManagerCommandTransport : IManagerCommandTransport
    {
        private readonly IBusControl _busControl;

        public ManagerCommandTransport(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public async Task<TManagerCommandResponse> Request<TManagerCommandRequest, TManagerCommandResponse>(
            IEndpointConfiguration endpointConfiguration, TManagerCommandRequest request, CancellationToken cancellationToken)
            where TManagerCommandRequest : ManagerCommandRequest
            where TManagerCommandResponse : ManagerCommandResponse
        {
            var requestClient = CreateRequestClient<TManagerCommandRequest, TManagerCommandResponse>(endpointConfiguration);
            return await requestClient.Request(request, cancellationToken).ConfigureAwait(false);
        }

        private IRequestClient<TManagerCommandRequest, TManagerCommandResponse> CreateRequestClient<TManagerCommandRequest, TManagerCommandResponse>(IEndpointConfiguration endpointConfiguration)
            where TManagerCommandRequest : ManagerCommandRequest
            where TManagerCommandResponse : ManagerCommandResponse
        {
            var serviceAddress = endpointConfiguration.Address;
            return _busControl.CreateRequestClient<TManagerCommandRequest, TManagerCommandResponse>(serviceAddress, TimeSpan.FromSeconds(30));
        }
    }
}