using System.Threading;
using System.Threading.Tasks;

namespace Diadem.Workflow.Core.Execution.Transitions
{
    public interface ITransition
    {
        Task<TransitionEvaluationResult> Evaluate(TransitionEvaluationContext transitionEvaluationContext, CancellationToken cancellationToken);
    }
}