using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution.Events;
using Diadem.Workflow.Core.Model;
using Serilog;

namespace Diadem.Workflow.Core.Execution.EventHandlers
{
    /// <summary>
    ///     Delegates execution of an event handler logic to a remote service/host through the provided endpoint
    /// </summary>
    public sealed class RemoteEventHandler : ICodeEventHandler
    {
        public RemoteEventHandler(string code)
        {
            Code = code;
        }

        public string Code { get; }

        public async Task<EventExecutionResult> Execute(EventExecutionContext eventExecutionContext, CancellationToken cancellationToken = default)
        {
            var remoteCode = Code.GetRemoteCode();
            var remoteExecutionState = eventExecutionContext.WorkflowContext.CreateRemoteExecutionState(
                eventExecutionContext.EventConfiguration.Parameters, remoteCode);
            var eventRequestWorkflowMessage = new EventRequestWorkflowMessage(eventExecutionContext.WorkflowContext.WorkflowConfiguration.Id,
                eventExecutionContext.WorkflowContext.WorkflowInstance.Id, remoteCode, remoteExecutionState);

            var workflowConfiguration = eventExecutionContext.WorkflowContext.WorkflowConfiguration;
            var endpointConfiguration = workflowConfiguration.FindEndpointConfiguration(Code);

            Log.Verbose("Sending event request message [{code}::{messageId}] to {endpoint} [{workflowInstanceId}]",
                Code, eventRequestWorkflowMessage.WorkflowMessageId, endpointConfiguration.Address,
                eventExecutionContext.WorkflowContext.WorkflowInstance.Id);

            var messageTransport = eventExecutionContext.WorkflowContext
                                                        .WorkflowMessageTransportFactoryProvider.CreateMessageTransportFactory(endpointConfiguration.Type)
                                                        .CreateMessageTransport(endpointConfiguration.Address);
            var responseWorkflowMessage = await messageTransport.Request<IEventRequestWorkflowMessage, IEventResponseWorkflowMessage>(
                endpointConfiguration, eventRequestWorkflowMessage, cancellationToken).ConfigureAwait(false);

            Log.Verbose("Received event response message [{messageId}] from {endpoint} [{workflowInstanceId}]",
                eventRequestWorkflowMessage.WorkflowMessageId, endpointConfiguration.Address,
                eventExecutionContext.WorkflowContext.WorkflowInstance.Id);

            // merge state changes made by remote event handler during execution
            eventExecutionContext.WorkflowContext.MergeRemoteExecutionState(responseWorkflowMessage.State);
            return responseWorkflowMessage.EventExecutionResult;
        }
    }
}