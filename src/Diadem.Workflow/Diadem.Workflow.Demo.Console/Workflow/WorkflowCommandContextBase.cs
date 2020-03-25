using System;
using Autofac;
using Diadem.Workflow.Core.Runtime;
using Diadem.Workflow.Demo.Console.Execution;

namespace Diadem.Workflow.Demo.Console.Workflow
{
    public abstract class WorkflowCommandContextBase : CommandContext, ICommandContext
    {
        public WorkflowCommandContextBase(RunnerContext runnerContext, IWorkflowMessageTransport workflowMessageTransport)
            : base(runnerContext, workflowMessageTransport)
        {
        }
    }
}