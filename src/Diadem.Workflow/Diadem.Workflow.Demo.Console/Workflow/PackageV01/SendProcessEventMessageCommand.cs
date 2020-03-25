using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Demo.Console.Execution;

namespace Diadem.Workflow.Demo.Console.Workflow.PackageV01
{
    public class SendProcessEventMessageCommand : ICommand
    {
        public string Name => "SendProcessEventMessage";

        public async Task Execute(ICommandContext commandContext, CancellationToken cancellationToken)
        {
            var workflowCommandContext = (PackageV01WorkflowCommandContext) commandContext;
            var packageV01WorkflowRunner = (PackageV01WorkflowRunner) commandContext.RunnerContext.Runner;

            var jsonState = new JsonState();
            jsonState.SetProperty("IsSent", false);

            IEventRequestWorkflowMessage initialEventWorkflowMessage = new EventRequestWorkflowMessage(
                packageV01WorkflowRunner.WorkflowId, workflowCommandContext.WorkflowInstanceId, "process", jsonState.ToJsonString());
            var workflowMessage = await workflowCommandContext.WorkflowMessageTransport.Request<IEventRequestWorkflowMessage, IEventResponseWorkflowMessage>(
                packageV01WorkflowRunner.EndpointConfiguration, initialEventWorkflowMessage, cancellationToken).ConfigureAwait(false);

            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "Command [{0}] has been executed [{1}], state [{2}], received workflow ID=[{3:D}]",
                Name, workflowMessage.EventExecutionResult.Status, workflowMessage.State.JsonState, workflowMessage.WorkflowInstanceId.GetValueOrDefault()));
            workflowCommandContext.WorkflowInstanceId = workflowMessage.WorkflowInstanceId.GetValueOrDefault();
        }
    }
}