using System;
using System.Diagnostics;

namespace Diadem.Workflow.Core.Configuration
{
    /// <summary>
    ///     Activity retry policy is in-place retry logic intended to make a few retry attempts within milliseconds interval
    /// </summary>
    [DebuggerDisplay("Count=[{Count}], Delay=[{Delay}], OnFailureTransitionToUse=[{OnFailureTransitionToUse}]")]
    public class ActivityRetryPolicyConfiguration
    {
        public ActivityRetryPolicyConfiguration()
        {
        }

        public ActivityRetryPolicyConfiguration(int count, int delay, string onFailureTransitionToUse)
        {
            if (delay > 100)
            {
                throw new ArgumentException($"Provided value [{delay}] is greater than 100");
            }

            Count = count;
            Delay = delay;
            OnFailureTransitionToUse = onFailureTransitionToUse;
        }

        public virtual int Count { get; }

        /// <summary>
        ///     Maximum allowed interval is 100 milliseconds
        /// </summary>
        public virtual int Delay { get; }

        public virtual string OnFailureTransitionToUse { get; }
    }
}