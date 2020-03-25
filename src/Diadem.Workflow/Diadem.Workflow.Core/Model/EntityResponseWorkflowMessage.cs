using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Core.Model
{
    public class EntityResponseWorkflowMessage : WorkflowMessage, IEntityResponseWorkflowMessage
    {
        public string EntityJsonPayload { get; set; }
        
        public EntityRequestExecutionStatus ExecutionStatus { get; set; }
    }
}