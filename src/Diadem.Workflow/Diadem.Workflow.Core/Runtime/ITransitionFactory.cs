using Diadem.Workflow.Core.Execution.Transitions;

namespace Diadem.Workflow.Core.Runtime
{
    public interface ITransitionFactory
    {
        ITransition CreateTransition(string code);
    }
}