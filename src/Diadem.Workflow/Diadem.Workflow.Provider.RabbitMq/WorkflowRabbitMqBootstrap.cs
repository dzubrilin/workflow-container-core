using System;
using System.Collections.Generic;
using System.Linq;
using Diadem.Core.Configuration;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;

namespace Diadem.Workflow.Provider.RabbitMq
{
    public class WorkflowRabbitMqBootstrap
    {
        private readonly IConfigurationProvider _configurationProvider;

        public WorkflowRabbitMqBootstrap(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public void Configure()
        {
            var rabbitMqHostSettings = GetRabbitMqHostSettings();
            var connectionFactory = rabbitMqHostSettings.GetConnectionFactory();
            var connection = CreateConnection(rabbitMqHostSettings, connectionFactory);
            var model = connection.CreateModel();

            var workflowRabbitMqDelayMessagingTopology = new WorkflowRabbitMqDelayMessagingTopology();
            var rabbitMqBrokerTopology = workflowRabbitMqDelayMessagingTopology.CreateRabbitMqBrokerTopology();

            foreach (var exchange in rabbitMqBrokerTopology.Exchanges)
            {
                ExchangeDelete(model, exchange.ExchangeName);
                ExchangeDeclare(model, exchange.ExchangeName, exchange.ExchangeType, exchange.Durable, exchange.AutoDelete, exchange.ExchangeArguments);
            }

            foreach (var exchangeBinding in rabbitMqBrokerTopology.ExchangeBindings)
            {
                ExchangeBind(model, exchangeBinding.Destination.ExchangeName, exchangeBinding.Source.ExchangeName, exchangeBinding.RoutingKey, exchangeBinding.Arguments);
            }

            foreach (var queue in rabbitMqBrokerTopology.Queues)
            {
                QueueDelete(model, queue.QueueName);
                QueueDeclare(model, queue.QueueName, queue.Durable, queue.Exclusive, queue.AutoDelete, queue.QueueArguments);
            }

            foreach (var queueBinding in rabbitMqBrokerTopology.QueueBindings)
            {
                QueueBind(model, queueBinding.Destination.QueueName, queueBinding.Source.ExchangeName, queueBinding.RoutingKey, queueBinding.Arguments);
            }
        }

        private static void ExchangeBind(IModel model, string destination, string source, string routingKey, IDictionary<string, object> arguments)
        {
            model.ExchangeBind(destination, source, routingKey, arguments);
        }

        private static void ExchangeDeclare(IModel model, string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments)
        {
            model.ExchangeDeclare(exchange, type, durable, autoDelete, arguments);
        }

        private static void ExchangeDelete(IModel model, string exchange)
        {
            model.ExchangeDelete(exchange);
        }

        private static void QueueBind(IModel model, string queue, string exchange, string routingKey, IDictionary<string, object> arguments)
        {
            model.QueueBind(queue, exchange, routingKey, arguments);
        }

        private static void QueueDeclare(IModel model, string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments)
        {
            model.QueueDeclare(queue, durable, exclusive, autoDelete, arguments);
        }

        private static void QueueDelete(IModel model, string queue)
        {
            model.QueueDelete(queue);
        }

        private static IConnection CreateConnection(RabbitMqHostSettings rabbitMqHostSettings, ConnectionFactory connectionFactory)
        {
            IConnection connection;
            if (rabbitMqHostSettings.ClusterMembers?.Any() ?? false)
            {
                connection = connectionFactory.CreateConnection(rabbitMqHostSettings.ClusterMembers, rabbitMqHostSettings.ClientProvidedName);
            }
            else
            {
                var hostNames = Enumerable.Repeat(rabbitMqHostSettings.Host, 1).ToList();
                connection = connectionFactory.CreateConnection(hostNames, rabbitMqHostSettings.ClientProvidedName);
            }

            return connection;
        }

        private RabbitMqHostSettings GetRabbitMqHostSettings()
        {
            var rabbitMqHostSettings = new RabbitMqHostSettingsImpl();
            var rabbitMqConfigurationSection = _configurationProvider.GetSection<IRabbitMqConfigurationSection>();

            rabbitMqHostSettings.Host = new Uri(rabbitMqConfigurationSection.Address).Host;
            rabbitMqHostSettings.Port = rabbitMqConfigurationSection.Port;
            rabbitMqHostSettings.Username = rabbitMqConfigurationSection.UserName;
            rabbitMqHostSettings.Password = rabbitMqConfigurationSection.Password;

            return rabbitMqHostSettings;
        }
    }
}