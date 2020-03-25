using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Execution.Events;

namespace Diadem.Workflow.Core.Scripting
{
    public class EventHandlerExecutionContextGlobals : WorkflowExecutionGlobals
    {
        public EventExecutionContext context;

        public IEntity entity;

        public JsonState state;
    }
}