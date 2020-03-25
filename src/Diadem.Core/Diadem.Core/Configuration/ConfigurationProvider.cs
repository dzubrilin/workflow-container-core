using System;
using System.Collections.Concurrent;

namespace Diadem.Core.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly ConcurrentDictionary<string, IConfigurationSection> _configurationSections;

        public ConfigurationProvider()
        {
            _configurationSections = new ConcurrentDictionary<string, IConfigurationSection>();
        }

        public TConfigurationSection GetSection<TConfigurationSection>() where TConfigurationSection : IConfigurationSection
        {
            var sectionCode = typeof(TConfigurationSection).FullName;
            if (string.IsNullOrEmpty(sectionCode))
            {
                throw new ApplicationException($"{typeof(TConfigurationSection).FullName} can not be used as a configuration section");
            }

            var configurationSection = _configurationSections.GetOrAdd(sectionCode, key =>
            {
                if (typeof(TConfigurationSection) == typeof(IHostConfigurationSection))
                {
                    return ConfigurationSectionProxy.CreateConfigurationSection<TConfigurationSection>(null);
                }

                var configurationDirectory = GetSection<IHostConfigurationSection>().ConfigurationDirectory;
                return ConfigurationSectionProxy.CreateConfigurationSection<TConfigurationSection>(configurationDirectory);
            });
            return (TConfigurationSection) configurationSection;
        }
    }
}