namespace Diadem.Core.Configuration
{
    [ConfigurationSection("RABBITMQ")]
    public interface IRabbitMqConfigurationSection : IConfigurationSection
    {
        [ConfigurationSource(ConfigurationSource.Any)]
        string Address { get; }

        [ConfigurationSource(ConfigurationSource.Any)]
        int Port { get; }

        [ConfigurationSource(ConfigurationSource.Any)]
        string UserName { get; }

        [ConfigurationSource(ConfigurationSource.Any)]
        string ReceiveQueueName { get; }

        [ConfigurationSource(ConfigurationSource.Any)]
        string Password { get; }
    }
}