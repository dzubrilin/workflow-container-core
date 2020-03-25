using System;

namespace Diadem.Workflow.Provider.MongoDb.Model
{
    public class WorkflowInstanceEventLogDto
    {
        public WorkflowInstanceEventLogDto()
        {
        }

        public WorkflowInstanceEventLogDto(string eventCode, DateTime started, int duration)
        {
            EventCode = eventCode;
            Started = started;
            Duration = duration;
        }

        public string EventCode { get; }

        public DateTime Started { get; }

        public int Duration { get; }
    }
}