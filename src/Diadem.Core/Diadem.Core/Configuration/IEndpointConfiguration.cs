using System;
using System.Collections.Generic;

namespace Diadem.Core.Configuration
{
    public interface IEndpointConfiguration
    {
        Uri Address { get; }
        
        IList<KeyValuePair<string, string>> Parameters { get; set; }

        string Code { get; }
        
        EndpointConfigurationType Type { get; set; }
        
        IConfigurationAuthentication Authentication { get; set; }
    }
}