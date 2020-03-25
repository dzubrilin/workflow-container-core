using System;
using System.Threading;
using Autofac;
using Diadem.Core.Configuration;
using Diadem.Messaging.Core;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Provider.RabbitMq;
using GreenPipes;
using MassTransit;

namespace Diadem.Workflow.Host.RabbitMq
{
    public class RabbitMqBusControlFactory : IBusControlFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        private readonly Lazy<IBusControl> _busControlLazy;

        public RabbitMqBusControlFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
            _busControlLazy = new Lazy<IBusControl>(CreateBusControlInternal, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public IBusControl CreateBusControl()
        {
            return _busControlLazy.Value;
        }

        private IBusControl CreateBusControlInternal()
        {
            var configurationProvider = _lifetimeScope.Resolve<IConfigurationProvider>();
            var rabbitMqConfigurationSection = configurationProvider.GetSection<IRabbitMqConfigurationSection>();
            var busControl = Bus.Factory.CreateUsingRabbitMq(rabbitMqBusFactoryConfigurator =>
            {
                // rabbitMqBusFactoryConfigurator.UseDelayedExchangeMessageScheduler();
                rabbitMqBusFactoryConfigurator.Send<DelayedTransitionWorkflowMessage>(rabbitMqMessageSendTopologyConfigurator =>
                {
                    rabbitMqMessageSendTopologyConfigurator.UseRoutingKeyFormatter(context => context.Message.Delay.ToRabbitMqRoutingKey("workflow"));

                    // multiple conventions can be set, in this case also CorrelationId
                    // x.UseCorrelationId(context => context.Message.TransactionId);
                });

                var host = rabbitMqBusFactoryConfigurator.Host(new Uri(rabbitMqConfigurationSection.Address), h =>
                {
                    h.Username(rabbitMqConfigurationSection.UserName);
                    h.Password(rabbitMqConfigurationSection.Password);
                });

                rabbitMqBusFactoryConfigurator.UseConcurrencyLimit(10);
                rabbitMqBusFactoryConfigurator.ReceiveEndpoint(
                    host,
                    rabbitMqConfigurationSection.ReceiveQueueName,
                    rabbitMqReceiveEndpointConfigurator =>
                    {
                        rabbitMqReceiveEndpointConfigurator.Consumer(_lifetimeScope.Resolve<IConsumer<IAsynchronousTransitionWorkflowMessage>>);
                        rabbitMqReceiveEndpointConfigurator.Consumer(_lifetimeScope.Resolve<IConsumer<IDelayedTransitionWorkflowMessage>>);
                        rabbitMqReceiveEndpointConfigurator.Consumer(_lifetimeScope.Resolve<IConsumer<IEventRequestWorkflowMessage>>);
                    });
            });

            return busControl;
        }
    }
}