using System;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Model;
using Serilog;

namespace Diadem.Workflow.Core.Execution.Transitions
{
    public class RemoteTransition : ITransition
    {
        public RemoteTransition(string code)
        {
            Code = code;
        }

        public string Code { get; }

        public async Task<TransitionEvaluationResult> Evaluate(TransitionEvaluationContext transitionEvaluationContext, CancellationToken cancellationToken)
        {
            var remoteCode = Code.GetRemoteCode();
            var remoteExecutionState = transitionEvaluationContext.StateExecutionContext.WorkflowContext
                .CreateRemoteExecutionState(transitionEvaluationContext.TransitionConfiguration.Parameters, remoteCode);
            var activityRequestWorkflowMessage = new TransitionEvaluationRequestWorkflowMessage(remoteCode)
            {
                WorkflowMessageId = Guid.NewGuid(),
                WorkflowInstanceId = transitionEvaluationContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id,
                WorkflowId = transitionEvaluationContext.StateExecutionContext.WorkflowContext.WorkflowConfiguration.Id,
                State = new WorkflowMessageState(remoteExecutionState)
            };

            var workflowConfiguration = transitionEvaluationContext.StateExecutionContext.WorkflowContext.WorkflowConfiguration;
            var endpointConfiguration = workflowConfiguration.FindEndpointConfiguration(Code);

            Log.Verbose("Sending transition evaluation request message [{code}::{messageId}] to {endpoint} [{workflowInstanceId}]",
                Code, activityRequestWorkflowMessage.WorkflowMessageId, endpointConfiguration.Address,
                transitionEvaluationContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id);

            var messageTransport = transitionEvaluationContext.StateExecutionContext.WorkflowContext
                                                              .WorkflowMessageTransportFactoryProvider.CreateMessageTransportFactory(endpointConfiguration.Type)
                                                              .CreateMessageTransport(endpointConfiguration.Address);
            var responseWorkflowMessage = await messageTransport.Request<ITransitionEvaluationRequestWorkflowMessage, ITransitionEvaluationResponseWorkflowMessage>(
                endpointConfiguration, activityRequestWorkflowMessage, cancellationToken).ConfigureAwait(false);

            Log.Verbose("Received transition evaluation response message [{messageId}] from {endpoint} [{workflowInstanceId}]",
                activityRequestWorkflowMessage.WorkflowMessageId, endpointConfiguration.Address,
                transitionEvaluationContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id);

            // merge state changes made by remote transition handler during execution
            transitionEvaluationContext.StateExecutionContext.WorkflowContext.MergeRemoteExecutionState(responseWorkflowMessage.State);
            return new TransitionEvaluationResult(responseWorkflowMessage.TransitionEvaluationStatus);
        }
    }
}