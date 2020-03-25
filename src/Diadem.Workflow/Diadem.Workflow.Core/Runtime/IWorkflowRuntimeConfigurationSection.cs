using System.Collections.Generic;
using Diadem.Core.Configuration;

namespace Diadem.Workflow.Core.Runtime
{
    [ConfigurationSection("WORKFLOWRUNTIME")]
    public interface IWorkflowRuntimeConfigurationSection : IConfigurationSection
    {
        string WorkflowHost { get; set; }

        string WorkflowRequestEndpoint { get; set; }
        
        List<WorkflowRuntimeConfigurationEndpoint> WorkflowRuntimeEndpoints { get; set; }
    }
}