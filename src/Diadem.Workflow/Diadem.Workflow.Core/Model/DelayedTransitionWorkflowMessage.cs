using System;

namespace Diadem.Workflow.Core.Model
{
    public class DelayedTransitionWorkflowMessage : WorkflowMessage, IDelayedTransitionWorkflowMessage
    {
        public DelayedTransitionWorkflowMessage(Guid workflowId, Guid? workflowInstanceId, TimeSpan delay, string moveFromState, string moveToState) : base(workflowId, workflowInstanceId)
        {
            Delay = delay;
            MoveFromState = moveFromState;
            MoveToState = moveToState;
        }

        public DelayedTransitionWorkflowMessage(Guid workflowId, Guid? workflowInstanceId, string jsonState, TimeSpan delay, string moveFromState, string moveToState) : base(workflowId,
            workflowInstanceId, jsonState)
        {
            Delay = delay;
            MoveFromState = moveFromState;
            MoveToState = moveToState;
        }

        public TimeSpan Delay { get; }

        public string MoveFromState { get; }

        public string MoveToState { get; }
    }
}