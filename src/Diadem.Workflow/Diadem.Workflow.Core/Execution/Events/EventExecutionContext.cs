using Diadem.Workflow.Core.Configuration;

namespace Diadem.Workflow.Core.Execution.Events
{
    public class EventExecutionContext
    {
        public EventExecutionContext(WorkflowContext workflowContext, IEvent @event, EventConfiguration eventConfiguration, StateEventConfiguration stateEventConfiguration)
        {
            Event = @event;
            EventConfiguration = eventConfiguration;
            StateEventConfiguration = stateEventConfiguration;
            WorkflowContext = workflowContext;
        }

        public IEvent Event { get; }

        public EventConfiguration EventConfiguration { get; }

        public StateEventConfiguration StateEventConfiguration { get; }

        public WorkflowContext WorkflowContext { get; }
    }
}