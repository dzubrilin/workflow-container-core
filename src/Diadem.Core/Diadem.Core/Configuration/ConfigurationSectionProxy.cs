using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Serilog;

namespace Diadem.Core.Configuration
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConfigurationSectionProxy : DispatchProxy, IConfigurationSection
    {
        private string _configurationDirectory;

        private string _configurationSectionPrefix;

        private Type _configurationSectionType;

        private ConcurrentDictionary<string, string> _configurationValuesMap;

        private Dictionary<string, string> _configurationValuesFromFile;

        internal static IConfigurationSection CreateConfigurationSection<TConfigurationSection>(string configurationDirectory)
            where TConfigurationSection : IConfigurationSection
        {
            object configurationSectionProxy = Create<TConfigurationSection, ConfigurationSectionProxy>();
            ((ConfigurationSectionProxy)configurationSectionProxy).SetConfigurationDirectory(configurationDirectory);
            ((ConfigurationSectionProxy)configurationSectionProxy).SetConfigurationSectionType(typeof(TConfigurationSection));
            return (TConfigurationSection)configurationSectionProxy;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var propertyInfo = GetPropertyInfoFromMethodInfo(targetMethod);
            if (null == propertyInfo)
            {
                throw new ApplicationException($"Configuration Section [{_configurationSectionType.FullName}] does not have configuration [{targetMethod.Name}] defined");
            }

            var configurationKey = $"{_configurationSectionPrefix}_{propertyInfo.Name}".ToUpperInvariant();
            var configurationValue = _configurationValuesMap.GetOrAdd(configurationKey, key => GetConfigurationValue(propertyInfo, key));
            return ConvertValue(configurationValue, propertyInfo.PropertyType);
        }

        private void SetConfigurationDirectory(string configurationDirectory)
        {
            _configurationDirectory = configurationDirectory;
        }

        private void SetConfigurationSectionType(Type configurationSectionType)
        {
            _configurationSectionType = configurationSectionType;
            _configurationValuesMap = new ConcurrentDictionary<string, string>();

            var configurationSectionAttribute = _configurationSectionType.GetCustomAttribute<ConfigurationSectionAttribute>();
            _configurationSectionPrefix = null != configurationSectionAttribute ? configurationSectionAttribute.Prefix : _configurationSectionType.Name;
        }

        private static object ConvertValue(string value, Type type)
        {
            if (typeof(IConvertible).IsAssignableFrom(type))
            {
                return Convert.ChangeType(value, type);
            }

            var typeConverterAttribute = type.GetCustomAttribute<TypeConverterAttribute>();
            if (null != typeConverterAttribute)
            {
                return TypeDescriptor.GetConverter(type).ConvertFromString(value);
            }

            var jsonSerializableConfigurationValueAttribute = type.GetCustomAttribute<JsonSerializableConfigurationValueAttribute>();
            if (null != jsonSerializableConfigurationValueAttribute)
            {
                return JsonConvert.DeserializeObject(value, type);
            }

            if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType)
            {
                var enumerableType = type.GetGenericArguments().First();
                jsonSerializableConfigurationValueAttribute = enumerableType.GetCustomAttribute<JsonSerializableConfigurationValueAttribute>();
                if (null != jsonSerializableConfigurationValueAttribute)
                {
                    return JsonConvert.DeserializeObject(value, type);
                }
            }
            
            throw new ApplicationException($"Type [{type.FullName}] must support either [IConvertible], or [TypeConverter], or marked with [JsonSerializableConfigurationValueAttribute]");
        }

        private string GetConfigurationValue(PropertyInfo propertyInfo, string configurationKey)
        {
            var configurationSource = ConfigurationSource.Any;
            var configurationSourceAttribute = propertyInfo.GetCustomAttribute<ConfigurationSourceAttribute>();
            if (null != configurationSourceAttribute)
            {
                configurationSource = configurationSourceAttribute.ConfigurationSource;
            }

            if ((configurationSource & ConfigurationSource.Environment) == ConfigurationSource.Environment)
            {
                var value = Environment.GetEnvironmentVariable(configurationKey, EnvironmentVariableTarget.Process);
                if (!string.IsNullOrEmpty(value))
                {
                    Log.Verbose("Read configuration {configurationKey} from environment as {value}", configurationKey, value);
                    return value;
                }
            }

            if ((configurationSource & ConfigurationSource.ConfigurationFile) == ConfigurationSource.ConfigurationFile)
            {
                if (null == _configurationValuesFromFile)
                {
                    var configurationValuesFromFile = ReadConfigurationValuesFromFile();
                    Interlocked.CompareExchange(ref _configurationValuesFromFile, configurationValuesFromFile, _configurationValuesFromFile);
                }

                if (_configurationValuesFromFile.TryGetValue(configurationKey, out var value))
                {
                    return value;
                }
            }

            throw new ApplicationException($"Cannot find configuration value for {configurationKey}");
        }

        private PropertyInfo GetPropertyInfoFromMethodInfo(MethodInfo targetMethod)
        {
            if (!targetMethod.IsSpecialName)
            {
                return null;
            }

            return _configurationSectionType.GetProperty(targetMethod.Name.Substring(4), BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.GetProperty);
        }

        private Dictionary<string, string> ReadConfigurationValuesFromFile()
        {
            var result = new Dictionary<string, string>();
            try
            {
                var configurationDirectory = _configurationDirectory;
                if (string.IsNullOrEmpty(configurationDirectory))
                {
                    configurationDirectory = AppContext.BaseDirectory;
                }

                var configurationFileLocation = Path.Combine(configurationDirectory, "configuration.xml");
                if (!File.Exists(configurationFileLocation))
                {
                    return result;
                }

                using (var fileStream = File.Open(configurationFileLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var xmlReader = XmlReader.Create(fileStream))
                {
                    var xDocument = XDocument.Load(xmlReader);
                    if (null == xDocument.Root)
                    {
                        return result;
                    }

                    var configurationSections = xDocument.Root.Elements("configurationSection").ToArray();
                    if (!configurationSections.Any())
                    {
                        return result;
                    }

                    var configurationSection = configurationSections.FirstOrDefault(cs => string.Equals(cs.Attribute("key")?.Value, _configurationSectionPrefix, StringComparison.OrdinalIgnoreCase));
                    if (null == configurationSection)
                    {
                        throw new ApplicationException($"Cannot find configuration section [{_configurationSectionPrefix}] in [configuration.xml] file");
                    }

                    var configurations = configurationSection.Elements("configuration").ToArray();
                    if (!configurations.Any())
                    {
                        return result;
                    }

                    foreach (var configuration in configurations)
                    {
                        var key = configuration.Attribute("key")?.Value;
                        if (string.IsNullOrEmpty(key))
                        {
                            continue;
                        }

                        var configurationKey = $"{_configurationSectionPrefix}_{key}".ToUpperInvariant();
                        var configurationValue = configuration.Attribute("value")?.Value ?? configuration.Value;
                        result.Add(configurationKey, configurationValue);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred during reading configuration file");
                throw;
            }
        }
    }
}