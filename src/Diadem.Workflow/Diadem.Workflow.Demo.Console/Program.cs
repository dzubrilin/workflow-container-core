using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Diadem.Messaging.Core;
using Diadem.Workflow.Demo.Console.Execution;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Diadem.Workflow.Demo.Console
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        private static readonly ManualResetEventSlim ShutdownResetEvent = new ManualResetEventSlim(false);

        private static readonly ManualResetEventSlim CompleteResetEvent = new ManualResetEventSlim(false);

        private static readonly ManualResetEventSlim CompleteBusResetEvent = new ManualResetEventSlim(false);

        static Program()
        {
            // TODO: embed application bootstrap services
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }
        
        public static async Task<int> Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
            Log.Information("Starting application workflow demo on {machine}", Environment.MachineName);
            
            try
            {
                using (var cancellationTokenSource = new CancellationTokenSource())
                using (var compositionContainer = AutofacContainerBuilder.CreateContainer())
                {
                    var cts = cancellationTokenSource;
                    var container = compositionContainer;

                    // 1. initialize MassTransit Bus infrastructure
                    //    to make it be able to handle request/response communication outgoing from this process
                    var busTask = Task.Factory.StartNew(() => RunBus(container, cts.Token), cts.Token);

                    // 2. execute normal logic
                    while (true)
                    {
                        await Run(container, cancellationTokenSource.Token);
                        
                        System.Console.WriteLine("Do you want to execute one more runner (Y/N)?");
                        var input = System.Console.ReadLine();
                        if (string.Equals(input, "Y", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        ShutdownResetEvent.Set();
                        break;
                    }

                    CompleteBusResetEvent.Wait(cts.Token);
                    busTask.Wait(cts.Token);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred during workflow host execution [{error}]", ex.Message);
            }

            Log.Information("Shutting down application workflow demo on {machine}", System.Environment.MachineName);
            CompleteResetEvent.Set();
            return 0;
        }

        private static async Task RunBus(IContainer container, CancellationToken cancellationToken)
        {
            var busControlFactory = container.Resolve<IBusControlFactory>();
            var busControl = busControlFactory.CreateBusControl();
            await busControl.StartAsync(cancellationToken);

            Log.Verbose("MessageBus has been started in workflow demo on {machine}", System.Environment.MachineName);

            ShutdownResetEvent.Wait(cancellationToken);
            await busControl.StopAsync(cancellationToken);
            CompleteBusResetEvent.Set();

            Log.Verbose("MessageBus has been stopped in workflow demo on {machine}", System.Environment.MachineName);
        }

        private static async Task Run(IContainer container, CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    if (ShutdownResetEvent.IsSet)
                    {
                        break;
                    }

                    var demoRunnerFactory = new DemoRunnerFactory();
                    var demoRunner = demoRunnerFactory.CreateRunnerFactory();
                    if (null == demoRunner)
                    {
                        System.Console.WriteLine("Exiting...");
                        return;
                    }

                    var runner = demoRunner.CreateDemoRunner();
                    if (null == runner)
                    {
                        System.Console.WriteLine("Exiting...");
                        return;
                    }

                    var runnerContext = new RunnerContext(container, runner);
                    await runner.Run(runnerContext, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"An error has occurred during workflow execution [{ex.Message}]");
            }
        }
    }
}