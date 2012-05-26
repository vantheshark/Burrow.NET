using System;
using Burrow.Internal;
using RabbitMQ.Client;

namespace Burrow.Extras.Internal
{
    public class PriorityConsumerManager : ConsumerManager
    {
        public PriorityConsumerManager(IRabbitWatcher watcher,
                                       IMessageHandlerFactory messageHandlerFactory,
                                       ISerializer serializer, int batchSize)
            : base(watcher, messageHandlerFactory, serializer, batchSize)
        {
        }

        public override IBasicConsumer CreateConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T> onReceiveMessage)
        {
            var action = CreateJobFactory(onReceiveMessage);
            var messageHandler = _messageHandlerFactory.Create(action);
            var consumer = new PriorityBurrowConsumer(null /*Will be set later*/, 0, channel, messageHandler, _watcher, consumerTag, true, 1);
            _createdConsumers.Add(consumer);
            return consumer;
        }

        public override IBasicConsumer CreateConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T, MessageDeliverEventArgs> onReceiveMessage)
        {
            var action = CreateJobFactory(subscriptionName, onReceiveMessage);
            var messageHandler = _messageHandlerFactory.Create(action);
            var consumer = new PriorityBurrowConsumer(null /*Will be set later*/, 0, channel, messageHandler, _watcher, consumerTag, false, 1);
            _createdConsumers.Add(consumer);
            return consumer;
        }

        public override IBasicConsumer CreateAsyncConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T> onReceiveMessage)
        {
            var action = CreateJobFactory(onReceiveMessage);
            var messageHandler = _messageHandlerFactory.Create(action);
            var consumer = new PriorityBurrowConsumer(null /*Will be set later*/, 0, channel, messageHandler, _watcher, consumerTag, true, BatchSize);
            _createdConsumers.Add(consumer);
            return consumer;
        }

        public override IBasicConsumer CreateAsyncConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T, MessageDeliverEventArgs> onReceiveMessage)
        {
            var action = CreateJobFactory(subscriptionName, onReceiveMessage);
            var messageHandler = _messageHandlerFactory.Create(action);
            var consumer = new PriorityBurrowConsumer(null /*Will be set later*/, 0, channel, messageHandler, _watcher, consumerTag, false, BatchSize);
            return consumer;
        }
    }
}
