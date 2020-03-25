using System.Threading;
using System.Threading.Tasks;

namespace Diadem.Workflow.Core.Execution.Activities
{
    internal interface IActivityProcessor
    {
        Task<ActivityExecutionResult> Process(ActivityExecutionContext activityExecutionContext, CancellationToken cancellationToken = default);
    }
}