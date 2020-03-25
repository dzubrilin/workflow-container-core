using System;
using System.Net;
using System.Runtime.Loader;
using System.Threading;
using Autofac;
using Diadem.Messaging.Core;
using Diadem.Workflow.Provider.RabbitMq;
using Microsoft.Extensions.Configuration;
using Serilog;
using IConfigurationProvider = Diadem.Core.Configuration.IConfigurationProvider;

namespace Diadem.Workflow.Host
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        private static readonly ManualResetEventSlim ShutdownResetEvent = new ManualResetEventSlim(false);

        private static readonly ManualResetEventSlim CompleteResetEvent = new ManualResetEventSlim(false);

        public static int Main(string[] args)
        {
            AssemblyLoadContext.Default.Unloading += DefaultOnUnloading;
            
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
            Log.Information("Starting application workflow host on {machine}", System.Environment.MachineName);

            try
            {
                using (var container = AutofacContainerBuilder.CreateContainer())
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    var configurationProvider = container.Resolve<IConfigurationProvider>();
                    var workflowRabbitMqBootstrap = new WorkflowRabbitMqBootstrap(configurationProvider);
                    workflowRabbitMqBootstrap.Configure();

                    var busControlFactory = container.Resolve<IBusControlFactory>();
                    var busControl = busControlFactory.CreateBusControl();
                    busControl.StartAsync(cancellationTokenSource.Token);

                    ShutdownResetEvent.Wait(cancellationTokenSource.Token);
                    CompleteResetEvent.Set();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred during workflow host execution {error}", ex.Message);
            }

            Log.Information("Shutting down application workflow host on {machine}", System.Environment.MachineName);
            return 0;
        }

        private static void DefaultOnUnloading(AssemblyLoadContext obj)
        {
            ShutdownResetEvent.Set();
            CompleteResetEvent.Wait();
        }
    }
}