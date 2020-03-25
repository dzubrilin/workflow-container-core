using System;

namespace Diadem.Workflow.Core.Model
{
    public class TransitionEvaluationRequestWorkflowMessage : WorkflowMessage, ITransitionEvaluationRequestWorkflowMessage
    {
        public TransitionEvaluationRequestWorkflowMessage(string code)
        {
            Code = code;
        }

        public TransitionEvaluationRequestWorkflowMessage(Guid workflowId, Guid? workflowInstanceId, string code, string jsonState) : base(workflowId, workflowInstanceId, jsonState)
        {
            Code = code;
        }

        public string Code { get; }
    }
}