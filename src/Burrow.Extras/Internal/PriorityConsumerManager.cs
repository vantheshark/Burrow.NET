using System;
using Burrow.Internal;
using RabbitMQ.Client;

namespace Burrow.Extras.Internal
{
    internal class PriorityConsumerManager : ConsumerManager
    {
        public PriorityConsumerManager(IRabbitWatcher watcher,
                                       IMessageHandlerFactory messageHandlerFactory,
                                       ISerializer serializer)
            : base(watcher, messageHandlerFactory, serializer)
        {
        }

        public override IBasicConsumer CreateConsumer<T>(IModel channel, string subscriptionName, Action<T> onReceiveMessage, ushort? consumerThreadCount)
        {
            var messageHandler = MessageHandlerFactory.Create<T>(subscriptionName, (msg, evt) => onReceiveMessage(msg));
            var consumer = new PriorityBurrowConsumer(channel, messageHandler, _watcher, true, (consumerThreadCount > 0 ? consumerThreadCount.Value : Global.DefaultConsumerBatchSize));
            _createdConsumers.Add(consumer);
            return consumer;
        }

        public override IBasicConsumer CreateAsyncConsumer<T>(IModel channel, string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage, ushort? consumerThreadCount)
        {
            var messageHandler = MessageHandlerFactory.Create(subscriptionName, onReceiveMessage);
            var consumer = new PriorityBurrowConsumer(channel, messageHandler, _watcher, false, (consumerThreadCount > 0 ? consumerThreadCount.Value : Global.DefaultConsumerBatchSize));
            _createdConsumers.Add(consumer);
            return consumer;
        }
    }
}
