using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Model;

namespace Diadem.Workflow.Core
{
    public class WorkflowProcessingResult
    {
        public WorkflowProcessingResult(IWorkflowInstance workflowInstance)
        {
            WorkflowInstance = workflowInstance;
        }

        public WorkflowProcessingResult(IWorkflowInstance workflowInstance, JsonState workflowExecutionState)
        {
            WorkflowInstance = workflowInstance;
            WorkflowExecutionState = workflowExecutionState;
        }

        public IWorkflowInstance WorkflowInstance { get; }

        public JsonState WorkflowExecutionState { get; }
    }
}