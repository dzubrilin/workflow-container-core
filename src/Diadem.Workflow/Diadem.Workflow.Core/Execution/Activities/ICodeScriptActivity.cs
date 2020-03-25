namespace Diadem.Workflow.Core.Execution.Activities
{
    public interface ICodeScriptActivity : IActivity
    {
        string Script { get; }
    }
}