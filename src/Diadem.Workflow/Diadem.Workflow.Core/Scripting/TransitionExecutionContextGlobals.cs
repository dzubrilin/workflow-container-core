using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Execution.Transitions;

namespace Diadem.Workflow.Core.Scripting
{
    public class TransitionExecutionContextGlobals : WorkflowExecutionGlobals
    {
        public TransitionEvaluationContext context;

        public IEntity entity;

        public JsonState state;
    }
}