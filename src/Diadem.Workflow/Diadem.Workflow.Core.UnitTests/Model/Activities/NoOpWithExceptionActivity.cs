using System;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Execution.Activities;

namespace Diadem.Workflow.Core.UnitTests.Model.Activities
{
    public class NoOpWithExceptionActivity : IActivity
    {
        public string Code => "NoOp";

        public Task<ActivityExecutionResult> Execute(ActivityExecutionContext activityExecutionContext, CancellationToken cancellationToken = default)
        {
            var workflowInstance = activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance;
            if (string.Equals(workflowInstance.CurrentStateCode, "process", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception();
            }

            if (string.Equals(workflowInstance.CurrentStateCode, "awaitingBadges", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception();
            }

            return Task.FromResult(new ActivityExecutionResult(ActivityExecutionStatus.Completed));
        }
    }
}