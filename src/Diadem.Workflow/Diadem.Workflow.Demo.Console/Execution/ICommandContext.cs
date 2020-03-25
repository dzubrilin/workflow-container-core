using Autofac;
using Diadem.Workflow.Core.Runtime;

namespace Diadem.Workflow.Demo.Console.Execution
{
    public interface ICommandContext
    {
        RunnerContext RunnerContext { get; }

        IWorkflowMessageTransport WorkflowMessageTransport { get; }
    }
}