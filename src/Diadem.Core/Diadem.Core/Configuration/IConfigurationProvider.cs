namespace Diadem.Core.Configuration
{
    public interface IConfigurationProvider
    {
        TConfigurationSection GetSection<TConfigurationSection>() where TConfigurationSection : IConfigurationSection;
    }
}