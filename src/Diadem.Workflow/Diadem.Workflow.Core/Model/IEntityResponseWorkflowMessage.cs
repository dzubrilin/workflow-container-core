using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Core.Model
{
    public interface IEntityResponseWorkflowMessage : IWorkflowMessage
    {
        string EntityJsonPayload { get; set; }
        
        EntityRequestExecutionStatus ExecutionStatus { get; set; }
    }
}