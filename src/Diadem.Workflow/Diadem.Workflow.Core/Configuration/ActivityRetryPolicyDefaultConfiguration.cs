using System;
using System.Diagnostics;

namespace Diadem.Workflow.Core.Configuration
{
    [DebuggerDisplay("Count=[{Count}], Delay=[{Delay}], OnFailureTransitionToUse=[{OnFailureTransitionToUse}]")]
    public sealed class ActivityRetryPolicyDefaultConfiguration : ActivityRetryPolicyConfiguration
    {
        public static readonly ActivityRetryPolicyConfiguration Instance = new ActivityRetryPolicyDefaultConfiguration();
        
        public ActivityRetryPolicyDefaultConfiguration()
        {
        }

        public override int Count => 1;

        public override int Delay => 10;

        public override string OnFailureTransitionToUse => string.Empty;
    }
}