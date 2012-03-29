using System;
using RabbitMQ.Client;

namespace Burrow
{
    public class Subscription
    {
        private IModel _channel;
        public string SubscriptionName { get; internal set; }
        public string QueueName { get; internal set; }
        public string ConsumerTag { get; internal set; }

        internal Subscription()
        {
        }

        internal Subscription(IModel channel) : this()
        {
            SetChannel(channel);
        }

        internal void SetChannel(IModel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }
            _channel = channel;
        }

        public void Ack(ulong deliveryTag)
        {
            if (_channel.IsOpen)
            {
                try
                {
                    _channel.BasicAck(deliveryTag, false);
                }
                finally 
                {
                }
            }
        }

        public void Nack(ulong deliveryTag, bool requeue)
        {
            if (_channel.IsOpen)
            {
                try
                {
                    _channel.BasicNack(deliveryTag, false, requeue);
                }
                finally
                {
                }
            }
        }
    }
}
