using System;
using System.Threading.Tasks;
using Burrow.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Burrow.Tests.Internal.ConsumerManagerTests
{
    public class ConsumerManagerForTest : ConsumerManager
    {
        public ConsumerManagerForTest(IRabbitWatcher watcher, IMessageHandlerFactory messageHandlerFactory, ISerializer serializer) 
            : base(watcher, messageHandlerFactory, serializer)
        {
        }

        public Func<BasicDeliverEventArgs, Task> CreateJobFactoryForTest<T>(Action<T> onReceiveMessage)
        {
            return CreateJobFactory(onReceiveMessage);
        }

        public Func<BasicDeliverEventArgs, Task> CreateJobFactoryForTest<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage)
        {
            return CreateJobFactory(subscriptionName, onReceiveMessage);
        }

        public void CheckMessageTypeForTest<TMessage>(IBasicProperties properties)
        {
            CheckMessageType<TMessage>(properties);
        }
    }
}
