using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Execution.Events;
using Diadem.Workflow.Core.Execution.States;
using Diadem.Workflow.Core.UnitTests.Model;
using Diadem.Workflow.Core.UnitTests.Model.Activities;
using Diadem.Workflow.Core.UnitTests.Model.EventHandlers;
using Diadem.Workflow.Core.UnitTests.Model.Events;
using NUnit.Framework;

namespace Diadem.Workflow.Core.UnitTests.WorkflowEngineTests
{
    public class WorkflowEngineTestNested01 : WorkflowEngineTestBase
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
        public async Task Nested01_HappyPath_MovedToFinal(bool mockWorkflowStore)
        {
            var workflowEngineBuildResult = BuildWorkflowEngine(mockWorkflowStore, true, new NoOpActivity(), new NoOpEventHandler());

            IEvent initialEvent = new SimpleDomainEntityEvent(Package01, "initial");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage02Id, initialEvent);

            var package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "awaitingSigners", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);
            Assert.IsFalse(Package01.GetProperty<bool>("IsSent"));

            var signer01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Signer01);
            Assert.IsTrue(string.Equals(signer01WorkflowInstance.CurrentStateCode, "awaitingBadges", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(signer01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);
            Assert.IsTrue(Signer01.GetProperty<bool>("IsSent"));

            // send event to a nested Signer01 to trigger its completion
            IEvent processEvent = new SimpleDomainEntityEvent(signer01WorkflowInstance.Id, "process");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowSigner01Id, processEvent);

            signer01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Signer01);
            Assert.IsTrue(string.Equals(signer01WorkflowInstance.CurrentStateCode, "final", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(signer01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
            Assert.IsTrue(Signer01.GetProperty<DateTime>("LastVisited") > DateTime.MinValue);
            Assert.IsTrue(Signer01.GetProperty<bool>("VisitedIntermediateState"));

            package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "final", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
            Assert.IsTrue(Package01.GetProperty<bool>("IsSent"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Nested01_NestedWorkflowFailed_MovedToFailed(bool mockWorkflowStore)
        {
            var workflowEngineBuildResult = BuildWorkflowEngine(mockWorkflowStore, true, new NoOpWithExceptionActivity(), new NoOpEventHandler());

            IEvent initialEvent = new SimpleDomainEntityEvent(Package01, "initial");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowPackage02Id, initialEvent);

            var package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "awaitingSigners", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);
            Assert.IsFalse(Package01.GetProperty<bool>("IsSent"));

            var signer01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Signer01);
            Assert.IsTrue(string.Equals(signer01WorkflowInstance.CurrentStateCode, "awaitingBadges", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(signer01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.AwaitingEvent);
            Assert.IsTrue(Signer01.GetProperty<bool>("IsSent"));

            // send event to a nested Signer01 to trigger its move to failed ==> Package01 & Singer01 & Signer02 must be moved to failed
            IEvent processEvent = new SimpleDomainEntityEvent(signer01WorkflowInstance.Id, "process");
            await workflowEngineBuildResult.WorkflowEngine.ProcessEvent(WorkflowSigner01Id, processEvent);

            package01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Package01);
            Assert.IsTrue(string.Equals(package01WorkflowInstance.CurrentStateCode, "failed", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(package01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
            Assert.IsFalse(Package01.GetProperty<bool>("IsSent"));

            signer01WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Signer01);
            Assert.IsTrue(string.Equals(signer01WorkflowInstance.CurrentStateCode, "failed", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(signer01WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);

            var signer02WorkflowInstance = workflowEngineBuildResult.GetWorkflowInstance(Signer02);
            Assert.IsTrue(string.Equals(signer02WorkflowInstance.CurrentStateCode, "failed", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(signer02WorkflowInstance.CurrentStateProgress == StateExecutionProgress.Completed);
        }
    }
}