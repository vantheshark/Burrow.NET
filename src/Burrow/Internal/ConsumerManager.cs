using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RabbitMQ.Client;

namespace Burrow.Internal
{
    public class ConsumerManager : IConsumerManager, IObserver<ISerializer>
    {
        public virtual IMessageHandlerFactory MessageHandlerFactory { get; private set; }

        protected readonly IRabbitWatcher _watcher;
        protected ISerializer _serializer;
        protected readonly List<IBasicConsumer> _createdConsumers;
        internal readonly Action<ISerializer> UpdateSerializzer;

        public ConsumerManager(IRabbitWatcher watcher, 
                               IMessageHandlerFactory messageHandlerFactory,
                               ISerializer serializer)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            if (messageHandlerFactory == null)
            {
                throw new ArgumentNullException("messageHandlerFactory");
            }
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            _watcher = watcher;
            MessageHandlerFactory = messageHandlerFactory;
            _serializer = serializer;
            _createdConsumers = new List<IBasicConsumer>();
            UpdateSerializzer = s => _serializer = s;
        }

        public virtual IBasicConsumer CreateConsumer<T>(IModel channel, string subscriptionName, Action<T> onReceiveMessage, ushort? consumerThreadCount)
        {
            var messageHandler = MessageHandlerFactory.Create<T>(subscriptionName, (msg, evt) => onReceiveMessage(msg));
            var consumer = new BurrowConsumer(channel, messageHandler, _watcher, true, (consumerThreadCount > 0 ? consumerThreadCount.Value : Global.DefaultConsumerBatchSize));
            _createdConsumers.Add(consumer);
            return consumer;
        }

        public virtual IBasicConsumer CreateAsyncConsumer<T>(IModel channel, string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage, ushort? consumerThreadCount)
        {
            var messageHandler = MessageHandlerFactory.Create(subscriptionName, onReceiveMessage);
            var consumer = new BurrowConsumer(channel, messageHandler, _watcher, false, (consumerThreadCount > 0 ? consumerThreadCount.Value : Global.DefaultConsumerBatchSize));
            _createdConsumers.Add(consumer);
            return consumer;
        }

        public void ClearConsumers()
        {
            _watcher.DebugFormat("Clearing consumer subscriptions");
            _createdConsumers.OfType<IDisposable>().ToList().ForEach(c => c.Dispose());
            _createdConsumers.Clear();
        }

        private bool _disposed;
        public void Dispose()
        {
            if (!_disposed)
            {
                ClearConsumers();
                MessageHandlerFactory.Dispose();
            }
            _disposed = true;
        }

        [ExcludeFromCodeCoverage]
        public void OnNext(ISerializer value)
        {
            _serializer = value;
        }

        [ExcludeFromCodeCoverage]
        public void OnError(Exception error)
        {
        }

        [ExcludeFromCodeCoverage]
        public void OnCompleted()
        {
        }
    }
}
