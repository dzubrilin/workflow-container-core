using System;

namespace Diadem.Core.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigurationSourceAttribute : Attribute
    {
        public ConfigurationSourceAttribute(ConfigurationSource configurationSource)
        {
            ConfigurationSource = configurationSource;
        }

        public ConfigurationSource ConfigurationSource { get; }
    }
}