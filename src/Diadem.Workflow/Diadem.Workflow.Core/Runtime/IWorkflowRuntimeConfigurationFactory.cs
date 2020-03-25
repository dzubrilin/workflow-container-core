using System;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Configuration;

namespace Diadem.Workflow.Core.Runtime
{
    public interface IWorkflowRuntimeConfigurationFactory
    {
        Task<WorkflowRuntimeConfiguration> GetWorkflowRuntimeConfiguration(WorkflowConfiguration workflowConfiguration, CancellationToken cancellationToken);
    }
}