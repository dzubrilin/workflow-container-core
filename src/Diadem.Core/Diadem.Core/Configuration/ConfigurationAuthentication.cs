using System;
using System.Collections.Generic;

namespace Diadem.Core.Configuration
{
    public class ConfigurationAuthentication : IConfigurationAuthentication
    {
        public static readonly ConfigurationAuthentication None = new ReadOnlyConfigurationAuthentication();

        public ConfigurationAuthentication()
        {
        }

        public ConfigurationAuthentication(ConfigurationAuthenticationType type, IList<KeyValuePair<string, string>> parameters)
        {
            Type = type;
            Parameters = parameters;
        }

        public virtual ConfigurationAuthenticationType Type { get; set; }
        
        public virtual IList<KeyValuePair<string, string>> Parameters { get; set; }

        public sealed class ReadOnlyConfigurationAuthentication : ConfigurationAuthentication
        {
            public override ConfigurationAuthenticationType Type
            {
                get => ConfigurationAuthenticationType.None;
                set => throw new NotSupportedException();
            }

            public override IList<KeyValuePair<string, string>> Parameters
            {
                get => null;
                set => throw new NotSupportedException();
            }
        }
    }
}