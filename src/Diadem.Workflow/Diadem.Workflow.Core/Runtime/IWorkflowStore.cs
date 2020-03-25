using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Model;

namespace Diadem.Workflow.Core.Runtime
{
    public interface IWorkflowStore
    {
        /// <summary>
        ///     Archive workflow instance
        /// </summary>
        /// <param name="workflowInstance"></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task ArchiveWorkflowInstance(IWorkflowInstance workflowInstance, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets workflow class configuration
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<string> GetWorkflowConfiguration(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets workflow instance
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="entityId"></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<IWorkflowInstance> GetWorkflowInstance(string entityType, string entityId, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets workflow instance
        /// </summary>
        /// <param name="id">Workflow instance ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<IWorkflowInstance> GetWorkflowInstance(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets workflow instance state execution log
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<IList<IWorkflowInstanceStateLog>> GetWorkflowInstanceStateLog(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets workflow message
        /// </summary>
        /// <param name="id">Workflow message ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<IWorkflowMessageState> GetWorkflowMessageState(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns all the nested workflow instances
        /// </summary>
        /// <param name="parentId">Parent workflow instance ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<IEnumerable<IWorkflowInstance>> GetNestedWorkflowInstances(Guid parentId, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns all the nested workflow instances of a particular workflow class
        /// </summary>
        /// <param name="parentId">Parent workflow instance ID</param>
        /// <param name="workflowId">Nested workflow class ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<IEnumerable<IWorkflowInstance>> GetNestedWorkflowInstances(Guid parentId, Guid workflowId, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Saves workflow configuration
        /// </summary>
        /// <param name="id"></param>
        /// <param name="classCode"></param>
        /// <param name="content"></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task SaveWorkflowConfigurationContent(Guid id, string classCode, string content, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Saves workflow instance
        /// </summary>
        /// <param name="workflowInstance">Workflow instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task SaveWorkflowInstance(IWorkflowInstance workflowInstance, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Saves workflow message state
        /// </summary>
        /// <param name="workflowMessage">Workflow message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task SaveWorkflowMessageState(IWorkflowMessage workflowMessage, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Saves workflow instance activity log
        /// </summary>
        /// <param name="workflowInstanceActivityLog">Workflow instance activity log message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task SaveWorkflowInstanceActivityLog(IWorkflowInstanceActivityLog workflowInstanceActivityLog, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Saves workflow instance event log
        /// </summary>
        /// <param name="workflowInstanceEventLog">Workflow instance event log message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task SaveWorkflowInstanceEventLog(IWorkflowInstanceEventLog workflowInstanceEventLog, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Saves workflow instance message log
        /// </summary>
        /// <param name="workflowInstanceMessageLog">Workflow instance message log message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task SaveWorkflowInstanceMessageLog(IWorkflowInstanceMessageLog workflowInstanceMessageLog, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Saves workflow instance state log
        /// </summary>
        /// <param name="workflowInstanceStateLog">Workflow instance state log message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task SaveWorkflowInstanceStateLog(IWorkflowInstanceStateLog workflowInstanceStateLog, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Tries to obtain a lock on a workflow instance (must be used against Root Workflow Instance ID)
        /// </summary>
        /// <param name="ownerId">Workflow Runtime ID</param>
        /// <param name="workflowInstanceId">Workflow instance ID</param>
        /// <param name="lockedAt">Lock start timestamp</param>
        /// <param name="lockedUntil">Lock finish timestamp</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<bool> TryLockWorkflowInstance(Guid ownerId, Guid workflowInstanceId, DateTime lockedAt, DateTime lockedUntil, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Tries to obtain a lock on a workflow instance (must be used against Root Workflow Instance ID)
        /// </summary>
        /// <param name="ownerId">Workflow Runtime ID</param>
        /// <param name="workflowInstanceId">Workflow instance ID</param>
        /// <param name="lockedAt">Lock start timestamp</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<bool> TryUnlockWorkflowInstance(Guid ownerId, Guid workflowInstanceId, DateTime lockedAt, CancellationToken cancellationToken = default);
    }
}