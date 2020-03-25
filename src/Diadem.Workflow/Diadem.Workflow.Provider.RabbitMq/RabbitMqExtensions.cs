using System;
using System.Collections;
using System.Globalization;
using System.Text;

namespace Diadem.Workflow.Provider.RabbitMq
{
    public static class RabbitMqExtensions
    {
        /// <summary>
        /// Converts TimeSpan to a routing key as described here (https://docs.particular.net/transports/rabbitmq/delayed-delivery)
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="targetExchange"></param>
        /// <returns></returns>
        public static string ToRabbitMqRoutingKey(this TimeSpan timeSpan, string targetExchange)
        {
            var totalSeconds = (int)timeSpan.TotalSeconds;

            var index = 0;
            var tmp = totalSeconds;
            var bitArray = new BitArray(WorkflowRabbitMqDelayMessagingTopology.BitCount);
            for (var i = 0; i < 31; i++)
            {
                var bit = (tmp & 1) == 1;
                if (bit)
                {
                    if (i >= WorkflowRabbitMqDelayMessagingTopology.BitCount)
                    {
                        throw new NotSupportedException($"Range {timeSpan:G} is out of supported maximum duration");
                    }

                    bitArray.Set(index, true);
                }

                index += 1;
                tmp = tmp >> 1;
            }

            var stringBuilder = new StringBuilder(50);
            for (var i = WorkflowRabbitMqDelayMessagingTopology.BitCount - 1; i >= 0; i--)
            {
                stringBuilder.Append(bitArray.Get(i) ? '1' : '0');
                stringBuilder.Append('.');
            }

            stringBuilder.Append(targetExchange);
            return stringBuilder.ToString();
        }

        public static string ToRabbitMqDelayedExchangeName(this TimeSpan timeSpan, string exchangeBaseName)
        {
            var totalSeconds = (int)timeSpan.TotalSeconds;

            var max = 0;
            var tmp = totalSeconds;
            for (var i = 0; i < WorkflowRabbitMqDelayMessagingTopology.BitCount; i++)
            {
                var bit = (tmp & 1) == 1;
                if (bit)
                {
                    max = i;
                }

                tmp = tmp >> 1;
            }

            return exchangeBaseName + max.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
        }
    }
}