using System;
using System.Collections.Generic;
using System.Linq;
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
        protected readonly List<IBasicConsumer> _createdConsumers;

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
            _messageHandlerFactory = messageHandlerFactory;
            _serializer = serializer;
            _createdConsumers = new List<IBasicConsumer>();
        }

        public virtual IBasicConsumer CreateConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T> onReceiveMessage)
        {
            var action = CreateJobFactory(onReceiveMessage);
            var messageHandler = _messageHandlerFactory.Create(action);
            var consumer = new BurrowConsumer(channel, messageHandler, _watcher, true, 1);
            _createdConsumers.Add(consumer);
            return consumer;
        }

        public virtual IBasicConsumer CreateConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T, MessageDeliverEventArgs> onReceiveMessage)
        {
            var action = CreateJobFactory(subscriptionName, onReceiveMessage);
            var messageHandler = _messageHandlerFactory.Create(action);
            var consumer = new BurrowConsumer(channel, messageHandler, _watcher, false, 1);
            _createdConsumers.Add(consumer);
            return consumer;
        }

        public virtual IBasicConsumer CreateAsyncConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T> onReceiveMessage, ushort? batchSize)
        {
            var action = CreateJobFactory(onReceiveMessage);
            var messageHandler = _messageHandlerFactory.Create(action);
            var consumer = new BurrowConsumer(channel, messageHandler, _watcher, true, (batchSize > 1 ? batchSize.Value : Global.DefaultConsumerBatchSize));
            _createdConsumers.Add(consumer);
            return consumer;
        }

        public virtual IBasicConsumer CreateAsyncConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T, MessageDeliverEventArgs> onReceiveMessage, ushort? batchSize)
        {
            var action = CreateJobFactory(subscriptionName, onReceiveMessage);
            var messageHandler = _messageHandlerFactory.Create(action);
            var consumer = new BurrowConsumer(channel, messageHandler, _watcher, false, (batchSize > 1 ? batchSize.Value : Global.DefaultConsumerBatchSize));
            _createdConsumers.Add(consumer);
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
            _createdConsumers.OfType<IDisposable>().ToList().ForEach(c => c.Dispose());
            _createdConsumers.Clear();
        }

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
