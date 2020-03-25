using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Model;
using Serilog;

namespace Diadem.Workflow.Core.Runtime
{
    public class RemoteWorkflowDomainStore : IWorkflowDomainStore
    {
        public async Task<IEntity> GetDomainEntity(IWorkflowMessageTransportFactoryProvider workflowMessageTransportFactoryProvider,
                                                   WorkflowConfiguration workflowConfiguration, IWorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            var endpointCode = $"EntityType::{workflowInstance.EntityType}";
            var endpointConfiguration = workflowConfiguration.FindEndpointConfiguration(endpointCode);
            var eventRequestWorkflowMessage = new EntityRequestWorkflowMessage(workflowInstance.EntityType, workflowInstance.EntityId);

            Log.Verbose("Sending entity request message [{endpointCode}::{entityId}] to {endpoint} [{workflowInstanceId}]",
                endpointCode, workflowInstance.EntityId, endpointConfiguration.Address, workflowInstance.Id);

            var messageTransport = workflowMessageTransportFactoryProvider.CreateMessageTransportFactory(endpointConfiguration.Type).CreateMessageTransport(endpointConfiguration.Address);
            var responseWorkflowMessage = await messageTransport.Request<IEntityRequestWorkflowMessage, IEntityResponseWorkflowMessage>(
                endpointConfiguration, eventRequestWorkflowMessage, cancellationToken).ConfigureAwait(false);

            Log.Verbose("Received entity response message [{endpointCode}::{entityId}::{status}] from {endpoint} [{workflowInstanceId}]",
                endpointCode, workflowInstance.EntityId, responseWorkflowMessage.ExecutionStatus, endpointConfiguration.Address, workflowInstance.Id);

            if (responseWorkflowMessage.ExecutionStatus == EntityRequestExecutionStatus.Completed)
            {
                return new JsonEntity(responseWorkflowMessage.EntityJsonPayload);
            }

            if (responseWorkflowMessage.ExecutionStatus == EntityRequestExecutionStatus.NotFound)
            {
                Log.Error("Can not find {entityType} {entityId}... stopping {workflowInstanceId} processing",
                    workflowInstance.EntityType, workflowInstance.EntityId, workflowInstance.Id);
                
                throw new WorkflowException($"No entity has been found {workflowInstance.EntityType}::{workflowInstance.EntityId}");
            }
            
            throw new WorkflowException($"An error has occurred during obtaining {workflowInstance.EntityType}::{workflowInstance.EntityId}");
        }
    }
}