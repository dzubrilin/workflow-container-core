using System.Diagnostics;

namespace Diadem.Workflow.Core.Configuration
{
    [DebuggerDisplay("Type=[{Type}], Code=[{Code}]")]
    public sealed class EventConfiguration
    {
        public EventConfiguration()
        {
        }

        public EventConfiguration(EventTypeConfiguration type, string code, string parameters)
        {
            Type = type;
            Code = code;
            Parameters = parameters;
        }

        public string Code { get; }

        public string Parameters { get; }

        public EventTypeConfiguration Type { get; }
    }
}