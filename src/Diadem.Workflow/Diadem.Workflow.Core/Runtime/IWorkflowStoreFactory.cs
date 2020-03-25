namespace Diadem.Workflow.Core.Runtime
{
    public interface IWorkflowStoreFactory
    {
        IWorkflowStore Create();
    }
}