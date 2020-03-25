using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.Configuration;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution;
using Diadem.Workflow.Core.Execution.Activities;
using Diadem.Workflow.Core.Execution.States;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.Runtime;
using Moq;
using NUnit.Framework;

namespace Diadem.Workflow.Core.UnitTests.Execution.Activities
{
    public class RemoteActivityTests
    {
        private ActivityExecutionContext _activityExecutionContext;

        private IActivityResponseWorkflowMessage _activityResponseWorkflowMessage;

        private Mock<IEndpointConfiguration> _endpointConfiguration;

        private Mock<IRuntimeWorkflowEngine> _runtimeWorkflowEngine;

        private StateExecutionContext _stateExecutionContext;

        private WorkflowConfiguration _workflowConfiguration;

        private WorkflowContext _workflowContext;

        private WorkflowEngineBuilder _workflowEngineBuilder;

        private Mock<IWorkflowInstance> _workflowInstance;

        private Mock<IWorkflowMessageTransport> _workflowMessageTransport;

        private Mock<IWorkflowMessageTransportFactory> _workflowMessageTransportFactory;

        private Mock<IWorkflowMessageTransportFactoryProvider> _workflowMessageTransportFactoryProvider;
        
        [SetUp]
        public void Setup()
        {
            _workflowInstance = new Mock<IWorkflowInstance>();
            _runtimeWorkflowEngine = new Mock<IRuntimeWorkflowEngine>();
            _endpointConfiguration = new Mock<IEndpointConfiguration>();
            _workflowMessageTransport = new Mock<IWorkflowMessageTransport>();
            _workflowMessageTransportFactory = new Mock<IWorkflowMessageTransportFactory>();
            _workflowMessageTransportFactoryProvider = new Mock<IWorkflowMessageTransportFactoryProvider>();
            _workflowEngineBuilder = new WorkflowEngineBuilder().WithMessageTransportFactoryProvider(_workflowMessageTransportFactoryProvider.Object);

            _endpointConfiguration.Setup(f => f.Code).Returns("activity");
            _endpointConfiguration.Setup(f => f.Address).Returns(new Uri("rabbitmq://localhost/Diadem.Workflow.Core.Model:IActivityRequestWorkflowMessage"));

            var workflowId = Guid.NewGuid();
            _workflowInstance.Setup(f => f.Id).Returns(workflowId);
            _workflowConfiguration = new WorkflowConfiguration(workflowId, "unit.test", "unit.test", "Unit Test", new Version(0, 0, 1))
            {
                RuntimeConfiguration = new WorkflowRuntimeConfiguration(workflowId, "unit.test", "unit.test")
                {
                    EndpointConfiguration = _endpointConfiguration.Object,
                    EndpointConfigurations = new[] {new KeyValuePair<string, IEndpointConfiguration>("remote.activity.*", _endpointConfiguration.Object)}
                }
            };

            _activityResponseWorkflowMessage = new ActivityResponseWorkflowMessage {ActivityExecutionResult = new ActivityExecutionResult(ActivityExecutionStatus.Completed)};
            _workflowMessageTransport
                .Setup(f => f.Request<IActivityRequestWorkflowMessage, IActivityResponseWorkflowMessage>(
                    It.IsAny<IEndpointConfiguration>(), It.IsAny<IActivityRequestWorkflowMessage>(), It.IsAny<CancellationToken>()))
                .Returns<IEndpointConfiguration, IActivityRequestWorkflowMessage, CancellationToken>((epc, wm, ct) => Task.FromResult(_activityResponseWorkflowMessage));

            _workflowMessageTransportFactoryProvider
               .Setup(f => f.CreateMessageTransportFactory(It.IsAny<EndpointConfigurationType>()))
               .Returns<EndpointConfigurationType>(uri => _workflowMessageTransportFactory.Object);
            
            _workflowMessageTransportFactory
                .Setup(f => f.CreateMessageTransport(It.IsAny<Uri>()))
                .Returns<Uri>(uri => _workflowMessageTransport.Object);

            _workflowContext = new WorkflowContext(_runtimeWorkflowEngine.Object, _workflowEngineBuilder, _workflowInstance.Object, _workflowConfiguration);
            _stateExecutionContext = new StateExecutionContext(_workflowContext, new StateConfiguration());
            _activityExecutionContext = new ActivityExecutionContext(_stateExecutionContext, new ActivityConfiguration());
        }

        [TearDown]
        public void TearDown()
        {
            _workflowMessageTransport.Verify(m => m.Request<IActivityRequestWorkflowMessage, IActivityResponseWorkflowMessage>(
                It.IsAny<IEndpointConfiguration>(), It.IsAny<IActivityRequestWorkflowMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            _workflowMessageTransportFactory.Verify(m => m.CreateMessageTransport(It.IsAny<Uri>()), Times.Once);
        }

        [Test]
        public async Task RemoteActivity_Execute_Success()
        {
            var remoteActivity = new RemoteActivity("remote.activity.{activity01}");
            var activityExecutionResult = await remoteActivity.Execute(_activityExecutionContext, new CancellationToken());

            Assert.IsTrue(activityExecutionResult.Status == ActivityExecutionStatus.Completed);
        }

        [Test]
        public async Task RemoteActivity_Execute_FailedFromFailInResponse()
        {
            _activityResponseWorkflowMessage = new ActivityResponseWorkflowMessage {ActivityExecutionResult = new ActivityExecutionResult(ActivityExecutionStatus.Failed)};

            var remoteActivity = new RemoteActivity("remote.activity.{activity01}");
            var activityExecutionResult = await remoteActivity.Execute(_activityExecutionContext, new CancellationToken());

            Assert.IsTrue(activityExecutionResult.Status == ActivityExecutionStatus.Failed);
        }

        [Test]
        public async Task RemoteActivity_Execute_ActivityInitial()
        {
            _activityResponseWorkflowMessage = new ActivityResponseWorkflowMessage {ActivityExecutionResult = new ActivityExecutionResult(ActivityExecutionStatus.Failed)};

            var remoteActivity = new RemoteActivity("remote.activity.{activity01}");
            var activityExecutionResult = await remoteActivity.Execute(_activityExecutionContext, new CancellationToken());

            Assert.IsTrue(activityExecutionResult.Status == ActivityExecutionStatus.Failed);
        }
    }
}