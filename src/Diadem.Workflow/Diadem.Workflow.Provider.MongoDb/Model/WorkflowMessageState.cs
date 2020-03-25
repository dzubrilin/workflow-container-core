using System;

namespace Diadem.Workflow.Provider.MongoDb.Model
{
    internal class WorkflowMessageStateDto
    {
        public DateTime Created { get; set; }

        public string JsonState { get; set; }

        public string WorkflowInstanceId { get; set; }

        public string WorkflowMessageId { get; set; }
    }
}