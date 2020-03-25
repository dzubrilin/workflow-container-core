using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Diadem.Web.Authentication.Basic.Events
{
    public class BasicAuthenticationFailedContext : ResultContext<BasicAuthenticationOptions>
    {
        public BasicAuthenticationFailedContext(
            HttpContext context,
            AuthenticationScheme scheme,
            BasicAuthenticationOptions options)
            : base(context, scheme, options)
        {
        }

        public Exception Exception { get; set; }
    }
}