using System;

namespace Diadem.Workflow.Core.Model
{
    public class WorkflowInstanceEventLog : IWorkflowInstanceEventLog
    {
        public WorkflowInstanceEventLog(Guid workflowInstanceId, string eventCode, DateTime started, int duration)
        {
            WorkflowInstanceId = workflowInstanceId;
            EventCode = eventCode;
            Started = started;
            Duration = duration;
        }

        public Guid WorkflowInstanceId { get; }

        public string EventCode { get; }

        public DateTime Started { get; }

        public int Duration { get; }
    }
}