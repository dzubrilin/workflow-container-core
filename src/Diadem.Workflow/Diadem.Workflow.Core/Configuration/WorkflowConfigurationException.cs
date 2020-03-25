using System;
using System.Collections.Generic;

namespace Diadem.Workflow.Core.Configuration
{
    public sealed class WorkflowConfigurationException : Exception
    {
        public WorkflowConfigurationException()
        {
            ValidationMessages = new List<string>();
        }

        public WorkflowConfigurationException(string message) : base(message)
        {
            ValidationMessages = new List<string>();
        }

        public WorkflowConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
            ValidationMessages = new List<string>();
        }

        public IList<string> ValidationMessages { get; }
    }
}