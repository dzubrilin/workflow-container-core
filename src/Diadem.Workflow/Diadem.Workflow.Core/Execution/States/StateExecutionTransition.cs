using Diadem.Workflow.Core.Configuration;

namespace Diadem.Workflow.Core.Execution.States
{
    public struct StateExecutionTransition
    {
        public StateExecutionTransition(StateTypeConfiguration nextStateType, TransitionConfiguration transitionConfiguration)
        {
            NextStateType = nextStateType;
            TransitionConfiguration = transitionConfiguration;
        }

        public StateTypeConfiguration NextStateType { get; }

        public TransitionConfiguration TransitionConfiguration { get; }
    }
}