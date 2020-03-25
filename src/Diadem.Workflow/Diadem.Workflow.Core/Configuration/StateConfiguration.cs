using System.Collections.Generic;
using System.Diagnostics;

namespace Diadem.Workflow.Core.Configuration
{
    [DebuggerDisplay("Code=[{Code}], Type=[{Type}]")]
    public sealed class StateConfiguration
    {
        public StateConfiguration()
        {
            Events = new List<StateEventConfiguration>();
            Activities = new List<ActivityConfiguration>();
            Transitions = new List<TransitionConfiguration>();
        }

        public StateConfiguration(string code, StateTypeConfiguration typeConfiguration) : this()
        {
            Code = code;
            Type = typeConfiguration;
        }

        public IList<ActivityConfiguration> Activities { get; }

        public string Code { get; }

        public StateTypeConfiguration Type { get; }

        public IList<StateEventConfiguration> Events { get; }

        public IList<TransitionConfiguration> Transitions { get; }
    }
}