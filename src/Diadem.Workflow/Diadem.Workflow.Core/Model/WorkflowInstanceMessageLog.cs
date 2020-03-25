using System;

namespace Diadem.Workflow.Core.Model
{
    public class WorkflowInstanceMessageLog : IWorkflowInstanceMessageLog
    {
        public WorkflowInstanceMessageLog(Guid workflowInstanceId, Guid workflowMessageId, string type, DateTime started, int duration)
        {
            WorkflowInstanceId = workflowInstanceId;
            WorkflowMessageId = workflowMessageId;
            Type = type;
            Started = started;
            Duration = duration;
        }

        public int Duration { get; }

        public DateTime Started { get; }

        public string Type { get; }

        public Guid WorkflowInstanceId { get; }

        public Guid WorkflowMessageId { get; }
    }
}