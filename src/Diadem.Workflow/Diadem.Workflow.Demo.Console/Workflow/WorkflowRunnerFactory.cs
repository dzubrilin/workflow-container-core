using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Diadem.Workflow.Demo.Console.Execution;
using Diadem.Workflow.Demo.Console.Workflow.PackageV01;

namespace Diadem.Workflow.Demo.Console.Workflow
{
    public class WorkflowRunnerFactory : IDemoRunnerFactory
    {
        private static readonly IEnumerable<IWorkflowRunner> WorkflowRunners =
            new[]
            {
                new PackageV01WorkflowRunner()
            };

        public string Name => "Workflow Runner";

        public IDemoRunner CreateDemoRunner()
        {
            var workflowRunners = WorkflowRunners.ToArray();

            for (var i = 0; i < 3; i++)
            {
                var index = 0;
                System.Console.WriteLine("Please select workflow runner (E for exit):");
                foreach (var workflowRunner in workflowRunners)
                {
                    System.Console.WriteLine($"[{(++index).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}] - {workflowRunner.Name}");
                }

                var input = System.Console.ReadLine();
                if (string.Equals("E", input, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (!int.TryParse(input, out var position))
                {
                    if (i == 2)
                    {
                        System.Console.WriteLine("Selection failed : ( ... Exiting.");
                        return null;
                    }

                    continue;
                }

                if (position > 0 && position <= workflowRunners.Length)
                {
                    return workflowRunners[position - 1];
                }

                if (i == 2)
                {
                    System.Console.WriteLine("Selection failed : ( ... Exiting.");
                    return null;
                }

                System.Console.WriteLine($"Input must be in a range [1..{workflowRunners.Length}]");
            }

            return null;
        }
    }
}