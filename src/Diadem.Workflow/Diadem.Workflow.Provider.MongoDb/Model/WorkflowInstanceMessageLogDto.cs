using System;

namespace Diadem.Workflow.Provider.MongoDb.Model
{
    public class WorkflowInstanceMessageLogDto
    {
        public WorkflowInstanceMessageLogDto()
        {
        }

        public WorkflowInstanceMessageLogDto(string workflowInstanceId, string workflowMessageId, string type, DateTime started, int duration)
        {
            WorkflowInstanceId = workflowInstanceId;
            WorkflowMessageId = workflowMessageId;
            Type = type;
            Started = started;
            Duration = duration;
        }

        public int Duration { get; set; }

        public DateTime Started { get; set; }

        public string Type { get; set; }

        public string WorkflowInstanceId { get; set; }

        public string WorkflowMessageId { get; set; }
    }
}