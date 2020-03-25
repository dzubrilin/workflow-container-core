using System.Threading;
using System.Threading.Tasks;

namespace Diadem.Workflow.Core.Execution.Messages
{
    public interface IMessageProcessor
    {
        Task<MessageExecutionResult> ProcessMessage(MessageExecutionContext messageExecutionContext, CancellationToken cancellationToken = default);
    }
}