using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using MassTransit.RabbitMqTransport.Topology.Builders;
using MassTransit.RabbitMqTransport.Topology.Entities;
using Microsoft.Extensions.Primitives;
using RabbitMQ.Client;

namespace Diadem.Workflow.Provider.RabbitMq
{
    public class WorkflowRabbitMqDelayMessagingTopology
    {
        internal const int BitCount = 28;

        private long _nextId;

        public RabbitMqBrokerTopology CreateRabbitMqBrokerTopology()
        {
            var queues = new List<QueueEntity>(BitCount);
            var exchanges = new List<ExchangeEntity>(BitCount + 1);
            var exchangeBindings = new List<ExchangeBindingEntity>(BitCount);
            var queueBindings = new List<QueueBindingEntity>(BitCount);

            var id = GetNextId();
            var prevExchange = CreateDelayedTransitionWorkflowMessage();
            exchanges.Add(prevExchange);

            for (var level = 0; level < BitCount; level++)
            {
                var exchange = CreateExchange(level);
                var exchangeBinding = CreateExchangeBinding(level, exchange, prevExchange);

                var queue = CreateQueue(level);
                var queueBinding = CreateQueueBinding(level, exchange, queue);

                exchanges.Add(exchange);
                exchangeBindings.Add(exchangeBinding);
                queues.Add(queue);
                queueBindings.Add(queueBinding);

                prevExchange = exchange;
            }

            return new RabbitMqBrokerTopology(exchanges, exchangeBindings, queues, queueBindings);
        }

        private long GetNextId()
        {
            return Interlocked.Increment(ref _nextId);
        }

        private ExchangeEntity CreateDelayedTransitionWorkflowMessage()
        {
            var id = GetNextId();
            const string exchangeName = "Diadem.Workflow.Core.Model:IDelayedTransitionWorkflowMessage";
            return new ExchangeEntity(id, exchangeName, ExchangeType.Fanout, true, false, null);
        }

        private ExchangeEntity CreateExchange(int level)
        {
            var id = GetNextId();
            var exchangeName = GetExchangeName(level);
            return new ExchangeEntity(id, exchangeName, ExchangeType.Topic, true, false, null);
        }

        private ExchangeBindingEntity CreateExchangeBinding(int level, ExchangeEntity srcExchange, ExchangeEntity dstExchange)
        {
            var id = GetNextId();
            var routingKey = GetRoutingKey(level, true);
            return new ExchangeBindingEntity(id, srcExchange, dstExchange, routingKey, null);
        }

        private QueueEntity CreateQueue(int level)
        {
            var id = GetNextId();
            var queueName = GetQueueName(level);
            var queueEntity = new QueueEntity(id, queueName, true, false, false, null);

            var lowExchangeName = level == 0 ? "workflow" : GetExchangeName(level - 1);
            queueEntity.QueueArguments.Add("x-dead-letter-exchange", lowExchangeName);

            var ttl = 1000L * (1L << level);
            queueEntity.QueueArguments.Add("x-message-ttl", ttl);

            return queueEntity;
        }

        private QueueBindingEntity CreateQueueBinding(int level, ExchangeEntity srcExchange, QueueEntity dstQueue)
        {
            var id = GetNextId();
            var routingKey = GetRoutingKey(level, false);
            return new QueueBindingEntity(id, srcExchange, dstQueue, routingKey, null);
        }

        private static string GetRoutingKey(int level, bool forExchange)
        {
            var routingKeyBuilder = new StringBuilder(32);
            for (var i = BitCount - 1; i >= level; i--)
            {
                if (i > level)
                {
                    routingKeyBuilder.Append('*');
                }
                else if (i == level)
                {
                    routingKeyBuilder.Append(forExchange ? '0' : '1');
                }

                routingKeyBuilder.Append('.');
            }

            routingKeyBuilder.Append('#');
            return routingKeyBuilder.ToString();
        }

        private static string GetExchangeName(int level)
        {
            return $"workflow.exchange.delay-{level.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}";
        }

        private static string GetQueueName(int level)
        {
            return $"workflow.queue.delay-{level.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}";
        }
    }
}