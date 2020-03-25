using System.Threading;
using System.Threading.Tasks;

namespace Diadem.Core.Commands
{
    public interface ICommandHandler<in TCommandRequest, TCommandResponse>
        where TCommandRequest : CommandRequest
        where TCommandResponse : CommandResponse
    {
        Task<TCommandResponse> Handle(TCommandRequest request, CancellationToken cancellationToken);
    }
}