using Autofac;
using Diadem.Workflow.Core.Runtime;

namespace Diadem.Workflow.Demo.Console.Execution
{
    public class CommandContext : ICommandContext
    {
        public CommandContext(RunnerContext runnerContext, IWorkflowMessageTransport workflowMessageTransport)
        {
            RunnerContext = runnerContext;
            WorkflowMessageTransport = workflowMessageTransport;
        }

        public RunnerContext RunnerContext { get; }

        public IWorkflowMessageTransport WorkflowMessageTransport { get; }
    }
}