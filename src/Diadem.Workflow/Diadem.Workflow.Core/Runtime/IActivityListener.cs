using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Execution.Activities;

namespace Diadem.Workflow.Core.Runtime
{
    public interface IActivityListener
    {
        Task AfterExecutionActivity(IActivity activity, ActivityExecutionContext activityExecutionContext,
            ActivityExecutionResult activityExecutionResult, CancellationToken cancellationToken = default);

        Task BeforeExecutingActivity(IActivity activity, ActivityExecutionContext activityExecutionContext,
            CancellationToken cancellationToken = default);
    }
}