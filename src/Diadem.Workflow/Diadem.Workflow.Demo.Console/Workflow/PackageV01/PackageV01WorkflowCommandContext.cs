using System;
using Autofac;
using Diadem.Workflow.Core.Runtime;
using Diadem.Workflow.Demo.Console.Execution;

namespace Diadem.Workflow.Demo.Console.Workflow.PackageV01
{
    public class PackageV01WorkflowCommandContext : WorkflowCommandContextBase
    {
        public PackageV01WorkflowCommandContext(RunnerContext runnerContext, IWorkflowMessageTransport workflowMessageTransport)
            : base(runnerContext, workflowMessageTransport)
        {
        }

        public Guid WorkflowInstanceId { get; set; }
    }
}