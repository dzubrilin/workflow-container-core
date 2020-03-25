using System.Net.Http;

namespace Diadem.Http.Core
{
    public interface IHttpClientFactory
    {
        HttpClient CreateHttpClient();
    }
}