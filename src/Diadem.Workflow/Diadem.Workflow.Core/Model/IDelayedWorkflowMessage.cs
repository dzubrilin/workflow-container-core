using System;

namespace Diadem.Workflow.Core.Model
{
    public interface IDelayedWorkflowMessage : IWorkflowMessage
    {
        TimeSpan Delay { get; }
    }
}