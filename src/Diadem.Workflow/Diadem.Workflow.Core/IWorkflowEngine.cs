using System;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Execution.Events;
using Diadem.Workflow.Core.Model;

namespace Diadem.Workflow.Core
{
    public interface IWorkflowEngine
    {
        Guid Id { get; }

        Task<WorkflowProcessingResult> ProcessEvent(Guid workflowId, IEvent @event);

        Task<WorkflowProcessingResult> ProcessEvent(Guid workflowId, IEvent @event, CancellationToken cancellationToken);

        Task<WorkflowProcessingResult> ProcessMessage(IWorkflowMessage workflowMessage);

        Task<WorkflowProcessingResult> ProcessMessage(IWorkflowMessage workflowMessage, CancellationToken cancellationToken);
    }
}