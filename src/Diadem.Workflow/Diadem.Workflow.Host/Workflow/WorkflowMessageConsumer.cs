using System;
using System.Threading.Tasks;
using Autofac;
using Diadem.Workflow.Core.Execution.Events;
using Diadem.Workflow.Core.Model;
using MassTransit;
using Serilog;

namespace Diadem.Workflow.Host.Workflow
{
    public class WorkflowMessageConsumer : IConsumer<IAsynchronousTransitionWorkflowMessage>, IConsumer<IDelayedTransitionWorkflowMessage>, IConsumer<IEventRequestWorkflowMessage>
    {
        private readonly ILifetimeScope _lifetimeScope;

        public WorkflowMessageConsumer(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public async Task Consume(ConsumeContext<IAsynchronousTransitionWorkflowMessage> context)
        {
            Log.Debug("Received asynchronous transition workflow {messageId} for {workflowInstanceId} [{state}]",
                context.Message.WorkflowMessageId, context.Message.WorkflowInstanceId, context.Message.State.JsonState);

            try
            {
                using (var scope = _lifetimeScope.BeginLifetimeScope())
                {
                    var workflowEngineFactory = scope.Resolve<IWorkflowEngineFactory>();
                    var workflowEngine = workflowEngineFactory.CreateWorkflowEngine();

                    await workflowEngine.ProcessMessage(context.Message, context.CancellationToken);
                }
                
                await context.ReceiveContext.NotifyConsumed(context, TimeSpan.Zero, typeof(WorkflowMessageConsumer).Name);
                Log.Debug("Finished processing asynchronous transition workflow {messageId} for {workflowInstanceId} [{state}]",
                    context.Message.WorkflowMessageId, context.Message.WorkflowInstanceId, context.Message.State.JsonState);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred during asynchronous transition workflow execution {messageId} {workflowInstanceId} [{state}]",
                    context.Message.WorkflowMessageId, context.Message.WorkflowInstanceId, context.Message.State.JsonState);
            }
        }

        public async Task Consume(ConsumeContext<IEventRequestWorkflowMessage> context)
        {
            Log.Debug("Received event workflow {messageId} for {workflowInstanceId} of {code}",
                context.Message.WorkflowMessageId, context.Message.WorkflowInstanceId, context.Message.EventCode);

            EventResponseWorkflowMessage eventResponseWorkflowMessage;
            try
            {
                using (var scope = _lifetimeScope.BeginLifetimeScope())
                {
                    var workflowEngineFactory = scope.Resolve<IWorkflowEngineFactory>();
                    var workflowEngine = workflowEngineFactory.CreateWorkflowEngine();

                    var workflowProcessingResult = await workflowEngine.ProcessMessage(context.Message, context.CancellationToken);
                    eventResponseWorkflowMessage = new EventResponseWorkflowMessage
                    {
                        WorkflowId = workflowProcessingResult.WorkflowInstance.WorkflowId,
                        WorkflowInstanceId = workflowProcessingResult.WorkflowInstance.Id,
                        WorkflowMessageId = context.Message.WorkflowMessageId,
                        EventExecutionResult = new EventExecutionResult(EventExecutionStatus.Completed),
                        State = new WorkflowMessageState(workflowProcessingResult.WorkflowExecutionState)
                    };

                    Log.Debug("Finished processing event workflow {messageId} for {workflowInstanceId} of {code}... response has been sent",
                        context.Message.WorkflowMessageId, context.Message.WorkflowInstanceId, context.Message.EventCode);
                }
            }
            catch (Exception ex)
            {
                eventResponseWorkflowMessage = new EventResponseWorkflowMessage
                {
                    EventExecutionResult = new EventExecutionResult(EventExecutionStatus.Failed),
                    WorkflowId = context.Message.WorkflowId,
                    WorkflowInstanceId = context.Message.WorkflowInstanceId,
                    WorkflowMessageId = context.Message.WorkflowMessageId,
                    State = context.Message.State
                };
                
                Log.Error(ex, "An error has occurred during processing event workflow {messageId} for {workflowInstanceId} of {code} [{state}]",
                    context.Message.WorkflowMessageId, context.Message.WorkflowInstanceId, context.Message.EventCode, context.Message.State?.JsonState);
            }
            
            await context.RespondAsync<IEventResponseWorkflowMessage>(eventResponseWorkflowMessage);
        }

        public async Task Consume(ConsumeContext<IDelayedTransitionWorkflowMessage> context)
        {
            Log.Debug("Received delayed asynchronous transition workflow {messageId} for {workflowInstanceId} {fromState} {toState}",
                context.Message.WorkflowMessageId, context.Message.WorkflowInstanceId, context.Message.MoveFromState, context.Message.MoveToState);
            
            try
            {
                using (var scope = _lifetimeScope.BeginLifetimeScope())
                {
                    var workflowEngineFactory = scope.Resolve<IWorkflowEngineFactory>();
                    var workflowEngine = workflowEngineFactory.CreateWorkflowEngine();

                    await workflowEngine.ProcessMessage(context.Message, context.CancellationToken);
                }
                
                await context.ReceiveContext.NotifyConsumed(context, TimeSpan.Zero, typeof(WorkflowMessageConsumer).Name);
                Log.Debug("Finished processing delayed asynchronous transition workflow {messageId} for {workflowInstanceId} {fromState} {toState}",
                    context.Message.WorkflowMessageId, context.Message.WorkflowInstanceId, context.Message.MoveFromState, context.Message.MoveToState);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred during asynchronous transition {messageId} {workflowInstanceId} {fromState} {toState}",
                    context.Message.WorkflowMessageId, context.Message.WorkflowInstanceId, context.Message.MoveFromState, context.Message.MoveToState);
            }
        }
    }
}