using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core;
using Diadem.Core.Configuration;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.Runtime;
using Serilog;
using Serilog.Events;

namespace Diadem.Workflow.Provider.Http
{
    public class WorkflowMessageHttpTransport : IWorkflowMessageTransport
    {
        private readonly HttpClient _httpClient;

        public WorkflowMessageHttpTransport(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TResponseWorkflowMessage> Request<TRequestWorkflowMessage, TResponseWorkflowMessage>(
            IEndpointConfiguration endpointConfiguration, TRequestWorkflowMessage workflowMessage, CancellationToken cancellationToken)
            where TRequestWorkflowMessage : class, IWorkflowMessage
            where TResponseWorkflowMessage : class, IWorkflowMessage
        {
            VerifyEndpointConfiguration(endpointConfiguration);
            SetAuthenticationParameters(endpointConfiguration);
            
            var uriBuilder = new WorkflowMessageHttpUriBuilder(endpointConfiguration.Address);
            var httpAddress = uriBuilder.Build(workflowMessage.State.JsonState);
            var httpMethod = GetHttpMethod(endpointConfiguration);

            try
            {
                var httpResponseMessage = await MakeHttpCall(httpMethod, httpAddress, workflowMessage.State.JsonState, cancellationToken)
                    .ConfigureAwait(false);
                
                var responseString = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (Log.IsEnabled(LogEventLevel.Verbose))
                {
                    Log.Verbose("Finished {method} request to {address} with {statusCode} and {response}",
                        httpMethod, httpAddress, httpResponseMessage.StatusCode, responseString);
                }

                var responseMessageType = ResolveClassByInterface<TResponseWorkflowMessage>();
                var responseMessage = (TResponseWorkflowMessage) Activator.CreateInstance(responseMessageType);
                responseMessage.WorkflowMessageId = Guid.NewGuid();
                responseMessage.WorkflowId = workflowMessage.WorkflowId;
                responseMessage.WorkflowInstanceId = workflowMessage.WorkflowInstanceId;

                if (responseString.TryConvertToJsonState(out var jsonState))
                {
                    responseMessage.State = new WorkflowMessageState(jsonState);
                }
                
                return responseMessage;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred during making call to {address}", httpAddress);
                throw;
            }
        }

        public async Task Send<TWorkflowMessage>(IEndpointConfiguration endpointConfiguration, TWorkflowMessage workflowMessage, CancellationToken cancellationToken)
            where TWorkflowMessage : class, IWorkflowMessage
        {
            VerifyEndpointConfiguration(endpointConfiguration);
            SetAuthenticationParameters(endpointConfiguration);
            
            var uriBuilder = new WorkflowMessageHttpUriBuilder(endpointConfiguration.Address);
            var httpAddress = uriBuilder.Build(workflowMessage.State.JsonState);
            var httpMethod = GetHttpMethod(endpointConfiguration);

            await MakeHttpCall(httpMethod, httpAddress, workflowMessage.State.JsonState, cancellationToken).ConfigureAwait(false);
        }

        public Task SendWithDelay<TWorkflowMessage>(IEndpointConfiguration endpointConfiguration, TWorkflowMessage workflowMessage, CancellationToken cancellationToken)
            where TWorkflowMessage : class, IDelayedWorkflowMessage
        {
            throw new NotImplementedException("HTTP transport does not support delayed sending");
        }

        private async Task<HttpResponseMessage> MakeHttpCall(HttpMethod httpMethod, Uri httpAddress, string jsonStateString, CancellationToken cancellationToken)
        {
            if (Log.IsEnabled(LogEventLevel.Verbose))
            {
                Log.Verbose("Starting {method} request to {address} with {body}",
                    httpMethod, httpAddress, jsonStateString);
            }
            
            HttpResponseMessage httpResponseMessage = null;
            if (httpMethod == HttpMethod.Get)
            {
                httpResponseMessage = await _httpClient.GetAsync(httpAddress, HttpCompletionOption.ResponseContentRead, cancellationToken)
                                                       .ConfigureAwait(false);
            }
            else if (httpMethod == HttpMethod.Post)
            {
                var httpContent = new StringContent(jsonStateString, Encoding.UTF8);
                httpResponseMessage = await _httpClient.PostAsync(httpAddress, httpContent, cancellationToken)
                                                       .ConfigureAwait(false);
            }

            if (null == httpResponseMessage)
            {
                throw new ApplicationException($"Http {httpMethod} request to {httpAddress} has failed");
            }
            
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                if (Log.IsEnabled(LogEventLevel.Verbose))
                {
                    Log.Verbose("Finished {method} request to {address} with {statusCode}", httpMethod, httpAddress, httpResponseMessage.StatusCode);
                }
                
                throw new ApplicationException($"{httpMethod} request has failed with StatusCode=[{httpResponseMessage.StatusCode}]");
            }

            return httpResponseMessage;
        }

        private static HttpMethod GetHttpMethod(IEndpointConfiguration endpointConfiguration)
        {
            if (null == endpointConfiguration.Parameters)
            {
                return HttpMethod.Get;
            }
            
            if (!TryGetParameter(endpointConfiguration.Parameters, "Method", out var method))
            {
                return HttpMethod.Get;
            }
            
            if (!(string.Equals("GET", method, StringComparison.OrdinalIgnoreCase) || string.Equals("POST", method, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ApplicationException($"WorkflowMessageHttpTransport does not support '{method.ToUpper()}' method");
            }
            
            return new HttpMethod(method.ToUpper());
        }

        private static void VerifyEndpointConfiguration(IEndpointConfiguration endpointConfiguration)
        {
            Guard.ArgumentNotNull(endpointConfiguration, nameof(endpointConfiguration));
            Guard.ArgumentNotNull(endpointConfiguration.Address, nameof(endpointConfiguration.Address));

            var scheme = endpointConfiguration.Address.Scheme;
            if (!(string.Equals(Uri.UriSchemeHttp, scheme, StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(Uri.UriSchemeHttps, scheme, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ApplicationException($"Endpoint [{endpointConfiguration.Code}] uses Type = 'Http' but Address scheme is '{scheme}'");
            }
        }

        private void SetAuthenticationParameters(IEndpointConfiguration endpointConfiguration)
        {
            if (null == endpointConfiguration.Authentication || endpointConfiguration.Authentication.Type == ConfigurationAuthenticationType.None)
            {
                return;
            }

            if (endpointConfiguration.Authentication.Type == ConfigurationAuthenticationType.Basic)
            {
                SetBasicAuthenticationParameters(endpointConfiguration);
            }
        }

        private void SetBasicAuthenticationParameters(IEndpointConfiguration endpointConfiguration)
        {
            if (!TryGetParameter(endpointConfiguration.Authentication.Parameters, "UserName", out var username))
            {
                throw new ApplicationException($"Endpoint {endpointConfiguration.Code} is configured to use 'Basic' authentication but no UserName has been provided");
            }

            if (!TryGetParameter(endpointConfiguration.Authentication.Parameters, "Password", out var password))
            {
                throw new ApplicationException($"Endpoint {endpointConfiguration.Code} is configured to use 'Basic' authentication but no Password has been provided");
            }

            var authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue ("Basic", authorization);
        }

        private static bool TryGetParameter(IEnumerable<KeyValuePair<string, string>> parameters, string key, out string value)
        {
            if (null == parameters)
            {
                value = string.Empty;
                return false;
            }

            foreach (var keyValuePair in parameters)
            {
                if (string.Equals(keyValuePair.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = keyValuePair.Value;
                    return true;
                }
            }
            
            value = string.Empty;
            return false;
        }

        private static Type ResolveClassByInterface<TResponseWorkflowMessage>()
        {
            var type = typeof(TResponseWorkflowMessage);
            if (type.IsClass)
            {
                return type;
            }

            if (!type.IsInterface)
            {
                throw new ApplicationException($"Type {type.FullName} is not an interface");
            }
            
            var assembly = type.GetTypeInfo().Assembly;
            var assemblyTypes = assembly.DefinedTypes.Where(x => x.ImplementedInterfaces.Contains(type));
            foreach (var assemblyType in assemblyTypes)
            {
                if (assemblyType.DeclaredConstructors.Any(ctor => !ctor.GetParameters().Any()))
                {
                    return assemblyType;
                }
            }

            throw new ApplicationException($"Can not find a class with parameterless constructor that implements {type.FullName} in assembly {assembly.FullName}");
        }
    }
}