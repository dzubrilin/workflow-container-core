using System;
using System.Collections.Generic;
using Diadem.Core.Configuration;

namespace Diadem.Workflow.Core.Configuration
{
    public sealed class WorkflowRuntimeConfiguration
    {
        public WorkflowRuntimeConfiguration(Guid workflowId, string workflowClass, string workflowCode)
        {
            WorkflowClass = workflowClass;
            WorkflowCode = workflowCode;
            WorkflowId = workflowId;
        }

        public string WorkflowClass { get; }

        public string WorkflowCode { get; }

        public Guid WorkflowId { get; }

        public IEndpointConfiguration EndpointConfiguration { get; set; }

        public IList<KeyValuePair<string, IEndpointConfiguration>> EndpointConfigurations { get; set; }
    }
}