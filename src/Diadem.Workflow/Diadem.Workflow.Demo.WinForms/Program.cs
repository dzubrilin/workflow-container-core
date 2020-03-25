using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Loader;
using System.Threading;
using Common.Logging;
using Common.Logging.Configuration;
using Common.Logging.NLog;
using Flow.Core;
using Flow.Demo.WorkflowCommands;

namespace Flow.Demo
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        private static readonly ILog Logger;

        private static readonly ManualResetEventSlim ShutdownResetEvent = new ManualResetEventSlim(false);

        private static readonly ManualResetEventSlim CompleteResetEvent = new ManualResetEventSlim(false);

        static Program()
        {
            var props = new NameValueCollection
            {
                {"configType", "FILE"},
                {"configFile", "./nlog.config"}
            };
            LogManager.Adapter = new NLogLoggerFactoryAdapter(props);
            Logger = LogManager.GetLogger<Program>();
        }

        public static int Main(string[] args)
        {
            try
            {
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    Logger.Info($"Starting application workflow demo on [{Dns.GetHostName()}]");
                    //AssemblyLoadContext.Default.Unloading += DefaultOnUnloading;

                    while (true)
                    {
                        Run();
                        Console.WriteLine("Do you want to execute one more workflow (Y/N)?");
                        var input = Console.ReadLine();
                        if (!string.Equals(input, "Y", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        break;
                    }

                    ShutdownResetEvent.Wait(cancellationTokenSource.Token);
                    Logger.Info($"Shutting down application workflow demo on [{Dns.GetHostName()}]");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An error has occurred during workflow host execution [{ex.Message}]", ex);
            }

            CompleteResetEvent.Set();
            return 0;
        }

        private static void Run()
        {
            var workflowRunner = GetWorkflowRunner();
            if (null == workflowRunner)
            {
                Console.WriteLine("Exiting...");
                ShutdownResetEvent.Set();
                return;
            }

            Console.WriteLine($"Starting execution of workflow [{workflowRunner.Name}]");
            var workflowCommandContext = new WorkflowCommandContext(workflowRunner.EntityType, workflowRunner.WorkflowId);
            while (true)
            {
                var workflowCommand = GetWorkflowCommand(workflowRunner);
                if (null == workflowCommand)
                {
                    Console.WriteLine($"Finished execution of workflow [{workflowRunner.Name}]");
                    return;
                }

                workflowCommand.Execute(workflowCommandContext);
            }
        }

        private static IWorkflowCommand GetWorkflowCommand(IWorkflowRunner workflowRunner)
        {
            var workflowCommands = workflowRunner.WorkflowCommands.ToArray();

            for (int i = 0; i < 3; i++)
            {
                var index = 0;
                Console.WriteLine("Please select workflow command (E for exit):");
                foreach (var workflowCommand in workflowCommands)
                {
                    Console.WriteLine($"[{(++index).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}] - {workflowCommand.Name}");
                }

                var input = Console.ReadLine();
                if (string.Equals("E", input, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (!int.TryParse(input, out var position))
                {
                    if (i == 2)
                    {
                        Console.WriteLine("Selection failed : ( ... Exiting.");
                        return null;
                    }

                    continue;
                }

                if (position > 0 && position <= workflowCommands.Length)
                {
                    return workflowCommands[position - 1];
                }

                if (i == 2)
                {
                    Console.WriteLine("Selection failed : ( ... Exiting.");
                    return null;
                }

                Console.WriteLine($"Input must be in a range [1..{workflowCommands.Length}]");
            }

            return null;
        }

        private static IWorkflowRunner GetWorkflowRunner()
        {
            var workflowRunnerFactory = new WorkflowRunnerFactory();
            var workflowRunners = workflowRunnerFactory.CreateWorkflowRunners().ToArray();

            for (int i = 0; i < 3; i++)
            {
                var index = 0;
                Console.WriteLine("Please select workflow runner (E for exit):");
                foreach (var workflowRunner in workflowRunners)
                {
                    Console.WriteLine($"[{(++index).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}] - {workflowRunner.Name}");
                }

                var input = Console.ReadLine();
                if (string.Equals("E", input, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (!int.TryParse(input, out var position))
                {
                    if (i == 2)
                    {
                        Console.WriteLine("Selection failed : ( ... Exiting.");
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
                    Console.WriteLine("Selection failed : ( ... Exiting.");
                    return null;
                }

                Console.WriteLine($"Input must be in a range [1..{workflowRunners.Length}]");
            }

            return null;
        }

        private static void DefaultOnUnloading(AssemblyLoadContext obj)
        {
            ShutdownResetEvent.Set();
            CompleteResetEvent.Wait();
        }
    }
}