using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.Configuration;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution;
using Diadem.Workflow.Core.Execution.EventHandlers;
using Diadem.Workflow.Core.Execution.Events;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.Runtime;
using Moq;
using NUnit.Framework;

namespace Diadem.Workflow.Core.UnitTests.Execution.EventHandlers
{
    public class RemoteEventHandlerTests
    {
        private Mock<IEndpointConfiguration> _endpointConfiguration;

        private IEvent _event;

        private EventExecutionContext _eventExecutionContext;

        private IEventResponseWorkflowMessage _eventResponseWorkflowMessage;

        private Mock<IRuntimeWorkflowEngine> _runtimeWorkflowEngine;

        private WorkflowConfiguration _workflowConfiguration;

        private WorkflowContext _workflowContext;

        private WorkflowEngineBuilder _workflowEngineBuilder;

        private Mock<IWorkflowInstance> _workflowInstance;

        private Mock<IWorkflowMessageTransport> _workflowMessageTransport;

        private Mock<IWorkflowMessageTransportFactory> _workflowMessageTransportFactory;

        private Mock<IWorkflowMessageTransportFactoryProvider> _workflowMessageTransportFactoryProvider;
        
        [SetUp]
        public void SetUp()
        {
            _workflowInstance = new Mock<IWorkflowInstance>();
            _runtimeWorkflowEngine = new Mock<IRuntimeWorkflowEngine>();
            _endpointConfiguration = new Mock<IEndpointConfiguration>();
            _workflowMessageTransport = new Mock<IWorkflowMessageTransport>();
            _workflowMessageTransportFactory = new Mock<IWorkflowMessageTransportFactory>();
            _workflowMessageTransportFactoryProvider = new Mock<IWorkflowMessageTransportFactoryProvider>();
            _workflowEngineBuilder = new WorkflowEngineBuilder().WithMessageTransportFactoryProvider(_workflowMessageTransportFactoryProvider.Object);

            _endpointConfiguration.Setup(f => f.Code).Returns("rabbit.mq");
            _endpointConfiguration.Setup(f => f.Address).Returns(new Uri("rabbitmq://localhost/address"));

            var workflowId = Guid.NewGuid();
            _workflowInstance.Setup(f => f.Id).Returns(workflowId);
            _workflowConfiguration = new WorkflowConfiguration(workflowId, "unit.test", "unit.test", "Unit Test", new Version(0, 0, 1))
            {
                RuntimeConfiguration = new WorkflowRuntimeConfiguration(workflowId, "unit.test", "unit.test")
                {
                    EndpointConfiguration = _endpointConfiguration.Object,
                    EndpointConfigurations = new[] {new KeyValuePair<string, IEndpointConfiguration>("remote.*", _endpointConfiguration.Object)}
                }
            };

            _eventResponseWorkflowMessage = new EventResponseWorkflowMessage {EventExecutionResult = new EventExecutionResult(EventExecutionStatus.Completed)};
            _workflowMessageTransport
                .Setup(f => f.Request<IEventRequestWorkflowMessage, IEventResponseWorkflowMessage>(
                    It.IsAny<IEndpointConfiguration>(), It.IsAny<IEventRequestWorkflowMessage>(), It.IsAny<CancellationToken>()))
                .Returns<IEndpointConfiguration, IWorkflowMessage, CancellationToken>((epc, wm, ct) => Task.FromResult(_eventResponseWorkflowMessage));

            _workflowMessageTransportFactoryProvider
               .Setup(f => f.CreateMessageTransportFactory(It.IsAny<EndpointConfigurationType>()))
               .Returns<EndpointConfigurationType>(uri => _workflowMessageTransportFactory.Object);
            
            _workflowMessageTransportFactory
                .Setup(f => f.CreateMessageTransport(It.IsAny<Uri>()))
                .Returns<Uri>(uri => _workflowMessageTransport.Object);

            _event = new StatefulEvent("remote.{sign-event01}", workflowId, new JsonState());
            _workflowContext = new WorkflowContext(_runtimeWorkflowEngine.Object, _workflowEngineBuilder, _workflowInstance.Object, _workflowConfiguration);
            _eventExecutionContext = new EventExecutionContext(_workflowContext, _event,
                new EventConfiguration(EventTypeConfiguration.Application, _event.Code, string.Empty),
                new StateEventConfiguration());
        }

        [TearDown]
        public void TearDown()
        {
            _workflowMessageTransport.Verify(m => m.Request<IEventRequestWorkflowMessage, IEventResponseWorkflowMessage>(
                It.IsAny<IEndpointConfiguration>(), It.IsAny<IEventRequestWorkflowMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            _workflowMessageTransportFactory.Verify(m => m.CreateMessageTransport(It.IsAny<Uri>()), Times.Once);
        }

        [Test]
        public async Task RemoteActivity_Execute_Success()
        {
            var remoteEventHandler = new RemoteEventHandler(_event.Code);
            var eventExecutionResult = await remoteEventHandler.Execute(_eventExecutionContext);

            Assert.IsTrue(eventExecutionResult.Status == EventExecutionStatus.Completed);
        }

        [Test]
        public async Task RemoteActivity_Execute_FailedFromFailInResponse()
        {
            _eventResponseWorkflowMessage = new EventResponseWorkflowMessage {EventExecutionResult = new EventExecutionResult(EventExecutionStatus.Failed)};

            var remoteEventHandler = new RemoteEventHandler(_event.Code);
            var eventExecutionResult = await remoteEventHandler.Execute(_eventExecutionContext);

            Assert.IsTrue(eventExecutionResult.Status == EventExecutionStatus.Failed);
        }
    }
}