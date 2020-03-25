using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Execution.Events;
using Diadem.Workflow.Core.Execution.States;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.UnitTests.Model;
using Diadem.Workflow.Core.UnitTests.Model.Activities;
using Diadem.Workflow.Core.UnitTests.Model.EventHandlers;
using Diadem.Workflow.Core.UnitTests.Model.Events;
using NUnit.Framework;

namespace Diadem.Workflow.Core.UnitTests.WorkflowEngineTests
{
    public class WorkflowEngineTestsAsync01 : WorkflowEngineTestBase
    {
        [SetUp]
        public void Setup()
        {
            Signer01 = new Signer("1");
            Signer02 = new Signer("2");
            Package01 = new Package("1", new List<Signer>(new[] {(Signer)Signer01, (Signer)Signer02}));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Simple01_HappyPath_MovedToFinal(bool mockWorkflowStore)
        {
            var workflowEngineBuildResult = BuildWorkflowEngine(mockWorkflowStore, false, new NoOpActivity(), new NoOpEventHandler());

            IEvent initialEvent = new SimpleDomainEntityEvent(Package01, "initial");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage04Id, initialEvent);

            var package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "process", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingAsyncTransition);
            Assert.IsTrue(Package01.GetProperty<bool>("IsSent"));

            IWorkflowMessage workflowMessage = new AsynchronousTransitionWorkflowMessage(WorkflowPackage04Id, package01WorkflowInstance.Id, "{}");
            await workflowEngineBuildResult.WorkflowEngine.ProcessMessage(workflowMessage);

            package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "final", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Simple01_Failed_DoNotAllowAsyncTransitionWhileWaitingEvent(bool mockWorkflowStore)
        {
            var workflowEngineBuildResult = BuildWorkflowEngine(mockWorkflowStore, false, new NoOpActivity(), new NoOpEventHandler());

            IEvent initialEvent = new SimpleDomainEntityEvent(Package01, "initial");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage01Id, initialEvent);

            var package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "process", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);
            Assert.IsTrue(Package01.GetProperty<bool>("IsSent"));

            IWorkflowMessage workflowMessage = new AsynchronousTransitionWorkflowMessage(WorkflowPackage01Id, package01WorkflowInstance.Id, "{}");
            await workflowEngineBuildResult.WorkflowEngine.ProcessMessage(workflowMessage);

            package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "failed", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Simple01_Failed_DoNotAllowEventsWhileWaitingAsyncTransition(bool mockWorkflowStore)
        {
            var workflowEngineBuildResult = BuildWorkflowEngine(mockWorkflowStore, false, new NoOpActivity(), new NoOpEventHandler());

            IEvent initialEvent = new SimpleDomainEntityEvent(Package01, "initial");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage04Id, initialEvent);

            var package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "process", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingAsyncTransition);
            Assert.IsTrue(Package01.GetProperty<bool>("IsSent"));

            IEvent processEvent = new SimpleDomainEntityEvent(package01WorkflowInstance.Id, "process");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage04Id, processEvent);

            package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "failed", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
        }
    }
}