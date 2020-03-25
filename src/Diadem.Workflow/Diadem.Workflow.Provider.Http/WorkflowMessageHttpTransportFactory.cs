using System;
using System.Net.Http;
using Diadem.Http.Core;
using Diadem.Workflow.Core.Runtime;

namespace Diadem.Workflow.Provider.Http
{
    public class WorkflowMessageHttpTransportFactory : IWorkflowMessageTransportFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WorkflowMessageHttpTransportFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IWorkflowMessageTransport CreateMessageTransport(Uri address)
        {
            return new WorkflowMessageHttpTransport(_httpClientFactory.CreateHttpClient());
        }
    }
}