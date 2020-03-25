using System;
using System.Net.Http;
using Diadem.Http.Core;

namespace Diadem.Http.DotNetCore
{
    public class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateHttpClient()
        {
            var socketsHttpHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(1D),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1D),
                MaxConnectionsPerServer = 10
            };

            return new HttpClient(socketsHttpHandler);
        }
    }
}