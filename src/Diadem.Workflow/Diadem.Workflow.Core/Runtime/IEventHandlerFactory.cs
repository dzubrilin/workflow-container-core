using Diadem.Workflow.Core.Execution.EventHandlers;

namespace Diadem.Workflow.Core.Runtime
{
    public interface IEventHandlerFactory
    {
        ICodeEventHandler CreateEventHandler(string code);
    }
}