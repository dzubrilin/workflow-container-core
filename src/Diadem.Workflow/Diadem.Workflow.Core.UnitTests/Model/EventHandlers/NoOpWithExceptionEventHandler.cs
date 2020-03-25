using System;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Execution.EventHandlers;
using Diadem.Workflow.Core.Execution.Events;

namespace Diadem.Workflow.Core.UnitTests.Model.EventHandlers
{
    public class NoOpWithExceptionEventHandler : ICodeEventHandler
    {
        public string Code => "noOp";

        public Task<EventExecutionResult> Execute(EventExecutionContext eventExecutionContext, CancellationToken cancellationToken = default)
        {
            if (string.Equals(eventExecutionContext.WorkflowContext.WorkflowInstance.CurrentStateCode, "process", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception();
            }

            return Task.FromResult(new EventExecutionResult(EventExecutionStatus.Completed));
        }
    }
}