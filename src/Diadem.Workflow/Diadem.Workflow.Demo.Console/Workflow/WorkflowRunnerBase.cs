using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Diadem.Core.Configuration;
using Diadem.Messaging.Core;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Demo.Console.Execution;
using Diadem.Workflow.Provider.RabbitMq;

namespace Diadem.Workflow.Demo.Console.Workflow
{
    public abstract class WorkflowRunnerBase : IWorkflowRunner
    {
        public abstract string Name { get; }

        public virtual IEndpointConfiguration EndpointConfiguration => new EndpointConfiguration(
            "workflow", EndpointConfigurationType.RabbitMq, new Uri("rabbitmq://flow-rabbitmq/Diadem.Workflow.Core.Model:IEventRequestWorkflowMessage"), ConfigurationAuthentication.None);

        public abstract IEnumerable<ICommand> Commands { get; }

        public async Task Run(RunnerContext runnerContext, CancellationToken cancellationToken)
        {
            var busControlFactory = runnerContext.LifetimeScope.Resolve<IBusControlFactory>();
            var busControl = busControlFactory.CreateBusControl();
            var busHandle = await busControl.StartAsync(cancellationToken);

            try
            {
                System.Console.WriteLine($"Starting execution of workflow [{Name}]");
                var workflowMessageTransportFactory = new WorkflowMessageRabbitMqTransportFactory(busControlFactory);
                var workflowCommandContext = CreateWorkflowCommandContext(runnerContext, workflowMessageTransportFactory);
                while (true)
                {
                    var workflowCommand = GetWorkflowCommand(this);
                    if (null == workflowCommand)
                    {
                        System.Console.WriteLine($"Finished execution of workflow [{Name}]");
                        return;
                    }

                    try
                    {
                        System.Console.WriteLine($"Starting command [{workflowCommand.Name}]");
                        await workflowCommand.Execute(workflowCommandContext, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"An error has occurred during command execution [{ex.Message}]");
                    }
                }
            }
            finally
            {
                await busHandle.StopAsync(cancellationToken);
            }
        }

        protected abstract WorkflowCommandContextBase CreateWorkflowCommandContext(RunnerContext runnerContext, WorkflowMessageRabbitMqTransportFactory workflowMessageTransportFactory);

        private static ICommand GetWorkflowCommand(IWorkflowRunner workflowRunner)
        {
            var workflowCommands = workflowRunner.Commands.ToArray();

            for (var i = 0; i < 3; i++)
            {
                var index = 0;
                System.Console.WriteLine("Please select command (E for exit):");
                foreach (var workflowCommand in workflowCommands)
                {
                    System.Console.WriteLine($"[{(++index).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}] - {workflowCommand.Name}");
                }

                var input = System.Console.ReadLine();
                if (string.Equals("E", input, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (!int.TryParse(input, out var position))
                {
                    if (i == 2)
                    {
                        System.Console.WriteLine("Selection failed : ( ... Exiting.");
                        return null;
                    }

                    continue;
                }

                if (position > 0 && position <= workflowCommands.Length)
                {
                    return workflowCommands[position - 1];
                }

                if (i == 2)
                {
                    System.Console.WriteLine("Selection failed : ( ... Exiting.");
                    return null;
                }

                System.Console.WriteLine($"Input must be in a range [1..{workflowCommands.Length}]");
            }

            return null;
        }
    }
}