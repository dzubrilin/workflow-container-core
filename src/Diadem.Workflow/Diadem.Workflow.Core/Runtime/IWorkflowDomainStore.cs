using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution;
using Diadem.Workflow.Core.Model;

namespace Diadem.Workflow.Core.Runtime
{
    public interface IWorkflowDomainStore
    {
        Task<IEntity> GetDomainEntity(IWorkflowMessageTransportFactoryProvider workflowMessageTransportFactoryProvider, WorkflowConfiguration workflowConfiguration,
                                      IWorkflowInstance workflowInstance, CancellationToken cancellationToken = default);
        
    }
}