using System;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.Configuration;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Model;

namespace Diadem.Workflow.Core.Runtime
{
    public interface IWorkflowMessageTransport
    {
        Task<TResponseWorkflowMessage> Request<TRequestWorkflowMessage, TResponseWorkflowMessage>(
            IEndpointConfiguration endpointConfiguration, TRequestWorkflowMessage workflowMessage, CancellationToken cancellationToken)
            where TRequestWorkflowMessage : class, IWorkflowMessage
            where TResponseWorkflowMessage : class, IWorkflowMessage;

        Task Send<TWorkflowMessage>(IEndpointConfiguration endpointConfiguration, TWorkflowMessage workflowMessage, CancellationToken cancellationToken)
            where TWorkflowMessage : class, IWorkflowMessage;

        Task SendWithDelay<TWorkflowMessage>(IEndpointConfiguration endpointConfiguration, TWorkflowMessage workflowMessage, CancellationToken cancellationToken)
            where TWorkflowMessage : class, IDelayedWorkflowMessage;
    }
}