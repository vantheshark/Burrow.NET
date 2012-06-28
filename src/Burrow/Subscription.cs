using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using System.Linq;

namespace Burrow
{
    public class Subscription
    {
        private IModel _channel;
        public string QueueName { get; set; }
        public string SubscriptionName { get; set; }
        public string ConsumerTag { get; set; }

        internal protected Subscription()
        {            
        }

        // Incase someone want to mock this class
        internal protected Subscription(IModel channel) : this()
        {
            SetChannel(channel);
        }

        public void SetChannel(IModel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }
            _channel = channel;
        }

        #region -- http://www.rabbitmq.com/amqp-0-9-1-reference.html#basic.ack.multiple --
        public void Ack(ulong deliveryTag)
        {
            _channel.BasicAck(deliveryTag, false);
        }

        public void Ack(IEnumerable<ulong> deliveryTags)
        {
            _channel.BasicAck(deliveryTags.Max(), true);
        }

        public void AckAllOutstandingMessages()
        {
            _channel.BasicAck(0, true);
        }
        

        public void Nack(ulong deliveryTag, bool requeue)
        {
            _channel.BasicNack(deliveryTag, false, requeue);
        }

        public void Nack(IEnumerable<ulong> deliveryTags, bool requeue)
        {
            _channel.BasicNack(deliveryTags.Max(), true, requeue);
        }

        public void NackAllOutstandingMessages(bool requeue)
        {
            _channel.BasicNack(0, true, requeue);
        }
        #endregion
    }
}
