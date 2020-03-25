using System;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Model;
using Serilog;

namespace Diadem.Workflow.Core.Execution.Activities
{
    /// <summary>
    ///     Delegates execution of an activity to a remote service/host through the provided endpoint
    /// </summary>
    public sealed class RemoteActivity : IActivity
    {
        public RemoteActivity(string code)
        {
            Code = code;
        }

        public string Code { get; }

        public async Task<ActivityExecutionResult> Execute(ActivityExecutionContext activityExecutionContext, CancellationToken cancellationToken = default)
        {
            var remoteCode = Code.GetRemoteCode();
            var remoteExecutionState = activityExecutionContext.StateExecutionContext.WorkflowContext
                .CreateRemoteExecutionState(activityExecutionContext.ActivityConfiguration.Parameters, remoteCode);
            var activityRequestWorkflowMessage = new ActivityRequestWorkflowMessage(remoteCode)
            {
                WorkflowMessageId = Guid.NewGuid(),
                WorkflowInstanceId = activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id,
                WorkflowId = activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowConfiguration.Id,
                State = new WorkflowMessageState(remoteExecutionState),
            };

            var workflowConfiguration = activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowConfiguration;
            var endpointConfiguration = workflowConfiguration.FindEndpointConfiguration(Code);

            Log.Verbose("Sending activity request message [{code}::{messageId}] to endpoint [{endpoint}]",
                Code, activityRequestWorkflowMessage.WorkflowMessageId, endpointConfiguration.Address);

            var workflowMessageTransport = activityExecutionContext.StateExecutionContext.WorkflowContext
                                                                   .WorkflowMessageTransportFactoryProvider.CreateMessageTransportFactory(endpointConfiguration.Type)
                                                                   .CreateMessageTransport(endpointConfiguration.Address);
            var responseWorkflowMessage = await workflowMessageTransport.Request<IActivityRequestWorkflowMessage, IActivityResponseWorkflowMessage>(
                endpointConfiguration, activityRequestWorkflowMessage, cancellationToken).ConfigureAwait(false);

            Log.Verbose("Received activity response message {messageId} from {endpoint} with {status}",
                activityRequestWorkflowMessage.WorkflowMessageId, endpointConfiguration.Address, responseWorkflowMessage.ActivityExecutionResult?.Status);

            // merge state changes made by remote activity during execution
            activityExecutionContext.StateExecutionContext.WorkflowContext.MergeRemoteExecutionState(responseWorkflowMessage.State);
            return responseWorkflowMessage.ActivityExecutionResult;
        }
    }
}