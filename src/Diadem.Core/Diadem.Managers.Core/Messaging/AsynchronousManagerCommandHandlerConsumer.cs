using System;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.Commands;
using MassTransit;
using Serilog;

namespace Diadem.Managers.Core.Messaging
{
    public class AsynchronousManagerCommandHandlerConsumer<TManagerCommandRequest, TManagerCommandResponse> : IConsumer<TManagerCommandRequest>
        where TManagerCommandRequest : ManagerCommandRequest
        where TManagerCommandResponse : ManagerCommandResponse, new()
    {
        private readonly IManager _manager;

        public AsynchronousManagerCommandHandlerConsumer(IManager manager)
        {
            _manager = manager;
        }

        public async Task Consume(ConsumeContext<TManagerCommandRequest> context)
        {
            try
            {
                var response = await _manager.Handle<TManagerCommandRequest, TManagerCommandResponse>(context.Message, CancellationToken.None);
                await context.RespondAsync(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred during execution of a remote manager command <{request}, {response}>",
                    typeof(TManagerCommandRequest).Name, typeof(TManagerCommandResponse).Name);

                var response = ManagerCommandResponse.Create<TManagerCommandResponse>(CommandExecutionStatus.InternalServerError, new CommandExecutionInternalErrorResult());
                await context.RespondAsync(response);
            }
        }
    }
}