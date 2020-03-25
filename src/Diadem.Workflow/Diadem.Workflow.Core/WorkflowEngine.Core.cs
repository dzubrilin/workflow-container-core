using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Cache;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution;
using Diadem.Workflow.Core.Execution.Events;
using Diadem.Workflow.Core.Execution.Messages;
using Diadem.Workflow.Core.Execution.States;
using Diadem.Workflow.Core.Execution.Transitions;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.Runtime;
using Diadem.Workflow.Core.Scripting;
using Serilog;

namespace Diadem.Workflow.Core
{
    public partial class WorkflowEngine : IWorkflowEngine, IRuntimeWorkflowEngine
    {
        private static readonly Lazy<IWorkflowCache> Cache =
            new Lazy<IWorkflowCache>(() => new InMemoryWorkflowCache(), LazyThreadSafetyMode.ExecutionAndPublication);

        internal static readonly Lazy<IScriptingEngine> ScriptingEngine =
            new Lazy<IScriptingEngine>(() => new InProcessScriptingEngine(), LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly Lazy<IEventProcessor> _eventProcessorLazy;

        private readonly Lazy<IMessageProcessor> _messageProcessorLazy;

        private readonly Lazy<IStateProcessor> _stateProcessorLazy;

        private readonly Lazy<ITransitionProcessor> _transitionProcessorLazy;

        public WorkflowEngine(WorkflowEngineBuilder workflowEngineBuilder)
        {
            Id = Guid.NewGuid();
            WorkflowEngineBuilder = workflowEngineBuilder;
            _messageProcessorLazy = new Lazy<IMessageProcessor>(() => new MessageProcessor(this), LazyThreadSafetyMode.ExecutionAndPublication);
            _eventProcessorLazy = new Lazy<IEventProcessor>(() => new EventProcessor(this), LazyThreadSafetyMode.ExecutionAndPublication);
            _stateProcessorLazy = new Lazy<IStateProcessor>(() => new StateProcessor(this), LazyThreadSafetyMode.ExecutionAndPublication);
            _transitionProcessorLazy = new Lazy<ITransitionProcessor>(() => new TransitionProcessor(this), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        internal WorkflowEngineBuilder WorkflowEngineBuilder { get; }

        public Guid Id { get; }

        internal async Task SaveWorkflowInstance(IWorkflowInstance workflowInstance, CancellationToken cancellationToken)
        {
            ((WorkflowInstanceLock) workflowInstance.Lock).LockedUntil = DateTime.UtcNow.AddSeconds(10D);
            await WorkflowEngineBuilder.WorkflowStore.SaveWorkflowInstance(workflowInstance, cancellationToken).ConfigureAwait(false);
        }

        private IWorkflowInstanceLock CreateWorkflowInstanceLock(WorkflowConfiguration workflowConfiguration)
        {
            var created = DateTime.UtcNow;

            // TODO: move locking configuration to workflow configuration (currently 10 seconds is the default lock duration)
            return new WorkflowInstanceLock(Id, WorkflowInstanceLockMode.Locked, created, created.AddSeconds(10D));
        }

        private async Task<IWorkflowInstance> GetWorkflowInstanceWithLock(Guid workflowInstanceId, CancellationToken cancellationToken)
        {
            IWorkflowInstance workflowInstance = null;
            for (var i = 1; i <= 3; i++)
            {
                // make sure that UTC now is obtained before the call to store (not after)
                var utcNow = DateTime.UtcNow;
                workflowInstance = await WorkflowEngineBuilder.WorkflowStore.GetWorkflowInstance(workflowInstanceId, cancellationToken).ConfigureAwait(false);
                if (null == workflowInstance)
                {
                    return null;
                }

                // obtain lock on root workflow instance
                var success = await WorkflowEngineBuilder.WorkflowStore
                    .TryLockWorkflowInstance(Id, workflowInstance.RootWorkflowInstanceId, utcNow, utcNow.AddSeconds(3D), cancellationToken)
                    .ConfigureAwait(false);
                if (success)
                {
                    await PopulateWorkflowInstanceWithInternalState(workflowInstance, cancellationToken).ConfigureAwait(false);
                    return workflowInstance;
                }

                if (i == 3)
                {
                    throw new WorkflowException($"Cannot obtain lock on workflow instance [{workflowInstance.Id:D}]");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100D), cancellationToken).ConfigureAwait(false);
            }

            return workflowInstance;
        }

        private async Task<IWorkflowInstance> GetWorkflowInstanceWithNoLock(Guid workflowInstanceId, CancellationToken cancellationToken)
        {
            var workflowInstance = await WorkflowEngineBuilder.WorkflowStore.GetWorkflowInstance(workflowInstanceId, cancellationToken).ConfigureAwait(false);
            await PopulateWorkflowInstanceWithInternalState(workflowInstance, cancellationToken).ConfigureAwait(false);
            return workflowInstance;
        }

        private async Task PopulateWorkflowInstanceWithInternalState(IWorkflowInstance workflowInstance, CancellationToken cancellationToken)
        {
            var workflowInstanceInternal = (IWorkflowInstanceInternal) workflowInstance;
            var workflowInstanceStateLogs = await WorkflowEngineBuilder.WorkflowStore.GetWorkflowInstanceStateLog(workflowInstance.Id, cancellationToken).ConfigureAwait(false);
            if (null != workflowInstanceStateLogs && workflowInstanceStateLogs.Count > 0)
            {
                foreach (var workflowInstanceStateLog in workflowInstanceStateLogs.OrderBy(s => s.Started))
                {
                    workflowInstanceInternal.VisitedStatesInternal.Add(workflowInstanceStateLog.StateCode);
                }
            }
        }

        private async Task UnlockWorkflowInstance(IWorkflowInstance workflowInstance, CancellationToken cancellationToken)
        {
            ((WorkflowInstanceLock) workflowInstance.Lock).LockedUntil = workflowInstance.Lock.LockedAt;
            var success = await WorkflowEngineBuilder.WorkflowStore
                .TryUnlockWorkflowInstance(Id, workflowInstance.RootWorkflowInstanceId, workflowInstance.Lock.LockedAt, cancellationToken)
                .ConfigureAwait(false);

            if (!success)
            {
                Log.Warning("Failed to unlock workflow instance [{workflowInstanceId}] by a [{rootWorkflowInstanceId}]",
                    workflowInstance.Id, workflowInstance.RootWorkflowInstanceId);
            }
        }

        private async Task<bool> TryGetDomainEntity(WorkflowContext workflowContext, CancellationToken cancellationToken)
        {
            return await TryGetDomainEntity(workflowContext.WorkflowConfiguration, workflowContext.WorkflowInstance, cancellationToken)
                .ConfigureAwait(false);
        }
        
        private async Task<bool> TryGetDomainEntity(WorkflowConfiguration workflowConfiguration, IWorkflowInstance workflowInstance, CancellationToken cancellationToken)
        {
            // if no domain entity has been provided ==> try loading it through domain store
            //    1. so it will be available for all/any scripted code as an "entity" parameter
            //    2. at the moment we are no saving back entity through the store at the end of processing
            //       2.1. we intentionally obtain domain entity for read-only purpose
            //       2.2. all the mutation to domain entity must happen on a remote activity processing side
            if (null == workflowInstance.Entity
                && workflowConfiguration.HasScriptWithEntityUse()
                && !string.IsNullOrEmpty(workflowInstance.EntityType)
                && !string.IsNullOrEmpty(workflowInstance.EntityId))
            {
                workflowInstance.Entity = await WorkflowEngineBuilder.WorkflowDomainStore
                    .GetDomainEntity(WorkflowEngineBuilder.WorkflowMessageTransportFactoryProvider, workflowConfiguration, workflowInstance, cancellationToken)
                    .ConfigureAwait(false);
                if (null == workflowInstance.Entity)
                {
                    throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                        "Cannot load domain entity [EntityType={0}], [EntityID={1}] for workflow [ID={2:D}], [State={3}] ",
                        workflowInstance.EntityType, workflowInstance.EntityId, workflowInstance.Id, workflowInstance.CurrentStateCode));
                }

                return true;
            }

            return false;
        }

        private async Task<WorkflowConfiguration> GetWorkflowConfiguration(Guid workflowId, CancellationToken cancellationToken = default)
        {
            var key = workflowId.ToString("D", CultureInfo.InvariantCulture);
            if (Cache.Value.TryGetValue(key, out WorkflowConfiguration workflowConfiguration))
            {
                return workflowConfiguration;
            }

            IWorkflowParser workflowParser = new WorkflowXmlParser();
            var content = await WorkflowEngineBuilder.WorkflowStore.GetWorkflowConfiguration(workflowId, cancellationToken).ConfigureAwait(false);
            if (null == content)
            {
                throw new WorkflowException($"Workflow ID [{workflowId:D}] is referring to non existing workflow configuration");
            }

            workflowConfiguration = workflowParser.Parse(content);

            var workflowRuntimeConfiguration = await WorkflowEngineBuilder.WorkflowRuntimeConfigurationFactory
                .GetWorkflowRuntimeConfiguration(workflowConfiguration, cancellationToken)
                .ConfigureAwait(false);
            if (null == workflowRuntimeConfiguration)
            {
                throw new WorkflowException($"Workflow ID [{workflowId:D}] has not matching runtime configuration");
            }

            workflowConfiguration.RuntimeConfiguration = workflowRuntimeConfiguration;

            Cache.Value.Put(key, workflowConfiguration);
            return workflowConfiguration;
        }
    }
}