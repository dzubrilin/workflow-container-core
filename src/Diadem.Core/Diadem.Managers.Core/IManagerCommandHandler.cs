using System.Threading;
using System.Threading.Tasks;

namespace Diadem.Managers.Core
{
    public interface IManagerCommandHandler<in TManagerCommandRequest, TManagerCommandResponse> : IManagerCommandHandler
        where TManagerCommandRequest : ManagerCommandRequest
        where TManagerCommandResponse : ManagerCommandResponse
    {
        Task<TManagerCommandResponse> Handle(TManagerCommandRequest commandRequest, CancellationToken cancellationToken);
    }

    public interface IManagerCommandHandler
    {
    }
}