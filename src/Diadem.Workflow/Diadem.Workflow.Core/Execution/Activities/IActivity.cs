using System.Threading;
using System.Threading.Tasks;

namespace Diadem.Workflow.Core.Execution.Activities
{
    public interface IActivity
    {
        string Code { get; }

        Task<ActivityExecutionResult> Execute(ActivityExecutionContext activityExecutionContext, CancellationToken cancellationToken = default);
    }
}