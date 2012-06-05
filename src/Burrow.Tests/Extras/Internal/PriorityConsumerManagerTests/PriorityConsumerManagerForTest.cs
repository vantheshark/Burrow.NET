using System;
using System.Threading.Tasks;
using Burrow.Extras.Internal;

namespace Burrow.Tests.Extras.Internal.PriorityConsumerManagerTests
{
    public class PriorityConsumerManagerForTest : PriorityConsumerManager
    {
        public PriorityConsumerManagerForTest(IRabbitWatcher watcher, IMessageHandlerFactory messageHandlerFactory, ISerializer serializer, int batchSize) 
            : base(watcher, messageHandlerFactory, serializer, batchSize)
        {
        }

        public Func<RabbitMQ.Client.Events.BasicDeliverEventArgs, Task> CreateJobFactoryForTest<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage)
        {
            return CreateJobFactory(subscriptionName, onReceiveMessage);
        }
    }
}
