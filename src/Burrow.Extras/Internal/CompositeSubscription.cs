using System;
using System.Collections.Generic;
using System.Linq;

namespace Burrow.Extras.Internal
{
    /// <summary>
    /// A composite subscription that contains others child Subscription object
    /// <para>This is a result of subscribe to priority queues without auto ack, use the instance of this class to ack messages later</para>
    /// </summary>
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

        /// <summary>
        /// Get the ammount of subscriptions to the priority queues
        /// </summary>
        public int Count
        {
            get { return _internalCache.Count; }
        }

        /// <summary>
        /// Get the <see cref="Subscription"/> by consumer tag
        /// </summary>
        /// <param name="consumerTag"></param>
        /// <returns></returns>
        public Subscription GetByConsumerTag(string consumerTag)
        {
            if (_internalCache.ContainsKey(consumerTag))
            {
                return _internalCache[consumerTag];
            }
            return null;
        }

        #region -- http://www.rabbitmq.com/amqp-0-9-1-reference.html#basic.ack.multiple --
        /// <summary>
        /// Ack a message by its delivery tag of the consumer whose tag is consumerTag
        /// </summary>
        /// <param name="consumerTag"></param>
        /// <param name="deliveryTag"></param>
        public void Ack(string consumerTag, ulong deliveryTag)
        {
            TryAckOrNAck(consumerTag,  x => x.Ack(deliveryTag));
        }

        /// <summary>
        /// Ack all messages by delivery tags in the list of the consumer whose tag is consumerTag
        /// </summary>
        /// <param name="consumerTag"></param>
        /// <param name="deliveryTags"></param>
        public void Ack(string consumerTag, IEnumerable<ulong> deliveryTags)
        {
            TryAckOrNAck(consumerTag, x => x.Ack(deliveryTags));
        }

        /// <summary>
        /// Ack all messages that have delivery tag less than or equal provided delivery tag
        /// </summary>
        /// <param name="consumerTag"></param>
        /// <param name="deliveryTag"></param>
        public void AckAllUpTo(string consumerTag, ulong deliveryTag)
        {
            TryAckOrNAck(consumerTag, x => x.AckAllUpTo(deliveryTag));
        }

        /// <summary>
        /// Beware of using this method. It acks all unacknowledged messages of the consumer by consumerTag 
        /// </summary>
        public void AckAllOutstandingMessages(string consumerTag)
        {
            TryAckOrNAck(consumerTag, x => x.AckAllOutstandingMessages());
        }

        /// <summary>
        /// NAck a message by its delivery tag of the consumer whose tag is consumerTag
        /// </summary>
        /// <param name="consumerTag"></param>
        /// <param name="deliveryTag"></param>
        /// <param name="requeue"> </param>
        public void Nack(string consumerTag, ulong deliveryTag, bool requeue)
        {
            TryAckOrNAck(consumerTag, x => x.Nack(deliveryTag, requeue));
        }

        /// <summary>
        /// NAck all messages by delivery tags in the list of the consumer whose tag is consumerTag
        /// </summary>
        /// <param name="consumerTag"></param>
        /// <param name="deliveryTags"></param>
        /// <param name="requeue"> </param>
        public void Nack(string consumerTag, IEnumerable<ulong> deliveryTags, bool requeue)
        {
            TryAckOrNAck(consumerTag, x => x.Nack(deliveryTags, requeue));
        }

        /// <summary>
        /// Nack all messages that have delivery tag less than or equal provided delivery tag
        /// </summary>
        /// <param name="consumerTag"></param>
        /// <param name="deliveryTag"></param>
        /// <param name="requeue"> </param>
        public void NackAllUpTo(string consumerTag, ulong deliveryTag, bool requeue)
        {
            TryAckOrNAck(consumerTag, x => x.NackAllUpTo(deliveryTag, requeue));
        }

        /// <summary>
        /// Beware of using this method. It nacks all unacknowledged messages of the consumer by consumerTag 
        /// </summary>
        public void NackAllOutstandingMessages(string consumerTag, bool requeue)
        {
            TryAckOrNAck(consumerTag, x => x.NackAllOutstandingMessages(requeue));
        }

        /// <summary>
        /// Cancel all consumers on all priority queues
        /// </summary>
        public void CancelAll()
        {
            if (_internalCache != null && _internalCache.Values.Count > 0)
            {
                _internalCache.Values.ToList().ForEach(c => c.Cancel());
            }
        }

        private void TryAckOrNAck(string consumerTag, Action<Subscription> action)
        {
            var sub = GetByConsumerTag(consumerTag);
            if (sub == null)
            {
                throw new SubscriptionNotFoundException(consumerTag, string.Format("Subscription {0} not found, this problem could happen after a retry for new connection. You properly just ignore the old objects you're trying to ack/nack", consumerTag));
            }
            action(sub);
        }

        #endregion
    }
}
