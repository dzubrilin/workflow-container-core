using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.Configuration;

namespace Diadem.Managers.Core
{
    public interface IManagerCommandTransport
    {
        Task<TResponseWorkflowMessage> Request<TRequestWorkflowMessage, TResponseWorkflowMessage>(
            IEndpointConfiguration endpointConfiguration, TRequestWorkflowMessage workflowMessage, CancellationToken cancellationToken)
            where TRequestWorkflowMessage : ManagerCommandRequest
            where TResponseWorkflowMessage : ManagerCommandResponse;
    }
}