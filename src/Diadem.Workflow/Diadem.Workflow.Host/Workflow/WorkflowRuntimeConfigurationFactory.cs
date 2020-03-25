using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.Configuration;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Runtime;

namespace Diadem.Workflow.Host.Workflow
{
    public class WorkflowRuntimeConfigurationFactory : IWorkflowRuntimeConfigurationFactory
    {
        private readonly IConfigurationProvider _configurationProvider;

        public WorkflowRuntimeConfigurationFactory(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public Task<WorkflowRuntimeConfiguration> GetWorkflowRuntimeConfiguration(WorkflowConfiguration workflowConfiguration, CancellationToken cancellationToken)
        {
            var workflowRuntimeConfigurationSection = _configurationProvider.GetSection<IWorkflowRuntimeConfigurationSection>();
            var workflowRuntimeConfiguration = new WorkflowRuntimeConfiguration(workflowConfiguration.Id, workflowConfiguration.Class, workflowConfiguration.Code)
            {
                EndpointConfiguration = new EndpointConfiguration("workflow", EndpointConfigurationType.RabbitMq,
                    new Uri($"{workflowRuntimeConfigurationSection.WorkflowHost}/{workflowRuntimeConfigurationSection.WorkflowRequestEndpoint}"), ConfigurationAuthentication.None),
                EndpointConfigurations = new List<KeyValuePair<string, IEndpointConfiguration>>()
            };

            foreach (var workflowRuntimeConfigurationEndpoint in workflowRuntimeConfigurationSection.WorkflowRuntimeEndpoints)
            {
                IConfigurationAuthentication configurationAuthentication = ConfigurationAuthentication.None;
                if (null != workflowRuntimeConfigurationEndpoint.Authentication)
                {
                    configurationAuthentication = new ConfigurationAuthentication(
                        workflowRuntimeConfigurationEndpoint.Authentication.Type, workflowRuntimeConfigurationEndpoint.Authentication.Parameters);
                }
                
                workflowRuntimeConfiguration.EndpointConfigurations.Add(new KeyValuePair<string, IEndpointConfiguration>(workflowRuntimeConfigurationEndpoint.Code,
                    new EndpointConfiguration(workflowRuntimeConfigurationEndpoint.Code, workflowRuntimeConfigurationEndpoint.Type,
                        new Uri(workflowRuntimeConfigurationEndpoint.Address), configurationAuthentication,
                        workflowRuntimeConfigurationEndpoint.Parameters)));
            };
            
            return Task.FromResult(workflowRuntimeConfiguration);
        }
    }
}