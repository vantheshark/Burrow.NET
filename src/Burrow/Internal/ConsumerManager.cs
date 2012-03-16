using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Burrow.Internal
{
    public class ConsumerManager : IConsumerManager
    {
        private readonly IRabbitWatcher _watcher;
        private readonly IConsumerErrorHandler _consumerErrorStrategy;
        private readonly ISerializer _serializer;
        private readonly List<BurrowConsummer> _createdConsumers;

        public ConsumerManager(IRabbitWatcher watcher, IConsumerErrorHandler consumerErrorStrategy, ISerializer serializer, int batchSize)
        {
            if (consumerErrorStrategy == null)
            {
                throw new ArgumentNullException("consumerErrorStrategy");
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }
            if (batchSize < 1)
            {
                throw new ArgumentNullException("batchSize", "batchSize must be greter than or equal 1");
            }
            
            BatchSize = batchSize;

            _watcher = watcher;
            _consumerErrorStrategy = consumerErrorStrategy;
            _serializer = serializer;
            _createdConsumers = new List<BurrowConsummer>();
        }

        public IBasicConsumer CreateConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T> onReceiveMessage)
        {
            var action = CreateJobFactory(onReceiveMessage);
            var consumer = new SequenceConsumer(_watcher, _consumerErrorStrategy, _serializer, channel, consumerTag, action);
            _createdConsumers.Add(consumer);
            return consumer;
        }

        public IBasicConsumer CreateAsyncConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T> onReceiveMessage)
        {
            var action = CreateJobFactory(onReceiveMessage);
            var consumer = new ParallelConsumer(_watcher, _consumerErrorStrategy, _serializer, channel, consumerTag, action, BatchSize);
            _createdConsumers.Add(consumer);
            return consumer;
        }

        private Func<BasicDeliverEventArgs, Task> CreateJobFactory<T>(Action<T> onReceiveMessage)
        {
            return eventArgs => Task.Factory.StartNew(() =>
            {
                CheckMessageType<T>(eventArgs.BasicProperties);
                var message = _serializer.Deserialize<T>(eventArgs.Body);
                onReceiveMessage(message);
            });
        }

        public void ClearConsumers()
        {
            _watcher.DebugFormat("Clearing consumer subscriptions");
            _createdConsumers.ForEach(c => c.Dispose());
            _createdConsumers.Clear();
        }

        public int BatchSize { get; private set; }

        private bool _disposed;
        public void Dispose()
        {
            if (!_disposed)
            {
                _consumerErrorStrategy.Dispose();
                ClearConsumers();
            }
            _disposed = true;
        }

        private void CheckMessageType<TMessage>(IBasicProperties properties)
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
