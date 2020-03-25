using System;

namespace Diadem.Workflow.Core.Model
{
    public class WorkflowInstanceActivityLog : IWorkflowInstanceActivityLog
    {
        public WorkflowInstanceActivityLog(Guid workflowInstanceId, string activityCode, DateTime started, int duration)
        {
            WorkflowInstanceId = workflowInstanceId;
            ActivityCode = activityCode;
            Started = started;
            Duration = duration;
        }

        public WorkflowInstanceActivityLog(Guid workflowInstanceId, string activityCode, DateTime started, int duration, int tryCount)
        {
            WorkflowInstanceId = workflowInstanceId;
            ActivityCode = activityCode;
            Started = started;
            Duration = duration;
            TryCount = tryCount;
        }

        public string ActivityCode { get; }

        public int Duration { get; }

        public DateTime Started { get; }

        public int TryCount { get; }

        public Guid WorkflowInstanceId { get; }
    }
}