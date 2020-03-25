using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Diadem.Web.Authentication.Basic.Events
{
    public class ValidateCredentialsContext : ResultContext<BasicAuthenticationOptions>
    {
        public ValidateCredentialsContext(
            HttpContext context,
            AuthenticationScheme scheme,
            BasicAuthenticationOptions options)
            : base(context, scheme, options)
        {
        }

        public string AuthenticationKey { get; set; }

        public string AuthenticationSecret { get; set; }
    }
}