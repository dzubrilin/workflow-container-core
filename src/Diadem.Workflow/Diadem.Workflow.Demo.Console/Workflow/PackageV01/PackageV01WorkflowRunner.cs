using System;
using System.Collections.Generic;
using Autofac;
using Diadem.Workflow.Demo.Console.Execution;
using Diadem.Workflow.Provider.RabbitMq;

namespace Diadem.Workflow.Demo.Console.Workflow.PackageV01
{
    public class PackageV01WorkflowRunner : WorkflowRunnerBase, IWorkflowRunner
    {
        public string EntityType => "Workflow.Demo.DemoSign.Model.Package";

        public Guid WorkflowId => Guid.Parse("F420998A-0B37-41E8-8332-62959D847504");

        public override string Name => "Test v.0.0.2";

        public override IEnumerable<ICommand> Commands =>
            new ICommand[]
            {
                new SendInitialEventMessageCommand(),
                new SendProcessEventMessageCommand()
            };

        protected override WorkflowCommandContextBase CreateWorkflowCommandContext(RunnerContext runnerContext, WorkflowMessageRabbitMqTransportFactory workflowMessageTransportFactory)
        {
            return new PackageV01WorkflowCommandContext(runnerContext, workflowMessageTransportFactory.CreateMessageTransport(EndpointConfiguration.Address));
        }
    }
}