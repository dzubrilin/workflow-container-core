using System;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core;
using Diadem.Workflow.Core.Model;
using Serilog;
using Serilog.Events;

namespace Diadem.Workflow.Core.Execution.Messages
{
    internal class MessageProcessor : IMessageProcessor
    {
        private readonly WorkflowEngine _workflowEngine;

        public MessageProcessor(WorkflowEngine workflowEngine)
        {
            _workflowEngine = workflowEngine;
        }

        public Task<MessageExecutionResult> ProcessMessage(MessageExecutionContext messageExecutionContext, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNotNull(messageExecutionContext, nameof(messageExecutionContext));
            Guard.ArgumentNotNull(messageExecutionContext.WorkflowContext, nameof(messageExecutionContext.WorkflowContext));
            Guard.ArgumentNotNull(messageExecutionContext.WorkflowMessage, nameof(messageExecutionContext.WorkflowMessage));

            if (Log.IsEnabled(LogEventLevel.Verbose))
            {
                Log.Verbose("Starting processing [{message}|{messageId}] for {workflowInstanceId}, message state is [{state}]",
                    messageExecutionContext.WorkflowMessage.GetType().FullName,
                    messageExecutionContext.WorkflowMessage.WorkflowMessageId,
                    messageExecutionContext.WorkflowContext.WorkflowInstance.Id,
                    messageExecutionContext.WorkflowMessage.State.JsonState);
            }

            if (messageExecutionContext.WorkflowMessage is AsynchronousTransitionWorkflowMessage)
            {
                return Task.FromResult(new MessageExecutionResult(MessageExecutionStatus.ContinueAfterAsyncTransition));
            }

            throw new NotImplementedException($"Non [{nameof(AsynchronousTransitionWorkflowMessage)}] are not supported");
        }
    }
}