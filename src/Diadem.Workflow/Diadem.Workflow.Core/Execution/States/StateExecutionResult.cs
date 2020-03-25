using System.Collections.Generic;

namespace Diadem.Workflow.Core.Execution.States
{
    public sealed class StateExecutionResult
    {
        public StateExecutionResult(StateExecutionStatus status, StateExecutionTransition transition)
        {
            Status = status;
            Transition = transition;
        }

        public StateExecutionStatus Status { get; }

        public StateExecutionTransition Transition { get; }
    }
}