using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Execution.Activities;

namespace Diadem.Workflow.Core.Scripting
{
    public class ActivityExecutionContextGlobals : WorkflowExecutionGlobals
    {
        public ActivityExecutionContext context;

        public IEntity entity;

        public JsonState state;
    }
}