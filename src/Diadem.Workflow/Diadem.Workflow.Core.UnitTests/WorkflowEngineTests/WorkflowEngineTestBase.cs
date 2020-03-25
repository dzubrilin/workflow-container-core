using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.Configuration;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution.Activities;
using Diadem.Workflow.Core.Execution.EventHandlers;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.Runtime;
using Diadem.Workflow.Core.UnitTests.Model;
using Diadem.Workflow.Core.UnitTests.WorkflowConfigurations;
using Diadem.Workflow.Provider.MongoDb;
using MongoDB.Driver;
using Moq;

namespace Diadem.Workflow.Core.UnitTests.WorkflowEngineTests
{
    public abstract class WorkflowEngineTestBase
    {
        protected static readonly Guid WorkflowPackage01Id = Guid.Parse("DEA56678-C44F-4076-90A6-D742C55DC832");

        protected static readonly Guid WorkflowPackageCyclic01Id = Guid.Parse("DE55FB86-BAAC-4E1E-AFB2-E9DBA0F2468F");

        protected static readonly Guid WorkflowPackageCyclic02Id = Guid.Parse("C60CF787-90B9-421C-A00F-E338B038ED8C");

        protected static readonly Guid WorkflowPackage02Id = Guid.Parse("31162CDE-B9DA-4A98-9F0D-C8076440196C");

        protected static readonly Guid WorkflowPackage03Id = Guid.Parse("BC58C341-498A-4B6B-8A9B-5694E2E05DA8");

        protected static readonly Guid WorkflowPackage04Id = Guid.Parse("AD62684C-619C-4EE4-861B-78D0137F61A9");

        protected static readonly Guid WorkflowPackage05Id = Guid.Parse("D91FB91A-FAA6-47D9-B859-35DFD9096D70");

        protected static readonly Guid WorkflowPackage06Id = Guid.Parse("F420998A-0B37-41E8-8332-62959D847504");
        
        protected static readonly Guid WorkflowPackage07Id = Guid.Parse("724559E4-A035-469A-A56D-7D7579944F9D");

        protected static readonly Guid WorkflowSigner01Id = Guid.Parse("76D74A27-221F-45B3-9C61-EF0AC0070354");

        protected readonly IList<IWorkflowInstance> WorkflowInstances;

        protected readonly IDictionary<Guid, IWorkflowMessage> WorkflowMessages;

        protected readonly IDictionary<Guid, IWorkflowMessageState> WorkflowMessageStates;

        protected WorkflowEngineTestBase()
        {
            WorkflowInstances = new List<IWorkflowInstance>();
            WorkflowMessages = new Dictionary<Guid, IWorkflowMessage>();
            WorkflowMessageStates = new Dictionary<Guid, IWorkflowMessageState>();
        }

        protected IEntity Package01 { get; set; }

        protected IEntity Signer01 { get; set; }

        protected IEntity Signer02 { get; set; }

        protected IEntity Badge0101 { get; set; }

        protected IEntity Badge0102 { get; set; }

        protected IEntity Badge0103 { get; set; }

        protected IEntity Badge0104 { get; set; }

        protected Mock<IActivityFactory> ActivityFactoryMock { get; private set; }

        protected Mock<IEventHandlerFactory> EventHandlerFactoryMock { get; private set; }

        protected Mock<IWorkflowStore> WorkflowStoreMock { get; private set; }

        protected Mock<IWorkflowRuntimeConfigurationFactory> WorkflowRuntimeConfigurationFactoryMock { get; private set; }

        protected Mock<IWorkflowMessageTransport> WorkflowMessageTransportMock { get; private set; }

        protected Mock<IWorkflowMessageTransportFactory> WorkflowMessageTransportFactoryMock { get; private set; }
        
        protected Mock<IWorkflowMessageTransportFactoryProvider> WorkflowMessageTransportFactoryProviderMock { get; private set; }

        protected string ToJsonString(IEntity entity)
        {
            if (entity is JsonEntity jsonEntity)
            {
                return jsonEntity.ToJsonString();
            }

            throw new ArgumentException($"Entity has to be [{typeof(JsonEntity).Name}] but [{entity.GetType().Name}] has been provided");
        }

        protected void SetWorkflowInstance(IWorkflowInstance workflowInstance, CancellationToken cancellationToken)
        {
            for (var i = WorkflowInstances.Count - 1; i >= 0; i--)
            {
                if (null == workflowInstance.Entity)
                {
                    break;
                }

                if (WorkflowInstances[i].Entity.EntityType == workflowInstance.Entity.EntityType && WorkflowInstances[i].Entity.EntityId == workflowInstance.Entity.EntityId)
                {
                    WorkflowInstances.RemoveAt(i);
                }
            }

            WorkflowInstances.Add(workflowInstance);
        }

        protected IWorkflowMessage GetWorkflowMessage(Guid id)
        {
            return WorkflowMessages[id];
        }

        protected void SetWorkflowMessage(IEndpointConfiguration endpointConfiguration, IWorkflowMessage workflowMessage, CancellationToken cancellationToken)
        {
            if (!WorkflowMessages.ContainsKey(workflowMessage.WorkflowMessageId))
            {
                WorkflowMessages[workflowMessage.WorkflowMessageId] = workflowMessage;
            }
        }

        protected IWorkflowMessageState GetWorkflowMessageState(Guid id)
        {
            return WorkflowMessageStates[id];
        }

        protected void SetWorkflowMessageState(IWorkflowMessage workflowMessage, CancellationToken cancellationToken)
        {
            if (!WorkflowMessageStates.ContainsKey(workflowMessage.WorkflowMessageId))
            {
                WorkflowMessageStates[workflowMessage.WorkflowMessageId] = workflowMessage.State;
            }
        }

        protected IWorkflowInstance GetWorkflowInstance(Guid id)
        {
            return WorkflowInstances.Single(workflowInstance => workflowInstance.Id == id);
        }

        protected IWorkflowInstance GetWorkflowInstance(string entityType, string entityId)
        {
            return WorkflowInstances.Single(workflowInstance => workflowInstance.Entity.EntityType == entityType && workflowInstance.Entity.EntityId == entityId);
        }

        protected IWorkflowInstance GetWorkflowInstance(IEntity entity)
        {
            return WorkflowInstances.Single(workflowInstance => workflowInstance.Entity.EntityType == entity.EntityType && workflowInstance.Entity.EntityId == entity.EntityId);
        }

        protected WorkflowEngineBuildResult BuildWorkflowEngine(bool mockWorkflowStore, bool nested, IActivity activity, ICodeEventHandler eventHandler)
        {
            var activityFactoryMock = BuildActivityFactory(activity);
            var eventHandlerFactory = BuildEventHandlerFactory(eventHandler);
            var domainStore = BuildDomainStore(nested);
            var workflowStore = BuildWorkflowStore(mockWorkflowStore, nested);
            var workflowMessageBus = BuildWorkflowMessageTransportFactoryProvider();
            var workflowRuntimeConfigurationFactory = BuildWorkflowRuntimeConfigurationFactory();

            var workflowEngineBuilder = new WorkflowEngineBuilder();
            workflowEngineBuilder
                .WithActivityFactory(activityFactoryMock)
                .WithEventHandlerFactory(eventHandlerFactory)
                .WithDomainStore(domainStore)
                .WithMessageTransportFactoryProvider(workflowMessageBus)
                .WithWorkflowRuntimeConfigurationFactory(workflowRuntimeConfigurationFactory)
                .WithWorkflowStore(workflowStore);
            return new WorkflowEngineBuildResult(workflowEngineBuilder, new WorkflowEngine(workflowEngineBuilder));
        }

        private IActivityFactory BuildActivityFactory(IActivity activity)
        {
            ActivityFactoryMock = new Mock<IActivityFactory>();
            ActivityFactoryMock
                .Setup(f => f.CreateActivity(It.Is<string>(code => string.Equals("noOp", code, StringComparison.OrdinalIgnoreCase))))
                .Returns(activity);
            ActivityFactoryMock
                .Setup(f => f.CreateActivity(It.Is<string>(code => code.StartsWith("remote."))))
                .Returns(activity);
            return ActivityFactoryMock.Object;
        }

        private IEventHandlerFactory BuildEventHandlerFactory(ICodeEventHandler eventHandler)
        {
            EventHandlerFactoryMock = new Mock<IEventHandlerFactory>();
            EventHandlerFactoryMock
                .Setup(f => f.CreateEventHandler(It.Is<string>(code => string.Equals("noOp", code, StringComparison.OrdinalIgnoreCase))))
                .Returns(eventHandler);
            EventHandlerFactoryMock
                .Setup(f => f.CreateEventHandler(It.Is<string>(code => code.StartsWith("remote."))))
                .Returns(eventHandler);
            return EventHandlerFactoryMock.Object;
        }

        private IWorkflowDomainStore BuildDomainStore(bool nested)
        {
            return nested ? BuildDomainStoreNested().Object : BuildDomainStoreCore().Object;
        }

        private Mock<IWorkflowDomainStore> BuildDomainStoreNested()
        {
            var domainStore = BuildDomainStoreCore();
            
            SetupDomainStoreReturn<Signer>(domainStore, Signer01);
            SetupDomainStoreReturn<Signer>(domainStore, Signer02);

            SetupDomainStoreReturn<Badge>(domainStore, Badge0101);
            SetupDomainStoreReturn<Badge>(domainStore, Badge0102);
            SetupDomainStoreReturn<Badge>(domainStore, Badge0103);
            SetupDomainStoreReturn<Badge>(domainStore, Badge0104);

            return domainStore;
        }

        private Mock<IWorkflowDomainStore> BuildDomainStoreCore()
        {
            var domainStore = new Mock<IWorkflowDomainStore>();
            SetupDomainStoreReturn<Package>(domainStore, Package01);
            
            return domainStore;
        }

        private void SetupDomainStoreReturn<TEntity>(Mock<IWorkflowDomainStore> domainStoreMock, IEntity entity) where TEntity : IEntity
        {
            domainStoreMock
               .Setup(f => f.GetDomainEntity(
                    It.IsAny<IWorkflowMessageTransportFactoryProvider>(),
                    It.IsAny<WorkflowConfiguration>(),
                    It.Is<WorkflowInstance>(wi => 
                        string.Equals(wi.EntityType, typeof(TEntity).FullName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(wi.EntityId, entity.EntityId, StringComparison.OrdinalIgnoreCase)),
                    It.IsAny<CancellationToken>()))
               .Returns(Task.FromResult(entity));
        }

        private IWorkflowStore BuildWorkflowStore(bool mockWorkflowStore, bool nested)
        {
            if (mockWorkflowStore)
            {
                return nested ? BuildWorkflowStoreNested().Object : BuildWorkflowStoreFlat().Object;
            }

            var password = new SecureString();
            password.AppendChar('f');
            password.AppendChar('l');
            password.AppendChar('o');
            password.AppendChar('w');

            var mongoClient = new MongoClient(new MongoClientSettings
            {
                Server = new MongoServerAddress("localhost", 27017),
                Credential = MongoCredential.CreateCredential("admin", "flow", password),
                ConnectionMode = ConnectionMode.Automatic
            });

            var mongoDatabase = mongoClient.GetDatabase("workflow");
            var mongoStore = new MongoDbWorkflowStore(mongoDatabase);
            InitializeWorkflowStore(mongoStore);
            return mongoStore;
        }

        protected virtual void InitializeWorkflowStore(MongoDbWorkflowStore mongoDbWorkflowStore)
        {
            mongoDbWorkflowStore.Clear();

            mongoDbWorkflowStore.SaveWorkflowConfigurationContent(WorkflowPackage01Id, "Simple V0",
                WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackage01)).Wait();

            mongoDbWorkflowStore.SaveWorkflowConfigurationContent(WorkflowPackageCyclic01Id, "Simple Cyclic V0.1",
                WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackageCyclic01)).Wait();

            mongoDbWorkflowStore.SaveWorkflowConfigurationContent(WorkflowPackageCyclic02Id, "Simple Cyclic V0.2",
                WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackageCyclic02)).Wait();

            mongoDbWorkflowStore.SaveWorkflowConfigurationContent(WorkflowPackage02Id, "Package v.0.2",
                WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackage02)).Wait();

            mongoDbWorkflowStore.SaveWorkflowConfigurationContent(WorkflowPackage03Id, "Simple V0",
                WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackage03)).Wait();

            mongoDbWorkflowStore.SaveWorkflowConfigurationContent(WorkflowPackage04Id, "Simple V0",
                WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackage04)).Wait();

            mongoDbWorkflowStore.SaveWorkflowConfigurationContent(WorkflowPackage05Id, "Remote V0",
                WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackage05)).Wait();

            mongoDbWorkflowStore.SaveWorkflowConfigurationContent(WorkflowPackage06Id, "Simple V0.6 with delayed transition",
                WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackage06)).Wait();

            mongoDbWorkflowStore.SaveWorkflowConfigurationContent(WorkflowPackage07Id, "Simple V0.7 with custom activity retry and transition",
                WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackage07)).Wait();
            
            mongoDbWorkflowStore.SaveWorkflowConfigurationContent(WorkflowSigner01Id, "Signer v.0.1",
                WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowSigner01)).Wait();
        }

        private Mock<IWorkflowStore> BuildWorkflowStoreFlat()
        {
            WorkflowStoreMock = BuildWorkflowStoreCore();

            WorkflowStoreMock
                .Setup(f => f.GetNestedWorkflowInstances(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Enumerable.Empty<IWorkflowInstance>()));
            return WorkflowStoreMock;
        }

        private Mock<IWorkflowStore> BuildWorkflowStoreNested()
        {
            var workflowStore = BuildWorkflowStoreCore();

            workflowStore
                .Setup(f => f.GetWorkflowConfiguration(It.Is<Guid>(id => id == WorkflowPackage02Id), It.IsAny<CancellationToken>()))
                .Returns<Guid, CancellationToken>((id, ct) => Task.FromResult(WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackage02)));
            workflowStore
                .Setup(f => f.GetWorkflowConfiguration(It.Is<Guid>(id => id == WorkflowSigner01Id), It.IsAny<CancellationToken>()))
                .Returns<Guid, CancellationToken>((id, ct) => Task.FromResult(WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowSigner01)));

            workflowStore
                .Setup(f => f.GetNestedWorkflowInstances(It.Is<Guid>(id => id == GetWorkflowInstance(Package01).Id), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult<IEnumerable<IWorkflowInstance>>(new[] {GetWorkflowInstance(Signer01), GetWorkflowInstance(Signer02)}));

            return workflowStore;
        }

        protected virtual Mock<IWorkflowStore> BuildWorkflowStoreCore()
        {
            var workflowStore = new Mock<IWorkflowStore>();
            workflowStore
                .Setup(f => f.GetWorkflowConfiguration(It.Is<Guid>(id => id == WorkflowPackage01Id), It.IsAny<CancellationToken>()))
                .Returns<Guid, CancellationToken>((id, ct) => Task.FromResult(WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackage01)));
            workflowStore
                .Setup(f => f.GetWorkflowConfiguration(It.Is<Guid>(id => id == WorkflowPackage04Id), It.IsAny<CancellationToken>()))
                .Returns<Guid, CancellationToken>((id, ct) => Task.FromResult(WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackage04)));
            workflowStore
                .Setup(f => f.GetWorkflowConfiguration(It.Is<Guid>(id => id == WorkflowPackage07Id), It.IsAny<CancellationToken>()))
                .Returns<Guid, CancellationToken>((id, ct) => Task.FromResult(WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackage07)));
            workflowStore
                .Setup(f => f.GetWorkflowConfiguration(It.Is<Guid>(id => id == WorkflowPackageCyclic01Id), It.IsAny<CancellationToken>()))
                .Returns<Guid, CancellationToken>((id, ct) => Task.FromResult(WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackageCyclic01)));
            workflowStore
                .Setup(f => f.GetWorkflowConfiguration(It.Is<Guid>(id => id == WorkflowPackageCyclic02Id), It.IsAny<CancellationToken>()))
                .Returns<Guid, CancellationToken>((id, ct) => Task.FromResult(WorkflowConfigurationsHelper.GetWorkflowConfiguration(WorkflowConfigurationsHelper.FlowPackageCyclic02)));
            workflowStore
                .Setup(f => f.GetWorkflowInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, string, CancellationToken>((entityType, entityId, ct) => Task.FromResult(GetWorkflowInstance(entityType, entityId)));
            workflowStore
                .Setup(f => f.GetWorkflowInstance(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns<Guid, CancellationToken>((id, ct) => Task.FromResult(GetWorkflowInstance(id)));
            workflowStore
                .Setup(f => f.GetWorkflowMessageState(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns<Guid, CancellationToken>((id, ct) => Task.FromResult(GetWorkflowMessageState(id)));
            workflowStore
                .Setup(f => f.SaveWorkflowInstance(It.IsAny<IWorkflowInstance>(), It.IsAny<CancellationToken>()))
                .Callback<IWorkflowInstance, CancellationToken>(SetWorkflowInstance)
                .Returns(Task.CompletedTask);
            workflowStore
                .Setup(f => f.SaveWorkflowMessageState(It.IsAny<IWorkflowMessage>(), It.IsAny<CancellationToken>()))
                .Callback<IWorkflowMessage, CancellationToken>(SetWorkflowMessageState)
                .Returns(Task.CompletedTask);
            workflowStore
                .Setup(f => f.TryLockWorkflowInstance(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns<Guid, Guid, DateTime, DateTime, CancellationToken>(
                    (ownerId, workflowInstanceId, lockedAt, lockedUntil, ct) =>
                    {
                        var workflowInstance = GetWorkflowInstance(workflowInstanceId);
                        return Task.FromResult(workflowInstance.Lock.LockOwner == ownerId);
                    });

            return workflowStore;
        }

        private IWorkflowRuntimeConfigurationFactory BuildWorkflowRuntimeConfigurationFactory()
        {
            WorkflowRuntimeConfigurationFactoryMock = new Mock<IWorkflowRuntimeConfigurationFactory>();
            WorkflowRuntimeConfigurationFactoryMock
                .Setup(f => f.GetWorkflowRuntimeConfiguration(It.IsAny<WorkflowConfiguration>(), It.IsAny<CancellationToken>()))
                .Returns<WorkflowConfiguration, CancellationToken>((wc, ct) => Task.FromResult(new WorkflowRuntimeConfiguration(wc.Id, wc.Class, wc.Code)
                {
                    EndpointConfiguration = new EndpointConfiguration("InMemory", EndpointConfigurationType.RabbitMq, new Uri("rabbitmq://localhost/flow"), ConfigurationAuthentication.None)
                }));

            return WorkflowRuntimeConfigurationFactoryMock.Object;
        }

        private IWorkflowMessageTransportFactoryProvider BuildWorkflowMessageTransportFactoryProvider()
        {
            WorkflowMessageTransportMock = new Mock<IWorkflowMessageTransport>();
            WorkflowMessageTransportFactoryMock = new Mock<IWorkflowMessageTransportFactory>();
            WorkflowMessageTransportFactoryProviderMock = new Mock<IWorkflowMessageTransportFactoryProvider>();

            WorkflowMessageTransportMock
                .Setup(f => f.Send(It.IsAny<IEndpointConfiguration>(), It.IsAny<IWorkflowMessage>(), It.IsAny<CancellationToken>()))
                .Callback<IEndpointConfiguration, IWorkflowMessage, CancellationToken>(SetWorkflowMessage)
                .Returns(Task.CompletedTask);

            WorkflowMessageTransportFactoryMock
                .Setup(f => f.CreateMessageTransport(It.IsAny<Uri>()))
                .Returns<Uri>(address => WorkflowMessageTransportMock.Object);

            WorkflowMessageTransportFactoryProviderMock
               .Setup(f => f.CreateMessageTransportFactory(It.IsAny<EndpointConfigurationType>()))
               .Returns<EndpointConfigurationType>(uri => WorkflowMessageTransportFactoryMock.Object);
            
            return WorkflowMessageTransportFactoryProviderMock.Object;
        }

        protected class WorkflowEngineBuildResult
        {
            public WorkflowEngineBuildResult(WorkflowEngineBuilder workflowEngineBuilder, IWorkflowEngine workflowEngine)
            {
                WorkflowEngineBuilder = workflowEngineBuilder;
                WorkflowEngine = workflowEngine;
            }

            public WorkflowEngineBuilder WorkflowEngineBuilder { get; }

            public IWorkflowEngine WorkflowEngine { get; }

            public IWorkflowInstance GetWorkflowInstance(IEntity entity)
            {
                return WorkflowEngineBuilder.WorkflowStore.GetWorkflowInstance(entity.EntityType, entity.EntityId).Result;
            }
        }
    }
}