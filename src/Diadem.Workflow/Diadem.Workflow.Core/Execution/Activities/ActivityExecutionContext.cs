using System;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution.States;

namespace Diadem.Workflow.Core.Execution.Activities
{
    public sealed class ActivityExecutionContext
    {
        public ActivityExecutionContext(StateExecutionContext stateExecutionContext,
            ActivityConfiguration activityConfiguration)
        {
            ActivityConfiguration = activityConfiguration;
            StateExecutionContext = stateExecutionContext;
        }

        public ActivityConfiguration ActivityConfiguration { get; }

        public Exception Exception { get; internal set; }

        public StateExecutionContext StateExecutionContext { get; }
    }
}