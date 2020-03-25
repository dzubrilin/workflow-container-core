using Diadem.Workflow.Core.Model;

namespace Diadem.Workflow.Core.Execution.Messages
{
    public class MessageExecutionContext
    {
        public MessageExecutionContext(WorkflowContext workflowContext, IWorkflowMessage workflowMessage)
        {
            WorkflowContext = workflowContext;
            WorkflowMessage = workflowMessage;
        }

        public IWorkflowMessage WorkflowMessage { get; }

        public WorkflowContext WorkflowContext { get; }
    }
}