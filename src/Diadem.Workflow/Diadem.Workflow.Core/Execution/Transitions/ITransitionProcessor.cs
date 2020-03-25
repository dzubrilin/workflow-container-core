using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution.States;

namespace Diadem.Workflow.Core.Execution.Transitions
{
    public interface ITransitionProcessor
    {
        Task<TransitionEvaluationResult> Evaluate(StateExecutionContext stateExecutionContext,
            TransitionConfiguration transitionConfiguration, CancellationToken cancellationToken);
    }
}