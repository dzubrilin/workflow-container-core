namespace Diadem.Core.Configuration
{
    [ConfigurationSection("HOST")]
    public interface IHostConfigurationSection : IConfigurationSection
    {
        [ConfigurationSource(ConfigurationSource.Any)]
        string ConfigurationDirectory { get; }
    }
}