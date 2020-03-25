using System;
using System.Collections.Generic;

namespace Diadem.Workflow.Core.Configuration
{
    public sealed class WorkflowConfiguration
    {
        public WorkflowConfiguration()
        {
            Events = new List<EventConfiguration>();
            States = new List<StateConfiguration>();
        }

        public WorkflowConfiguration(Guid id, string @class, string code, string name, Version version) : this()
        {
            Id = id;
            Class = @class;
            Code = code;
            Name = name;
            Version = version;
        }

        public Guid Id { get; }

        public string Class { get; }

        public string Code { get; }

        public IList<EventConfiguration> Events { get; }

        public string Name { get; }

        public WorkflowRuntimeConfiguration RuntimeConfiguration { get; set; }

        public IList<StateConfiguration> States { get; }

        public Version Version { get; }
    }
}