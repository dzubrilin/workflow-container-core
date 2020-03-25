using System.Collections.Generic;
using Diadem.Core.Configuration;

namespace Diadem.Workflow.Core.Runtime
{
    [JsonSerializableConfigurationValue]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WorkflowRuntimeConfigurationEndpoint
    {
        public string Code { get; set; }
        
        public string Address { get; set; }
        
        public List<KeyValuePair<string, string>> Parameters { get; set; }
        
        public EndpointConfigurationType Type { get; set; }
        
        public WorkflowRuntimeConfigurationAuthentication Authentication { get; set; }
    }
}