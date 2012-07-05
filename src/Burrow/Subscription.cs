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
            TryAckOrNAck(x => x.BasicAck(deliveryTag, false), _channel, Global.DefaultWatcher);
        }

        public void Ack(IEnumerable<ulong> deliveryTags)
        {
            TryAckOrNAck(x => x.BasicAck(deliveryTags.Max(), true), _channel, Global.DefaultWatcher);
        }

        public void AckAllOutstandingMessages()
        {
            TryAckOrNAck(x => x.BasicAck(0, true), _channel, Global.DefaultWatcher);
        }
        

        public void Nack(ulong deliveryTag, bool requeue)
        {
            TryAckOrNAck(x => x.BasicNack(deliveryTag, false, requeue), _channel, Global.DefaultWatcher);
        }

        public void Nack(IEnumerable<ulong> deliveryTags, bool requeue)
        {
            TryAckOrNAck(x => x.BasicNack(deliveryTags.Max(), true, requeue), _channel, Global.DefaultWatcher);
        }

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
