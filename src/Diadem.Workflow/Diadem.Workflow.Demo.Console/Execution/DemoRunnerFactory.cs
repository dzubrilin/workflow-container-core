using System;
using System.Globalization;
using Diadem.Workflow.Demo.Console.Workflow;

namespace Diadem.Workflow.Demo.Console.Execution
{
    public class DemoRunnerFactory
    {
        private static readonly IDemoRunnerFactory[] DemoRunnerFactories =
        {
            new WorkflowRunnerFactory()
        };

        public IDemoRunnerFactory CreateRunnerFactory()
        {
            for (var i = 0; i < 3; i++)
            {
                var index = 0;
                System.Console.WriteLine("Please select runner (E for exit):");
                foreach (var demoRunnerFactory in DemoRunnerFactories)
                {
                    System.Console.WriteLine($"[{(++index).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}] - {demoRunnerFactory.Name}");
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

                if (position > 0 && position <= DemoRunnerFactories.Length)
                {
                    return DemoRunnerFactories[position - 1];
                }

                if (i == 2)
                {
                    System.Console.WriteLine("Selection failed : ( ... Exiting.");
                    return null;
                }

                System.Console.WriteLine($"Input must be in a range [1..{DemoRunnerFactories.Length}]");
            }

            return null;
        }
    }
}