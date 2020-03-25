using System.Threading;
using System.Threading.Tasks;

namespace Diadem.Workflow.Core.Execution.Events
{
    public interface IEventProcessor
    {
        Task<EventExecutionResult> Process(EventExecutionContext eventExecutionContext, CancellationToken cancellationToken = default);
    }
}