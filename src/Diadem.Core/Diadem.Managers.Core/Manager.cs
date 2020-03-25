using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Diadem.Core;
using Diadem.Core.Commands;
using Diadem.Core.Configuration;
using Serilog;
using Serilog.Events;

namespace Diadem.Managers.Core
{
    public abstract class Manager : IManager
    {
        private static readonly bool IsDebugEnabled = Log.IsEnabled(LogEventLevel.Debug);
        
        private static readonly bool IsErrorEnabled = Log.IsEnabled(LogEventLevel.Error);
        
        private readonly ILifetimeScope _lifetimeScope;
        
        protected Manager(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public async Task<TManagerCommandResponse> Handle<TManagerCommandRequest, TManagerCommandResponse>(TManagerCommandRequest request, CancellationToken cancellationToken)
            where TManagerCommandRequest : ManagerCommandRequest
            where TManagerCommandResponse : ManagerCommandResponse, new()
        {
            Guard.ArgumentNotNull(request, nameof(request));

            Stopwatch stopwatch = null;
            if (IsDebugEnabled || IsErrorEnabled)
            {
                stopwatch = Stopwatch.StartNew();
            }
            
            var commandHandler = _lifetimeScope.Resolve<IManagerCommandHandler<TManagerCommandRequest, TManagerCommandResponse>>();
            if (null == commandHandler)
            {
                Log.Warning("Cannot find handler for manager command for [<{request}, {response}>]",
                    typeof(TManagerCommandRequest).Name, typeof(TManagerCommandResponse).Name);

                return ManagerCommandResponse.Create<TManagerCommandResponse>(
                    CommandExecutionStatus.ExecutionFailed,
                    new CommandExecutionInternalErrorResult(
                        $"Cannot find handler for manager command for [<{typeof(TManagerCommandRequest).Name}, {typeof(TManagerCommandResponse).Name}>]"));
            }

            Log.Debug("Starting processing manager command [<{request}, {response}>]",
                typeof(TManagerCommandRequest).Name, typeof(TManagerCommandResponse).Name);

            try
            {
                var managerCommandResponse = await commandHandler.Handle(request, cancellationToken).ConfigureAwait(false);
                if (managerCommandResponse.CommandExecutionStatus == CommandExecutionStatus.Completed)
                {
                    stopwatch?.Stop();
                    Log.Debug("Finished processing manager command [<{request}, {response}>] {duration}",
                        typeof(TManagerCommandRequest).Name, typeof(TManagerCommandResponse).Name, stopwatch?.Elapsed);
                }
                else
                {
                    stopwatch?.Stop();
                    Log.Warning("Processing manager command [<{request}, {response}>] {duration} has failed with [{status}], [{message}]",
                        typeof(TManagerCommandRequest).Name, typeof(TManagerCommandResponse).Name, stopwatch?.Elapsed,
                        managerCommandResponse.CommandExecutionStatus, managerCommandResponse.CommandExecutionResult?.Description ?? "NULL");
                }

                return managerCommandResponse;
            }
            catch (Exception ex)
            {
                stopwatch?.Stop();
                Log.Error(ex, "An error has occurred during processing manager command [<{request}, {response}>] [{duration}]",
                    typeof(TManagerCommandRequest).Name,typeof(TManagerCommandResponse).Name, stopwatch?.Elapsed);

                return ManagerCommandResponse.Create<TManagerCommandResponse>(CommandExecutionStatus.InternalServerError, new CommandExecutionInternalErrorResult());
            }
        }

        public async Task<TManagerCommandResponse> HandleRemote<TManagerCommandRequest, TManagerCommandResponse>(TManagerCommandRequest request, CancellationToken cancellationToken)
            where TManagerCommandRequest : ManagerCommandRequest
            where TManagerCommandResponse : ManagerCommandResponse, new()
        {
            Guard.ArgumentNotNull(request, nameof(request));

            Stopwatch stopwatch = null;
            if (IsDebugEnabled || IsErrorEnabled)
            {
                stopwatch = Stopwatch.StartNew();
            }
            
            Log.Debug("Starting processing remote manager command [<{request}, {response}>]",
                typeof(TManagerCommandRequest).Name, typeof(TManagerCommandResponse).Name);

            var endpointConfiguration = GetRemoteEndpointConfiguration();
            if (null == endpointConfiguration)
            {
                Log.Warning("No remote endpoint configuration is defined for [<{request}, {response}>]",
                    typeof(TManagerCommandRequest).Name, typeof(TManagerCommandResponse).Name);

                return ManagerCommandResponse.Create<TManagerCommandResponse>(
                    CommandExecutionStatus.ExecutionFailed,
                    new CommandExecutionInternalErrorResult($"No remote endpoint configuration is defined for [{GetType().FullName}]"));
            }

            var managerCommandTransportFactory = GetManagerCommandTransportFactory();
            if (null == managerCommandTransportFactory)
            {
                Log.Warning("No command transport factory is defined for [<{request}, {response}>]",
                    typeof(TManagerCommandRequest).Name, typeof(TManagerCommandResponse).Name);
                
                return ManagerCommandResponse.Create<TManagerCommandResponse>(
                    CommandExecutionStatus.ExecutionFailed,
                    new CommandExecutionInternalErrorResult($"No command transport factory is defined for [{GetType().FullName}]"));
            }

            var managerCommandTransport = managerCommandTransportFactory.CreateCommandTransport();
            if (null == managerCommandTransport)
            {
                Log.Warning("No command transport is defined for for [<{request}, {response}>]",
                    typeof(TManagerCommandRequest).Name, typeof(TManagerCommandResponse).Name);
                
                return ManagerCommandResponse.Create<TManagerCommandResponse>(
                    CommandExecutionStatus.ExecutionFailed,
                    new CommandExecutionInternalErrorResult($"No command transport is defined for [{GetType().FullName}]"));
            }

            try
            {
                var managerCommandResponse = await managerCommandTransport
                    .Request<TManagerCommandRequest, TManagerCommandResponse>(endpointConfiguration, request, cancellationToken)
                    .ConfigureAwait(false);
                if (managerCommandResponse.CommandExecutionStatus == CommandExecutionStatus.Completed)
                {
                    stopwatch?.Stop();
                    Log.Debug("Finished processing manager remote command [<{request}, {response}>] {duration}",
                        typeof(TManagerCommandRequest).Name, typeof(TManagerCommandResponse).Name, stopwatch?.Elapsed);
                }
                else
                {
                    stopwatch?.Stop();
                    Log.Warning("Processing manager remote command [<{request}, {response}>]{duration} has failed with [{status}], [{message}]",
                        typeof(TManagerCommandRequest).Name, typeof(TManagerCommandResponse).Name, stopwatch?.Elapsed,
                        managerCommandResponse.CommandExecutionStatus, managerCommandResponse.CommandExecutionResult?.Description ?? "NULL");
                }

                return managerCommandResponse;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred during processing manager remote command [<{request}, {response}>] {duration}",
                    typeof(TManagerCommandRequest).Name, typeof(TManagerCommandResponse).Name, stopwatch?.Elapsed);

                return ManagerCommandResponse.Create<TManagerCommandResponse>(CommandExecutionStatus.InternalServerError, new CommandExecutionInternalErrorResult());
            }
        }

        protected abstract IEndpointConfiguration GetRemoteEndpointConfiguration();

        protected abstract IManagerCommandTransportFactory GetManagerCommandTransportFactory();
    }
}