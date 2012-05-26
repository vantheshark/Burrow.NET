using System.Threading;
using Burrow.Extras.Internal;
using RabbitMQ.Client;

namespace Burrow.Tests.Extras.Internal.PriorityBurrowConsumerTests
{
    internal class PriorityBurrowConsumerForTest : PriorityBurrowConsumer
    {
        public PriorityBurrowConsumerForTest(SharedEventBroker eventBroker, int priority, IModel channel, IMessageHandler messageHandler, IRabbitWatcher watcher, string consumerTag, bool autoAck, int batchSize) 
            : base(eventBroker, priority, channel, messageHandler, watcher, consumerTag, autoAck, batchSize)
        {
        }

        public bool? IsInterupted;
        public AutoResetEvent ResumeWaitHandler = new AutoResetEvent(false);
        public AutoResetEvent InteruptWaitHandler = new AutoResetEvent(false);

        internal override void Resume(int releaseCount)
        {
            IsInterupted = false;
            ResumeWaitHandler.Set();
        }
        internal override void Interupt(int volumn)
        {
            IsInterupted = true;
            InteruptWaitHandler.Set();
        }
    }
}