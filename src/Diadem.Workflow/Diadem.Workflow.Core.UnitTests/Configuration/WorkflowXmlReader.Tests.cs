using System;
using System.IO;
using System.Reflection;
using Diadem.Workflow.Core.Configuration;
using NUnit.Framework;

namespace Diadem.Workflow.Core.UnitTests.Configuration
{
    public class WorkflowXmlReaderTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Parse_Simple_Xml()
        {
            WorkflowConfiguration workflowConfiguration;
            var workflowReader = new WorkflowXmlParser();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.package.01.xml"))
            using (var streamReader = new StreamReader(stream))
            {
                workflowConfiguration = workflowReader.Parse(streamReader.ReadToEnd());
            }

            Assert.IsTrue(string.Equals(workflowConfiguration.Code, "test.v.0.0.1", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(workflowConfiguration.States.Count == 4);
        }

        [Test]
        public void Parse_Failed_ActivityFailureMustReferToExistingTransition()
        {
            var workflowReader = new WorkflowXmlParser();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.package.07.activity.retry.failure.xml"))
            using (var streamReader = new StreamReader(stream))
            {
                Assert.Throws<WorkflowConfigurationException>(() => workflowReader.Parse(streamReader.ReadToEnd()));
            }
        }

        [Test]
        public void Parse_Failed_DoNotAllowAsyncTransitionAndEventForTheSameState()
        {
            var workflowReader = new WorkflowXmlParser();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.package.03.xml"))
            using (var streamReader = new StreamReader(stream))
            {
                Assert.Throws<WorkflowConfigurationException>(() => workflowReader.Parse(streamReader.ReadToEnd()));
            }
        }
        
        [Test]
        public void Parse_Failed_DoNotAllowDuplicatedEventDeclarationForTheSameState()
        {
            var workflowReader = new WorkflowXmlParser();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.package.duplicated.event.01.xml"))
            using (var streamReader = new StreamReader(stream))
            {
                Assert.Throws<WorkflowConfigurationException>(() => workflowReader.Parse(streamReader.ReadToEnd()));
            }
        }
    }
}