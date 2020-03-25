using System;

namespace Diadem.Workflow.Core.Model
{
    public class WorkflowInstanceStateLog : IWorkflowInstanceStateLog
    {
        public WorkflowInstanceStateLog(Guid workflowInstanceId, string stateCode, DateTime started, int duration)
        {
            WorkflowInstanceId = workflowInstanceId;
            StateCode = stateCode;
            Started = started;
            Duration = duration;
        }

        public Guid WorkflowInstanceId { get; }

        public string StateCode { get; }

        public DateTime Started { get; }

        public int Duration { get; }
    }
}