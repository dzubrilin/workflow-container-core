using System.Collections.Generic;

namespace Diadem.Core.Configuration
{
    public interface IConfigurationAuthentication
    {
        ConfigurationAuthenticationType Type { get; set; }

        IList<KeyValuePair<string, string>> Parameters { get; set; }
    }
}