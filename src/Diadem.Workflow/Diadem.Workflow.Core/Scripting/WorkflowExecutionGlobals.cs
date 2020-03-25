using System.Threading;
using Diadem.Workflow.Core.Runtime;

namespace Diadem.Workflow.Core.Scripting
{
    public abstract class WorkflowExecutionGlobals
    {
        public CancellationToken cancellationToken;

        public IRuntimeWorkflowEngine workflowRuntime;
    }
}