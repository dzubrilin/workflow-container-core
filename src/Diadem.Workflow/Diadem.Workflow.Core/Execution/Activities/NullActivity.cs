using System.Threading;
using System.Threading.Tasks;

namespace Diadem.Workflow.Core.Execution.Activities
{
    public class NullActivity : IActivity
    {
        public NullActivity(string code)
        {
            Code = code;
        }

        public string Code { get; }

        public Task<ActivityExecutionResult> Execute(ActivityExecutionContext activityExecutionContext, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ActivityExecutionResult(ActivityExecutionStatus.Completed));
        }
    }
}