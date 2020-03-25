using Diadem.Workflow.Core;

namespace Diadem.Workflow.Host.Workflow
{
    public interface IWorkflowEngineFactory
    {
        IWorkflowEngine CreateWorkflowEngine();
    }
}