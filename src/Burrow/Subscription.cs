using System;
using System.Collections.Generic;
using System.IO;
using RabbitMQ.Client;
using System.Linq;
using RabbitMQ.Client.Exceptions;

namespace Burrow
{
    /// <summary>
    /// A wrapper hold reference to RabbitMQ.Client <see cref="IModel"/> object. 
    /// <para>From here you can ack/nack a message or cancel the consumer</para>
    /// </summary>
    public class Subscription
    {
        internal const string CloseByApplication = "Closed by application";

        private IModel _channel;
        /// <summary>
        /// The name of the queue
        /// </summary>
        public string QueueName { get; set; }
        /// <summary>
        /// Name of the subscription
        /// </summary>
        public string SubscriptionName { get; set; }
        /// <summary>
        /// Consumer tag
        /// </summary>
        public string ConsumerTag { get; set; }

        internal protected Subscription()
        {            
        }

        // Incase someone want to mock this class
        internal protected Subscription(IModel channel) : this()
        {
            SetChannel(channel);
        }

        /// <summary>
        /// Set the <see cref="IModel"/> (aka channel)
        /// </summary>
        /// <param name="channel"></param>
        public void SetChannel(IModel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }
            _channel = channel;
        }

        #region -- http://www.rabbitmq.com/amqp-0-9-1-reference.html#basic.ack.multiple --
        /// <summary>
        /// Cancel subscription
        /// </summary>
        public void Cancel()
        {
            TryCancel(x => x.BasicCancel(ConsumerTag), _channel, Global.DefaultWatcher);
        }

        /// <summary>
        /// Ack a message by its delivery tag
        /// </summary>
        /// <param name="deliveryTag"></param>
        public void Ack(ulong deliveryTag)
        {
            TryAck(_channel, deliveryTag, false);
        }
        
        /// <summary>
        /// Ack all messages that have delivery tag less than or equal provided delivery tag
        /// </summary>
        /// <param name="deliveryTag"></param>
        public void AckAllUpTo(ulong deliveryTag)
        {
            TryAck(_channel, deliveryTag, true);
        }

        /// <summary>
        /// Ack all messages provided in the list
        /// </summary>
        /// <param name="deliveryTags"></param>
        public void Ack(IEnumerable<ulong> deliveryTags)
        {
            if (deliveryTags == null)
            {
                throw new ArgumentNullException("deliveryTags");
            }
            var tags = deliveryTags.ToList();
            if (tags.Count == 0)
            {
                return;
            }

            lock (BurrowConsumer.OutstandingDeliveryTags)
            {
                var max = tags.Max();
                if (BurrowConsumer.OutstandingDeliveryTags.ContainsKey(_channel) && CanAckNackAll(BurrowConsumer.OutstandingDeliveryTags[_channel], tags, max))
                {
                    TryAck(_channel, max, true);
                }
                else
                {
                    tags.ForEach(t => TryAck(_channel, t, false));
                }
            }
        }

        private bool CanAckNackAll(List<ulong> outstandingList, List<ulong> list, ulong maxTag)
        {
            var minTag = list.Min();
            var intersection = outstandingList.Except(list);

            if (intersection.Any(x => x > minTag && x < maxTag))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Beware of using this method. It acks all unacknowledged messages 
        /// </summary>
        public void AckAllOutstandingMessages()
        {
            TryAck(_channel, 0, true);
        }

        /// <summary>
        /// Nack a messages by it's delivery tag
        /// </summary>
        /// <param name="deliveryTag"></param>
        /// <param name="requeue"></param>
        public void Nack(ulong deliveryTag, bool requeue)
        {
            TryNack(_channel, deliveryTag, false, requeue);
        }

        /// <summary>
        /// Nack all messages that have delivery tags less than or equal provided delivery tag
        /// </summary>
        /// <param name="deliveryTag"></param>
        /// <param name="requeue"></param>
        public void NackAllUpTo(ulong deliveryTag, bool requeue)
        {
            TryNack(_channel, deliveryTag, true, requeue);
        }

        /// <summary>
        /// Nack all messages privided by delivery tags in the list
        /// </summary>
        /// <param name="deliveryTags"></param>
        /// <param name="requeue"></param>
        public void Nack(IEnumerable<ulong> deliveryTags, bool requeue)
        {
            if (deliveryTags == null)
            {
                throw new ArgumentNullException("deliveryTags");
            }
            var tags = deliveryTags.ToList();
            if (tags.Count == 0)
            {
                return;
            }

            lock (BurrowConsumer.OutstandingDeliveryTags)
            {
                var max = tags.Max();
                if (BurrowConsumer.OutstandingDeliveryTags.ContainsKey(_channel) && CanAckNackAll(BurrowConsumer.OutstandingDeliveryTags[_channel], tags, max))
                {
                    TryNack(_channel, max, true, requeue);
                }
                else
                {
                    tags.ForEach(t => TryNack(_channel, t, false, requeue));
                }
            }
        }

        /// <summary>
        /// Beware of using this method. It nacks all unacknowledged messages 
        /// </summary>
        /// <param name="requeue"></param>
        public void NackAllOutstandingMessages(bool requeue)
        {
            TryNack(_channel, 0, true, requeue);
        }

        private const string FailedToAckMessage = "Basic ack/nack failed because chanel was closed with message {0}. Message remains on RabbitMQ and will be retried.";
        internal static void TryAckOrNack(bool ack, IModel channel, ulong deliveryTag, bool multiple, bool requeue, IRabbitWatcher watcher = null)
        {
            try
            {
                lock (BurrowConsumer.OutstandingDeliveryTags)
                {
                    if (channel == null)
                    {
                        (watcher ?? Global.DefaultWatcher).InfoFormat("Trying ack/nack msg but the Channel is null, will not do anything");
                    }
                    else if (!channel.IsOpen)
                    {
                        (watcher ?? Global.DefaultWatcher).InfoFormat("Trying ack/nack msg but the Channel is not open, will not do anything");
                    }
                    else
                    {
                        if (ack)
                        {
                            channel.BasicAck(deliveryTag, multiple);
                        }
                        else
                        {
                            channel.BasicNack(deliveryTag, multiple, requeue);
                        }

                        if (BurrowConsumer.OutstandingDeliveryTags.ContainsKey(channel))
                        {
                            if (deliveryTag == 0)
                            {
                                // Ack/Nack all out standing
                                BurrowConsumer.OutstandingDeliveryTags[channel].Clear();
                            }
                            else if (multiple)
                            {
                                // Ack/Nack all up to
                                BurrowConsumer.OutstandingDeliveryTags[channel].RemoveAll(x => x <= deliveryTag);
                            }
                            else
                            {
                                // Ack/Nack only 1 tag
                                BurrowConsumer.OutstandingDeliveryTags[channel].Remove(deliveryTag);
                            }
                        }
                    }
                }
            }
            catch (AlreadyClosedException alreadyClosedException)
            {
                (watcher ?? Global.DefaultWatcher).WarnFormat(FailedToAckMessage, alreadyClosedException.Message);
            }
            catch (IOException ioException)
            {
                (watcher ?? Global.DefaultWatcher).WarnFormat(FailedToAckMessage, ioException.Message);
            }
        }
        private void TryAck(IModel channel, ulong deliveryTag, bool multiple, IRabbitWatcher watcher = null)
        {
            TryAckOrNack(true, channel, deliveryTag, multiple, false, watcher);
        }
        private void TryNack(IModel channel, ulong deliveryTag, bool multiple, bool requeue, IRabbitWatcher watcher = null)
        {
            TryAckOrNack(false, channel, deliveryTag, multiple, requeue, watcher);
        }

        internal void TryCancel(Action<IModel> action, IModel channel, IRabbitWatcher watcher)
        {
            const string failedMessage = "Action failed because chanel was closed with message {0}. ";
            try
            {
                watcher.InfoFormat("Cancelling subscription {0} on queue {1}", SubscriptionName, QueueName);
                action(channel);
                watcher.InfoFormat("Subscription {0}  on queue {1} cancelled", SubscriptionName, QueueName);
            }
            catch (AlreadyClosedException alreadyClosedException)
            {
                watcher.WarnFormat(failedMessage, alreadyClosedException.Message);
            }
            catch (IOException ioException)
            {
                watcher.WarnFormat(failedMessage, ioException.Message);
            }
        }
        #endregion
    }
}
