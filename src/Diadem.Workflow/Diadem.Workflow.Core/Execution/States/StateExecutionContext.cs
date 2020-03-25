using Diadem.Workflow.Core.Configuration;

namespace Diadem.Workflow.Core.Execution.States
{
    public class StateExecutionContext
    {
        public StateExecutionContext(WorkflowContext workflowContext, StateConfiguration stateConfiguration)
        {
            StateConfiguration = stateConfiguration;
            WorkflowContext = workflowContext;
        }

        public StateConfiguration StateConfiguration { get; }

        public WorkflowContext WorkflowContext { get; }
    }
}