namespace Diadem.Core.Configuration
{
    [ConfigurationSection("SMTP")]
    public interface ISmtpConfigurationSection : IConfigurationSection
    {
        string Host { get; }

        int Port { get; }

        bool EnableSsl { get; }

        string UserName { get; }

        string Password { get; }
    }
}