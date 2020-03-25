using Diadem.Workflow.Core.Execution.States;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Diadem.Workflow.Provider.MongoDb.Model
{
    internal class WorkflowInstanceDto
    {
        public BsonDateTime Created { get; set; }

        public string CurrentStateCode { get; set; }

        public StateExecutionProgress CurrentStateProgress { get; set; }

        public string EntityId { get; set; }

        public string EntityType { get; set; }

        public string Id { get; set; }

        // TODO: use this native generated [_id_] by MongoDB instead of generated GUID
        [BsonId]
        public ObjectId ObjectId { get; set; }

        public string ParentWorkflowInstanceId { get; set; }

        public string RootWorkflowInstanceId { get; set; }

        public string WorkflowId { get; set; }

        public WorkflowInstanceLockDto Lock { get; set; }
    }
}