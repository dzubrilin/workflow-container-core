using System;

namespace Diadem.Workflow.Provider.MongoDb.Model
{
    public class WorkflowInstanceStateLogDto
    {
        public WorkflowInstanceStateLogDto(string stateCode, DateTime started, int duration)
        {
            StateCode = stateCode;
            Started = started;
            Duration = duration;
        }

        public string StateCode { get; }

        public DateTime Started { get; }

        public int Duration { get; }
    }
}