using System.Collections.Generic;
using Diadem.Core.Configuration;

namespace Diadem.Workflow.Core.Runtime
{
    [JsonSerializableConfigurationValue]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WorkflowRuntimeConfigurationAuthentication
    {
        public ConfigurationAuthenticationType Type { get; set; }

        public List<KeyValuePair<string, string>> Parameters { get; set; }
    }
}