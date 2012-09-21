using System;
using System.Collections.Generic;
using System.IO;
using RabbitMQ.Client;
using System.Linq;
using RabbitMQ.Client.Exceptions;

namespace Burrow
{
    public class Subscription
    {
        public const string CloseByApplication = "Closed by application";

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
        /// <summary>
        /// Ack a message by its delivery tag
        /// </summary>
        /// <param name="deliveryTag"></param>
        public void Ack(ulong deliveryTag)
        {
            TryAckOrNAck(x => x.BasicAck(deliveryTag, false), _channel, Global.DefaultWatcher);
        }
        
        /// <summary>
        /// Ack all messages that have delivery tag less than or equal provided delivery tag
        /// </summary>
        /// <param name="deliveryTag"></param>
        public void AckAllUpTo(ulong deliveryTag)
        {
            TryAckOrNAck(x => x.BasicAck(deliveryTag, true), _channel, Global.DefaultWatcher);
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
            deliveryTags.ToList().ForEach(tag => TryAckOrNAck(x => x.BasicAck(tag, false), _channel, Global.DefaultWatcher));
        }

        /// <summary>
        /// Beware of using this method. It acks all unacknowledged messages 
        /// </summary>
        public void AckAllOutstandingMessages()
        {
            TryAckOrNAck(x => x.BasicAck(0, true), _channel, Global.DefaultWatcher);
        }

        /// <summary>
        /// Nack a messages by it's delivery tag
        /// </summary>
        /// <param name="deliveryTag"></param>
        /// <param name="requeue"></param>
        public void Nack(ulong deliveryTag, bool requeue)
        {
            TryAckOrNAck(x => x.BasicNack(deliveryTag, false, requeue), _channel, Global.DefaultWatcher);
        }

        /// <summary>
        /// Nack all messages that have delivery tags less than or equal provided delivery tag
        /// </summary>
        /// <param name="deliveryTag"></param>
        /// <param name="requeue"></param>
        public void NackAllUpTo(ulong deliveryTag, bool requeue)
        {
            TryAckOrNAck(x => x.BasicNack(deliveryTag, true, requeue), _channel, Global.DefaultWatcher);
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
            deliveryTags.ToList().ForEach(tag => TryAckOrNAck(x => x.BasicNack(tag, false, requeue), _channel, Global.DefaultWatcher));
        }

        /// <summary>
        /// Beware of using this method. It nacks all unacknowledged messages 
        /// </summary>
        /// <param name="requeue"></param>
        public void NackAllOutstandingMessages(bool requeue)
        {
            TryAckOrNAck(x => x.BasicNack(0, true, requeue), _channel, Global.DefaultWatcher);
        }

        internal static void TryAckOrNAck(Action<IModel> action, IModel channel, IRabbitWatcher watcher)
        {
            const string failedToAckMessage = "Basic ack/nack failed because chanel was closed with message {0}. " +
                                              "Message remains on RabbitMQ and will be retried.";
            try
            {
                if (channel == null)
                {
                    watcher.InfoFormat("Trying ack/nack msg buth the Channel is null, will not do anything");
                }
                else if (!channel.IsOpen)
                {
                    watcher.InfoFormat("Trying ack/nack msg buth the Channel is not open, will not do anything");
                }
                else
                {
                    action(channel);    
                }
            }
            catch (AlreadyClosedException alreadyClosedException)
            {
                watcher.WarnFormat(failedToAckMessage, alreadyClosedException.Message);
            }
            catch (IOException ioException)
            {
                watcher.WarnFormat(failedToAckMessage, ioException.Message);
            }
        }
        #endregion
    }
}
