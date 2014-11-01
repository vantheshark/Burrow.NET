using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Burrow.Tests.BurrowConsumerTests
{
    public class BurrowConsumerForTest : BurrowConsumer
    {
        public static BasicDeliverEventArgs ADeliverEventArgs = new BasicDeliverEventArgs();

        static BurrowConsumerForTest()
        {
            Global.ConsumerDisposeTimeoutInSeconds = 1;
            ADeliverEventArgs.ConsumerTag = "ConsumerTag";
        }

        public BurrowConsumerForTest(IModel channel, IMessageHandler messageHandler, IRabbitWatcher watcher, bool autoAck, int batchSize) 
            : base(channel, messageHandler, watcher, autoAck, batchSize)
        {
             WaitHandler = new AutoResetEvent(false);
             ConsumerTag = "BurrowConsumerForTest";
        }

        internal protected override void DoAck(BasicDeliverEventArgs basicDeliverEventArgs, IBasicConsumer subscriptionInfo)
        {
            base.DoAck(basicDeliverEventArgs, subscriptionInfo);
            WaitHandler.Set();
        }

        public void DoAckForTest(BasicDeliverEventArgs basicDeliverEventArgs, IBasicConsumer subscriptionInfo)
        {
            DoAck(basicDeliverEventArgs, subscriptionInfo);
        }


        protected override void WhenChannelShutdown(IModel model, ShutdownEventArgs reason)
        {
            base.WhenChannelShutdown(model, reason);
            WaitHandler.Set();
        }

        public AutoResetEvent WaitHandler { get; set; }
    }
}
