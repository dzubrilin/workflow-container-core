using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Diadem.Workflow.Provider.MongoDb.Model
{
    public class WorkflowInstanceLogDto
    {
        public WorkflowInstanceLogDto()
        {
            WorkflowInstanceActivityLog = new List<WorkflowInstanceActivityLogDto>();
            WorkflowInstanceEventLog = new List<WorkflowInstanceEventLogDto>();
            WorkflowInstanceMessageLog = new List<WorkflowInstanceMessageLogDto>();
            WorkflowInstanceStateLog = new List<WorkflowInstanceStateLogDto>();
        }

        [BsonId] public ObjectId ObjectId { get; set; }

        public string WorkflowInstanceId { get; set; }

        public List<WorkflowInstanceActivityLogDto> WorkflowInstanceActivityLog { get; set; }

        public List<WorkflowInstanceEventLogDto> WorkflowInstanceEventLog { get; set; }

        public List<WorkflowInstanceMessageLogDto> WorkflowInstanceMessageLog { get; set; }

        public List<WorkflowInstanceStateLogDto> WorkflowInstanceStateLog { get; set; }
    }
}