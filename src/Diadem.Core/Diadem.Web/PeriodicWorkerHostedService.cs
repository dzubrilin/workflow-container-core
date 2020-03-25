using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Diadem.Web
{
    public abstract class PeriodicWorkerHostedService : IHostedService, IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource;

        private Task _executingTask;
        
        protected PeriodicWorkerHostedService(IServiceProvider services)
        {
            Services = services;
        }

        protected IServiceProvider Services { get; }
        
        protected abstract TimeSpan Interval { get; }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Log.IsEnabled(LogEventLevel.Verbose))
            {
                Log.Verbose("Starting {periodicActionHandler} at {machine}", GetType().Name, Environment.MachineName);
            }
            
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _executingTask = ExecuteInternal(_cancellationTokenSource.Token);
            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
            
            if (Log.IsEnabled(LogEventLevel.Verbose))
            {
                Log.Verbose("Stopped {periodicActionHandler} at {machine}", GetType().Name, Environment.MachineName);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }

        protected abstract Task Execute(CancellationToken cancellationToken);
        
        private async Task ExecuteInternal(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (Log.IsEnabled(LogEventLevel.Verbose))
                    {
                        Log.Verbose("Executing {periodicActionHandler} at {machine}", GetType().Name, Environment.MachineName);
                    }
                    
                    try
                    {
                        await Execute(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "An error has occurred during executing {periodicActionHandler} on {machine}",
                            GetType().Name,Environment.MachineName);
                    }
 
                    await Task.Delay(Interval, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred during execution of {periodicActionHandler} at {machine}",
                    GetType().Name, Environment.MachineName);
            }
        }
    }
}