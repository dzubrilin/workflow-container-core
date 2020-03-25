using Diadem.Workflow.Core.Execution.Events;

namespace Diadem.Workflow.Core.Execution.EventHandlers
{
    internal interface ICodeScriptEventHandler : IEventHandler
    {
        EventExecutionResult Execute(EventExecutionContext eventExecutionContext);
    }
}