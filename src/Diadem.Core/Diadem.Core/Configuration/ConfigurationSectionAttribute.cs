using System;

namespace Diadem.Core.Configuration
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    public class ConfigurationSectionAttribute : Attribute
    {
        public ConfigurationSectionAttribute(string prefix)
        {
            Prefix = prefix;
        }

        public string Prefix { get; }
    }
}