using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Execution.States;

namespace Diadem.Workflow.Core.Runtime
{
    public interface IStateListener
    {
        Task AfterExecutionActivities(StateExecutionContext stateExecutionContext, CancellationToken cancellationToken = default);

        Task AfterExecutionState(StateExecutionContext stateExecutionContext, StateExecutionResult stateExecutionResult, CancellationToken cancellationToken = default);

        Task BeforeExecutingState(StateExecutionContext stateExecutionContext, CancellationToken cancellationToken = default);
    }
}