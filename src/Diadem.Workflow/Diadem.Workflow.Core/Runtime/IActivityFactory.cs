using Diadem.Workflow.Core.Execution.Activities;

namespace Diadem.Workflow.Core.Runtime
{
    public interface IActivityFactory
    {
        IActivity CreateActivity(string code);
    }
}