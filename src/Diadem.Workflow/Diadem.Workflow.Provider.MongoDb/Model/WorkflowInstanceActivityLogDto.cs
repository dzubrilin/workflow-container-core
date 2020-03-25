using System;

namespace Diadem.Workflow.Provider.MongoDb.Model
{
    public class WorkflowInstanceActivityLogDto
    {
        public WorkflowInstanceActivityLogDto(string activityCode, DateTime started, int duration, int tryCount)
        {
            ActivityCode = activityCode;
            Started = started;
            Duration = duration;
            TryCount = tryCount;
        }

        public string ActivityCode { get; }

        public DateTime Started { get; }

        public int Duration { get; }

        public int TryCount { get; }
    }
}