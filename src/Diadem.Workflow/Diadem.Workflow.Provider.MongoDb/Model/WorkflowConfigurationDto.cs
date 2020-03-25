using System;

namespace Diadem.Workflow.Provider.MongoDb.Model
{
    internal class WorkflowConfigurationDto
    {
        public string Id { get; set; }

        public string Content { get; set; }

        public DateTime Created { get; set; }

        public string WorkflowClassId { get; set; }
    }
}