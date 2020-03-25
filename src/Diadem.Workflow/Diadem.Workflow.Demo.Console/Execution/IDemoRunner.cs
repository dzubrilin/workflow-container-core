using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Diadem.Core.Configuration;
using Diadem.Workflow.Core.Configuration;

namespace Diadem.Workflow.Demo.Console.Execution
{
    public interface IDemoRunner
    {
        string Name { get; }

        IEndpointConfiguration EndpointConfiguration { get; }

        IEnumerable<ICommand> Commands { get; }

        Task Run(RunnerContext runnerContext, CancellationToken cancellationToken);
    }
}