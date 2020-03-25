using System;

namespace Diadem.Workflow.Core
{
    public sealed class WorkflowException : Exception
    {
        public WorkflowException(string message) : base(message)
        {
        }

        public WorkflowException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}