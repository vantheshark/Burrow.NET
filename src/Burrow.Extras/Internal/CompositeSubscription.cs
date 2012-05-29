using System;
using System.Collections.Generic;

namespace Burrow.Extras.Internal
{
    public class CompositeSubscription
    {
        private readonly Dictionary<string, Subscription>  _internalCache = new Dictionary<string, Subscription>();

        //NOTE: To allow mock this class
        protected internal CompositeSubscription()
        {            
        }

        //NOTE: To allow call this method out side this library such as mocking
        protected internal void AddSubscription(Subscription subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }
            _internalCache[subscription.ConsumerTag] = subscription;
        }

        public int Count
        {
            get { return _internalCache.Count; }
        }

        public Subscription GetByConsumerTag(string consumerTag)
        {
            if (_internalCache.ContainsKey(consumerTag))
            {
                return _internalCache[consumerTag];
            }
            return null;
        }

        #region -- http://www.rabbitmq.com/amqp-0-9-1-reference.html#basic.ack.multiple --
        public void Ack(string consumerTag, ulong deliveryTag)
        {
            _internalCache[consumerTag].Ack(deliveryTag);
        }

        public void Ack(string consumerTag, IEnumerable<ulong> deliveryTags)
        {
            _internalCache[consumerTag].Ack(deliveryTags);
        }

        public void AckAllOutstandingMessages(string consumerTag)
        {
            _internalCache[consumerTag].AckAllOutstandingMessages();
        }


        public void Nack(string consumerTag, ulong deliveryTag, bool requeue)
        {
            _internalCache[consumerTag].Nack(deliveryTag, requeue);
        }

        public void Nack(string consumerTag, IEnumerable<ulong> deliveryTags, bool requeue)
        {
            _internalCache[consumerTag].Nack(deliveryTags, requeue);
        }

        public void NackAllOutstandingMessages(string consumerTag, bool requeue)
        {
            _internalCache[consumerTag].NackAllOutstandingMessages(requeue);
        }
        #endregion
    }
}
