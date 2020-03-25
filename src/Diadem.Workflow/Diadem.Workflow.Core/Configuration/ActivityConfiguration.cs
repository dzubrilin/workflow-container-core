using System.Diagnostics;

namespace Diadem.Workflow.Core.Configuration
{
    [DebuggerDisplay("Type=[{Type}], Code=[{Code}]")]
    public sealed class ActivityConfiguration
    {
        public ActivityConfiguration()
        {
            RetryPolicy = ActivityRetryPolicyDefaultConfiguration.Instance;
        }

        public ActivityConfiguration(ActivityTypeConfiguration type,
            string code, string parameters,
            ScriptTypeConfiguration scriptType, string script,
            ActivityRetryPolicyConfiguration retryPolicy)
        {
            Type = type;
            Code = code;
            Parameters = parameters;
            RetryPolicy = retryPolicy;
            Script = script;
            ScriptType = scriptType;
        }

        public string Code { get; }

        public string Parameters { get; }

        public string Script { get; }

        public ScriptTypeConfiguration ScriptType { get; }

        public ActivityTypeConfiguration Type { get; }

        public ActivityRetryPolicyConfiguration RetryPolicy { get; }
    }
}