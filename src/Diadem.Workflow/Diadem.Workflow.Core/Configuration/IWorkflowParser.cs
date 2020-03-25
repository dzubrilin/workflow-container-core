namespace Diadem.Workflow.Core.Configuration
{
    public interface IWorkflowParser
    {
        WorkflowConfiguration Parse(string content);
    }
}