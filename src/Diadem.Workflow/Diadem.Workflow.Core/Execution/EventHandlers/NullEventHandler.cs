using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Execution.Events;

namespace Diadem.Workflow.Core.Execution.EventHandlers
{
    public class NullEventHandler : ICodeEventHandler
    {
        public NullEventHandler(string code)
        {
            Code = code;
        }

        public string Code { get; }

        public Task<EventExecutionResult> Execute(EventExecutionContext eventExecutionContext, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new EventExecutionResult(EventExecutionStatus.Completed));
        }
    }
}