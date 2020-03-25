using Autofac;
using Diadem.Http.Core;

namespace Diadem.Http.DotNetCore
{
    public class HttpClientFactoryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HttpClientFactory>().As<IHttpClientFactory>().SingleInstance();
        }
    }
}