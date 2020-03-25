using System.Threading;
using System.Threading.Tasks;

namespace Diadem.Workflow.Core.Execution.States
{
    internal interface IStateProcessor
    {
        Task<StateExecutionResult> Process(StateExecutionContext stateExecutionContext, CancellationToken cancellationToken);
    }
}