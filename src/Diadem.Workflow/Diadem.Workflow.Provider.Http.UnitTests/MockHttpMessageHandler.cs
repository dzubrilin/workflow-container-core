using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Diadem.Workflow.Provider.Http.UnitTests
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        private readonly string _response;

        public MockHttpMessageHandler(HttpStatusCode statusCode, string response)
        {
            _statusCode = statusCode;
            _response = response;
        }
        
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage {StatusCode = _statusCode, Content = new StringContent(_response)});
        }
    }
}