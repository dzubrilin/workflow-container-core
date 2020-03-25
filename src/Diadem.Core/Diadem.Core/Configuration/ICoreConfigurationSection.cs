namespace Diadem.Core.Configuration
{
    [ConfigurationSection("CORE")]
    public interface ICoreConfigurationSection : IConfigurationSection
    {
        string AssetsDirectory { get; }

        string TempDirectory { get; }
        
        string ApplicationCode { get; }
        
        string ApplicationAesEncryptionKey { get; set; }
    }
}