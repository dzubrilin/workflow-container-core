using System;
using Diadem.Workflow.Core.Execution.Transitions;

namespace Diadem.Workflow.Core.Model
{
    public class TransitionEvaluationResponseWorkflowMessage : WorkflowMessage, ITransitionEvaluationResponseWorkflowMessage
    {
        public TransitionEvaluationResponseWorkflowMessage()
        {
        }

        public TransitionEvaluationResponseWorkflowMessage(Guid workflowId, Guid? workflowInstanceId) : base(workflowId, workflowInstanceId)
        {
        }

        public TransitionEvaluationResponseWorkflowMessage(Guid workflowId, Guid? workflowInstanceId, string jsonState) : base(workflowId, workflowInstanceId, jsonState)
        {
        }

        public TransitionEvaluationStatus TransitionEvaluationStatus { get; set; }
    }
}