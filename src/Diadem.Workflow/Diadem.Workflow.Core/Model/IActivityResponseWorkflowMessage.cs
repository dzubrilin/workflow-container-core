using Diadem.Workflow.Core.Execution.Activities;

namespace Diadem.Workflow.Core.Model
{
    public interface IActivityResponseWorkflowMessage : IWorkflowMessage
    {
        ActivityExecutionResult ActivityExecutionResult { get; set; }
    }
}