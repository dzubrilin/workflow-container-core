using System;
using Diadem.Core.Configuration;
using Diadem.Messaging.Core;
using Diadem.Workflow.Provider.RabbitMq;
using GreenPipes;
using MassTransit;

namespace Diadem.Workflow.Demo.Console.RabbitMq
{
    public class RabbitMqBusControlFactory : IBusControlFactory
    {
        private readonly IConfigurationProvider _configurationProvider;

        private IBusControl _busControl;

        public RabbitMqBusControlFactory(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public IBusControl CreateBusControl()
        {
            if (null != _busControl)
            {
                return _busControl;
            }

            var rabbitMqConfigurationSection = _configurationProvider.GetSection<IRabbitMqConfigurationSection>();
            _busControl = Bus.Factory.CreateUsingRabbitMq(rabbitMqBusFactoryConfigurator =>
            {
                var host = rabbitMqBusFactoryConfigurator.Host(new Uri(rabbitMqConfigurationSection.Address), h =>
                {
                    h.Username(rabbitMqConfigurationSection.UserName);
                    h.Password(rabbitMqConfigurationSection.Password);
                });

//                rabbitMqBusFactoryConfigurator.ReceiveEndpoint(host, "Diadem.Workflow.Core.Model:IEventRequestWorkflowMessage", configurator => { });
                rabbitMqBusFactoryConfigurator.UseConcurrencyLimit(5);
                rabbitMqBusFactoryConfigurator.ReceiveEndpoint(
                    host,
                    rabbitMqConfigurationSection.ReceiveQueueName,
                    rabbitMqReceiveEndpointConfigurator =>
                    {
//                        rabbitMqReceiveEndpointConfigurator.Consumer(_lifetimeScope.Resolve<IConsumer<IAsynchronousTransitionWorkflowMessage>>);
//                        rabbitMqReceiveEndpointConfigurator.Consumer(_lifetimeScope.Resolve<IConsumer<IDelayedTransitionWorkflowMessage>>);
//                        rabbitMqReceiveEndpointConfigurator.Consumer(_lifetimeScope.Resolve<IConsumer<IEventRequestWorkflowMessage>>);
                    });
            });

            return _busControl;
        }
    }
}