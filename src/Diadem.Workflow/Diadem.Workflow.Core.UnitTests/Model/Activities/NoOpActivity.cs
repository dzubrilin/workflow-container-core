using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Execution.Activities;

namespace Diadem.Workflow.Core.UnitTests.Model.Activities
{
    public class NoOpActivity : IActivity
    {
        public string Code => "NoOp";

        public Task<ActivityExecutionResult> Execute(ActivityExecutionContext activityExecutionContext, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ActivityExecutionResult(ActivityExecutionStatus.Completed));
        }
    }
}