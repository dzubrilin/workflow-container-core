using System;
using System.Threading;
using System.Threading.Tasks;

namespace Diadem.Web.Authentication.Basic.Events
{
    public abstract class BasicAuthenticationEvents
    {
        public abstract Task AuthenticationFailed(BasicAuthenticationFailedContext context);

        public abstract Task ValidateCredentials(ValidateCredentialsContext context);
    }
}