using System;
using System.Diagnostics;

namespace Diadem.Workflow.Core.Configuration
{
    [DebuggerDisplay("Code=[{Code}], Delay=[{Delay}]")]
    public sealed class AsyncTransitionRetryPolicySimpleConfiguration : AsyncTransitionRetryPolicyConfiguration
    {
        private int _delay;

        public AsyncTransitionRetryPolicySimpleConfiguration(string code) : base(code)
        {
        }

        public int Count { get; set; }

        /// <summary>
        ///     Maximum allowed interval is 100 milliseconds
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