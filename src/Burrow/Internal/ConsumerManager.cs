using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Burrow.Internal
{
    public class ConsumerManager : IConsumerManager
    {
        protected readonly IRabbitWatcher _watcher;
        protected readonly IMessageHandlerFactory _messageHandlerFactory;
        protected readonly ISerializer _serializer;
        protected readonly List<BurrowConsumer> _createdConsumers;

        public ConsumerManager(IRabbitWatcher watcher, 
                               IMessageHandlerFactory messageHandlerFactory,
                               ISerializer serializer, int batchSize)
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
            if (batchSize < 1)
            {
                throw new ArgumentException("batchSize must be greater than or equal 1", "batchSize");
            }
            
            BatchSize = batchSize;

            _watcher = watcher;
            _messageHandlerFactory = messageHandlerFactory;
            _serializer = serializer;
            _createdConsumers = new List<BurrowConsumer>();
        }

        public virtual IBasicConsumer CreateConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T> onReceiveMessage)
        {
            var action = CreateJobFactory(onReceiveMessage);
            var messageHandler = _messageHandlerFactory.Create(action);
            var consumer = new BurrowConsumer(channel, messageHandler, _watcher, consumerTag, true, 1);
            _createdConsumers.Add(consumer);
            return consumer;
        }

        public virtual IBasicConsumer CreateConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T, MessageDeliverEventArgs> onReceiveMessage)
        {
            var action = CreateJobFactory(subscriptionName, onReceiveMessage);
            var messageHandler = _messageHandlerFactory.Create(action);
            var consumer = new BurrowConsumer(channel, messageHandler, _watcher, consumerTag, false, 1);
            _createdConsumers.Add(consumer);
            return consumer;
        }

        public virtual IBasicConsumer CreateAsyncConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T> onReceiveMessage)
        {
            var action = CreateJobFactory(onReceiveMessage);
            var messageHandler = _messageHandlerFactory.Create(action);
            var consumer = new BurrowConsumer(channel, messageHandler, _watcher, consumerTag, true, BatchSize);
            _createdConsumers.Add(consumer);
            return consumer;
        }

        public virtual IBasicConsumer CreateAsyncConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T, MessageDeliverEventArgs> onReceiveMessage)
        {
            var action = CreateJobFactory(subscriptionName, onReceiveMessage);
            var messageHandler = _messageHandlerFactory.Create(action);
            var consumer = new BurrowConsumer(channel, messageHandler, _watcher, consumerTag, false, BatchSize);
            return consumer;
        }

        protected Func<BasicDeliverEventArgs, Task> CreateJobFactory<T>(Action<T> onReceiveMessage)
        {
            return eventArgs => Task.Factory.StartNew(() =>
            {
                CheckMessageType<T>(eventArgs.BasicProperties);
                var message = _serializer.Deserialize<T>(eventArgs.Body);
                onReceiveMessage(message);
            });
        }

        protected virtual Func<BasicDeliverEventArgs, Task> CreateJobFactory<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage)
        {
            return eventArgs => Task.Factory.StartNew(() =>
            {
                CheckMessageType<T>(eventArgs.BasicProperties);
                var message = _serializer.Deserialize<T>(eventArgs.Body);
                onReceiveMessage(message, new MessageDeliverEventArgs
                {
                    ConsumerTag = eventArgs.ConsumerTag,
                    DeliveryTag = eventArgs.DeliveryTag,
                    SubscriptionName = subscriptionName
                });
            });
        }

        public void ClearConsumers()
        {
            _watcher.DebugFormat("Clearing consumer subscriptions");
            _createdConsumers.ForEach(c => c.Dispose());
            _createdConsumers.Clear();
        }

        public int BatchSize { get; protected set; }

        private bool _disposed;
        public void Dispose()
        {
            if (!_disposed)
            {
                ClearConsumers();
                _messageHandlerFactory.Dispose();
            }
            _disposed = true;
        }

        protected void CheckMessageType<TMessage>(IBasicProperties properties)
        {
            var typeName = Global.DefaultTypeNameSerializer.Serialize(typeof(TMessage));
            if (properties.Type != typeName)
            {
                _watcher.ErrorFormat("Message type is incorrect. Expected '{0}', but was '{1}'", typeName, properties.Type);
                throw new Exception(string.Format("Message type is incorrect. Expected '{0}', but was '{1}'", typeName, properties.Type));
            }
        }
    }
}
