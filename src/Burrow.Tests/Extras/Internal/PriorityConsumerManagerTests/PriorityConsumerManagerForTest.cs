using System;
using System.Threading.Tasks;
using Burrow.Extras.Internal;

namespace Burrow.Tests.Extras.Internal.PriorityConsumerManagerTests
{
    internal class PriorityConsumerManagerForTest : PriorityConsumerManager
    {
        public PriorityConsumerManagerForTest(IRabbitWatcher watcher, IMessageHandlerFactory messageHandlerFactory, ISerializer serializer) 
            : base(watcher, messageHandlerFactory, serializer)
        {
        }

        public Func<RabbitMQ.Client.Events.BasicDeliverEventArgs, Task> CreateJobFactoryForTest<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage)
        {
            return CreateJobFactory(subscriptionName, onReceiveMessage);
        }
    }
}
