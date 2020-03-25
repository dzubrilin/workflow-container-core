using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Execution.Events;
using Diadem.Workflow.Core.Execution.States;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.UnitTests.Model;
using Diadem.Workflow.Core.UnitTests.Model.Activities;
using Diadem.Workflow.Core.UnitTests.Model.EventHandlers;
using Diadem.Workflow.Core.UnitTests.Model.Events;
using Moq;
using NUnit.Framework;

namespace Diadem.Workflow.Core.UnitTests.WorkflowEngineTests
{
    public class WorkflowEngineTestSimple01 : WorkflowEngineTestBase
    {
        [SetUp]
        public void Setup()
        {
            Signer01 = new Signer("1");
            Signer02 = new Signer("2");
            Package01 = new Package("1", new List<Signer>(new[] {(Signer)Signer01, (Signer)Signer02}));
        }

        [Test]
        public async Task Simple01_HappyPath_MovedToFinalOnEventWorkflowMessages()
        {
            var workflowEngineBuildResult = BuildWorkflowEngine(true, false, new NoOpActivity(), new NoOpEventHandler());

            var packageJsonState = new JsonState();
            packageJsonState.SetProperty("IsSent", false);

            IEventRequestWorkflowMessage initialEventWorkflowMessage = new EventRequestWorkflowMessage(
                WorkflowPackage01Id, Package01.GetType().FullName, Package01.EntityId, "initial", packageJsonState.ToJsonString());
            var workflowProcessingResult = await workflowEngineBuildResult.WorkflowEngine.ProcessMessage(initialEventWorkflowMessage);

            var package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "process", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);
            Assert.IsTrue(workflowProcessingResult.WorkflowInstance.Entity.GetProperty<bool>("IsSent"));

            IEventRequestWorkflowMessage processEventWorkflowMessage = new EventRequestWorkflowMessage(
                WorkflowPackage01Id, workflowProcessingResult.WorkflowInstance.Id, "process", "{}");
            workflowProcessingResult = await workflowEngineBuildResult.WorkflowEngine.ProcessMessage(processEventWorkflowMessage);

            package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "final", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
            Assert.IsTrue(workflowProcessingResult.WorkflowInstance.Entity.GetProperty<bool>("IsSent"));
        }

        [Test]
        public async Task Simple01_Cyclic_DetectCycleOfTwoStatesWithMockedStore()
        {
            var workflowEngineBuildResult = BuildWorkflowEngine(true, false, new NoOpActivity(), new NoOpEventHandler());

            var packageJsonState = new JsonState();
            packageJsonState.SetProperty("IsSent", false);

            IEventRequestWorkflowMessage initialEventWorkflowMessage = new EventRequestWorkflowMessage(
                WorkflowPackageCyclic01Id, Package01.GetType().FullName, Package01.EntityId, "initial", packageJsonState.ToJsonString());
            var workflowProcessingResult = await workflowEngineBuildResult.WorkflowEngine.ProcessMessage(initialEventWorkflowMessage);

            var package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "failed", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
            Assert.IsTrue(workflowProcessingResult.WorkflowInstance.Entity.GetProperty<bool>("IsSent"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Simple01_Cyclic_DetectCycleOfThreeStatesWithBothStores(bool mockWorkflowStore)
        {
            var workflowEngineBuildResult = BuildWorkflowEngine(mockWorkflowStore, false, new NoOpActivity(), new NoOpEventHandler());

            var packageJsonState = new JsonState();
            packageJsonState.SetProperty("IsSent", false);

            IEventRequestWorkflowMessage initialEventWorkflowMessage = new EventRequestWorkflowMessage(
                WorkflowPackageCyclic02Id, Package01.GetType().FullName, Package01.EntityId, "initial", packageJsonState.ToJsonString());
            var workflowProcessingResult = await workflowEngineBuildResult.WorkflowEngine.ProcessMessage(initialEventWorkflowMessage);

            var package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "process.03", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);
            Assert.IsTrue(workflowProcessingResult.WorkflowInstance.Entity.GetProperty<bool>("IsSent"));

            IEventRequestWorkflowMessage processEventWorkflowMessage = new EventRequestWorkflowMessage(
                WorkflowPackageCyclic02Id, workflowProcessingResult.WorkflowInstance.Id, "process", packageJsonState.ToJsonString());
            workflowProcessingResult = await workflowEngineBuildResult.WorkflowEngine.ProcessMessage(processEventWorkflowMessage);

            package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "process.03", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);

            processEventWorkflowMessage = new EventRequestWorkflowMessage(
                WorkflowPackageCyclic02Id, workflowProcessingResult.WorkflowInstance.Id, "process", packageJsonState.ToJsonString());
            workflowProcessingResult = await workflowEngineBuildResult.WorkflowEngine.ProcessMessage(processEventWorkflowMessage);

            package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "failed", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
        }

        [Test]
        public async Task Simple01_Lock_TakeTheLock()
        {
            var workflowEngineBuildResult = BuildWorkflowEngine(true, false, new NoOpActivity(), new NoOpEventHandler());

            var packageJsonState = new JsonState();
            packageJsonState.SetProperty("IsSent", false);

            IEventRequestWorkflowMessage initialEventWorkflowMessage = new EventRequestWorkflowMessage(
                WorkflowPackage01Id, Package01.GetType().FullName, Package01.EntityId, "initial", packageJsonState.ToJsonString());
            var workflowProcessingResult = await workflowEngineBuildResult.WorkflowEngine.ProcessMessage(initialEventWorkflowMessage);

            var package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "process", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);
            Assert.IsTrue(workflowProcessingResult.WorkflowInstance.Entity.GetProperty<bool>("IsSent"));
            Assert.IsTrue(package01WorkflowInstance.Lock.LockOwner == workflowEngineBuildResult.WorkflowEngine.Id);
            WorkflowStoreMock.Verify(m => m.TryLockWorkflowInstance(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
            WorkflowStoreMock.Verify(m => m.TryUnlockWorkflowInstance(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);

            IEventRequestWorkflowMessage processEventWorkflowMessage = new EventRequestWorkflowMessage(
                WorkflowPackage01Id, workflowProcessingResult.WorkflowInstance.Id, "process", "{}");
            workflowProcessingResult = await workflowEngineBuildResult.WorkflowEngine.ProcessMessage(processEventWorkflowMessage);

            package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "final", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
            Assert.IsTrue(workflowProcessingResult.WorkflowInstance.Entity.GetProperty<bool>("IsSent"));

            WorkflowStoreMock.Verify(m => m.TryLockWorkflowInstance(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
            WorkflowStoreMock.Verify(m => m.TryUnlockWorkflowInstance(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public async Task Simple01_Lock_FailedToTakeTheLock()
        {
            var workflowEngineBuildResult02 = BuildWorkflowEngine(true, false, new NoOpActivity(), new NoOpEventHandler());
            var workflowEngineBuildResult01 = BuildWorkflowEngine(true, false, new NoOpActivity(), new NoOpEventHandler());

            var packageJsonState = new JsonState();
            packageJsonState.SetProperty("IsSent", false);

            IEventRequestWorkflowMessage initialEventWorkflowMessage = new EventRequestWorkflowMessage(
                WorkflowPackage01Id, Package01.GetType().FullName, Package01.EntityId, "initial", packageJsonState.ToJsonString());
            var workflowProcessingResult = await workflowEngineBuildResult01.WorkflowEngine.ProcessMessage(initialEventWorkflowMessage);

            var package01WorkflowInstance = workflowEngineBuildResult01.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "process", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);
            Assert.IsTrue(workflowProcessingResult.WorkflowInstance.Entity.GetProperty<bool>("IsSent"));
            Assert.IsTrue(package01WorkflowInstance.Lock.LockOwner == workflowEngineBuildResult01.WorkflowEngine.Id);
            WorkflowStoreMock.Verify(m => m.TryLockWorkflowInstance(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
            WorkflowStoreMock.Verify(m => m.TryUnlockWorkflowInstance(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);

            IEventRequestWorkflowMessage processEventWorkflowMessage = new EventRequestWorkflowMessage(
                WorkflowPackage01Id, workflowProcessingResult.WorkflowInstance.Id, "process", "{}");
            Assert.ThrowsAsync<WorkflowException>(() => workflowEngineBuildResult02.WorkflowEngine.ProcessMessage(processEventWorkflowMessage));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Simple01_HappyPath_MovedToFinal(bool mockWorkflowStore)
        {
            var workflowEngineBuildResult = BuildWorkflowEngine(mockWorkflowStore, false, new NoOpActivity(), new NoOpEventHandler());

            IEvent initialEvent = new SimpleDomainEntityEvent(Package01, "initial");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage01Id, initialEvent);

            var package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "process", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);
            Assert.IsTrue(Package01.GetProperty<bool>("IsSent"));

            IEvent processEvent = new SimpleDomainEntityEvent(package01WorkflowInstance.Id, "process");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage01Id, processEvent);

            package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "final", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
        }
        
        [Test]
        public async Task Simple01_IntermediateFailure_RecoveredAndMovedToFinal()
        {
            var workflowEngineBuildResult = BuildWorkflowEngine(true, false, new NoOpActivity(), new NoOpEventHandler());

            IEvent initialEvent = new SimpleDomainEntityEvent(Package01, "initial");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage07Id, initialEvent);

            var package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "process", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);
            Assert.IsTrue(Package01.GetProperty<bool>("IsSent"));

            IEvent processEvent = new SimpleDomainEntityEvent(package01WorkflowInstance.Id, "process");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage07Id, processEvent);

            package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "final", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Simple01_FailedActivity_MovedToFailed(bool mockWorkflowStore)
        {
            var workflowEngineBuildResult = BuildWorkflowEngine(mockWorkflowStore, false, new NoOpWithExceptionActivity(), new NoOpEventHandler());

            IEvent initialEvent = new SimpleDomainEntityEvent(Package01, "initial");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage01Id, initialEvent);

            var package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "process", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);
            Assert.IsTrue(Package01.GetProperty<bool>("IsSent"));

            IEvent processEvent = new SimpleDomainEntityEvent(package01WorkflowInstance.Id, "process");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage01Id, processEvent);

            package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "failed", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Simple01_FailedEvent_MovedToFailed(bool mockWorkflowStore)
        {
            var workflowEngineBuildResult = BuildWorkflowEngine(mockWorkflowStore, false, new NoOpActivity(), new NoOpWithExceptionEventHandler());

            IEvent initialEvent = new SimpleDomainEntityEvent(Package01, "initial");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage01Id, initialEvent);

            var package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "process", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);
            Assert.IsTrue(Package01.GetProperty<bool>("IsSent"));

            IEvent processEvent = new SimpleDomainEntityEvent(package01WorkflowInstance.Id, "process");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage01Id, processEvent);

            package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "failed", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
        }
    }
}