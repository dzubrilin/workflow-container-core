using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Execution.Events;

namespace Diadem.Workflow.Core.Execution.EventHandlers
{
    public interface ICodeEventHandler : IEventHandler
    {
        Task<EventExecutionResult> Execute(EventExecutionContext eventExecutionContext, CancellationToken cancellationToken = default);
    }
}