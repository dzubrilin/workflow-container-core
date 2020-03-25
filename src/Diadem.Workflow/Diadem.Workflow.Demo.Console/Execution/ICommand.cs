using System.Threading;
using System.Threading.Tasks;

namespace Diadem.Workflow.Demo.Console.Execution
{
    public interface ICommand
    {
        string Name { get; }

        Task Execute(ICommandContext commandContext, CancellationToken cancellationToken);
    }
}