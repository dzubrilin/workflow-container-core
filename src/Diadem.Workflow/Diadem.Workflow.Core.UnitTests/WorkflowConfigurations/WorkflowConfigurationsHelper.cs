using System.IO;
using System.Reflection;

namespace Diadem.Workflow.Core.UnitTests.WorkflowConfigurations
{
    public static class WorkflowConfigurationsHelper
    {
        internal const string FlowPackage01 = "Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.package.01.xml";

        internal const string FlowPackageCyclic01 = "Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.package.cyclic.01.xml";

        internal const string FlowPackageCyclic02 = "Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.package.cyclic.02.xml";

        internal const string FlowPackage02 = "Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.package.02.xml";

        internal const string FlowPackage03 = "Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.package.03.xml";

        internal const string FlowPackage04 = "Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.package.04.xml";

        internal const string FlowPackage05 = "Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.package.05.xml";

        internal const string FlowPackage06 = "Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.package.06.xml";
        
        internal const string FlowPackage07 = "Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.package.07.xml";

        internal const string FlowSigner01 = "Diadem.Workflow.Core.UnitTests.WorkflowConfigurations.Basic.flow.signer.01.xml";

        public static string GetWorkflowConfiguration(string resourceName)
        {
            return StreamToString(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName));
        }

        private static string StreamToString(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}