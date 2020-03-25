using System.Threading;
using System.Threading.Tasks;

namespace Diadem.Managers.Core
{
    public interface IManager
    {
        Task<TManagerCommandResponse> Handle<TManagerCommandRequest, TManagerCommandResponse>(TManagerCommandRequest request, CancellationToken cancellationToken)
            where TManagerCommandRequest : ManagerCommandRequest
            where TManagerCommandResponse : ManagerCommandResponse, new();

        Task<TManagerCommandResponse> HandleRemote<TManagerCommandRequest, TManagerCommandResponse>(TManagerCommandRequest request, CancellationToken cancellationToken)
            where TManagerCommandRequest : ManagerCommandRequest
            where TManagerCommandResponse : ManagerCommandResponse, new();
    }
}