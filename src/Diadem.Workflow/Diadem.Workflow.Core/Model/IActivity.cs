namespace Flow.Core.Model
{
    public interface IActivity
    {
        string Code { get; }

        ActivityExecutionResult Execute(WorkflowContext context);
    }
}