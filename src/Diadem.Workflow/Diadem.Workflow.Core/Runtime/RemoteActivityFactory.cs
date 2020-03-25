using System;
using Diadem.Workflow.Core.Execution.Activities;

namespace Diadem.Workflow.Core.Runtime
{
    public class RemoteActivityFactory : IActivityFactory
    {
        public IActivity CreateActivity(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }

            if (string.Equals("noOp", code, StringComparison.OrdinalIgnoreCase))
            {
                return new NullActivity(code);
            }

            return code.StartsWith("remote.", StringComparison.OrdinalIgnoreCase)
                ? new RemoteActivity(code)
                : null;
        }
    }
}