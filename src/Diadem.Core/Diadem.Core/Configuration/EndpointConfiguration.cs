using System;
using System.Collections.Generic;

namespace Diadem.Core.Configuration
{
    public class EndpointConfiguration : IEndpointConfiguration
    {
        public EndpointConfiguration(string code, EndpointConfigurationType type, Uri address,
                                     IConfigurationAuthentication authentication, IList<KeyValuePair<string, string>> parameters = null)
        {
            Code = code;
            Type = type;
            Address = address;
            Parameters = parameters;
            Authentication = authentication;
        }

        public string Code { get; }
        
        public Uri Address { get; }
        
        public IList<KeyValuePair<string, string>> Parameters { get; set; }

        public EndpointConfigurationType Type { get; set; }
        
        public IConfigurationAuthentication Authentication { get; set; }
    }
}