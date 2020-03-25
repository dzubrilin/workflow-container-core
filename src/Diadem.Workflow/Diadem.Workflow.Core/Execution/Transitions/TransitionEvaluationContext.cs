using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution.States;

namespace Diadem.Workflow.Core.Execution.Transitions
{
    public class TransitionEvaluationContext
    {
        public TransitionEvaluationContext(StateExecutionContext stateExecutionContext,
            TransitionConfiguration transitionConfiguration)
        {
            TransitionConfiguration = transitionConfiguration;
            StateExecutionContext = stateExecutionContext;
        }

        public StateExecutionContext StateExecutionContext { get; }

        public TransitionConfiguration TransitionConfiguration { get; }
    }
}