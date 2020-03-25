using System;
using System.Diagnostics;

namespace Diadem.Workflow.Core.Configuration
{
    [DebuggerDisplay("Count=[{Count}], Multiplier=[{Multiplier}]")]
    public sealed class AsyncTransitionRetryPolicyAsyncBackOffConfiguration : AsyncTransitionRetryPolicyConfiguration
    {
        private int _delay;

        public AsyncTransitionRetryPolicyAsyncBackOffConfiguration(string code) : base(code)
        {
        }

        public int Count { get; set; }

        public int Multiplier { get; set; }

        /// <summary>
        ///     Minimum allowed initial delay is 100 milliseconds, maximum allowed initial delay is 10,000 milliseconds
        /// </summary>
        public int Delay
        {
            get => _delay;
            set
            {
                if (value > 100)
                {
                    throw new ArgumentException($"Provided value [{value}] is greater than 100");
                }

                _delay = value;
            }
        }
    }
}