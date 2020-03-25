using System.Diagnostics;

namespace Diadem.Workflow.Core.Configuration
{
    [DebuggerDisplay("Code=[{Code}]")]
    public class AsyncTransitionRetryPolicyConfiguration
    {
        protected AsyncTransitionRetryPolicyConfiguration(string code)
        {
            Code = code;
        }

        public string Code { get; }
    }
}