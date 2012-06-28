using System.Threading;
using RabbitMQ.Client;

namespace Burrow.Tests.BurrowConsumerTests
{
    public class BurrowConsumerForTest : BurrowConsumer
    {
        public BurrowConsumerForTest(IModel channel, IMessageHandler messageHandler, IRabbitWatcher watcher, bool autoAck, int batchSize) 
            : base(channel, messageHandler, watcher, autoAck, batchSize)
        {
             WaitHandler = new AutoResetEvent(false);
        }


        protected override void DoAck(RabbitMQ.Client.Events.BasicDeliverEventArgs basicDeliverEventArgs, IBasicConsumer subscriptionInfo)
        {
            base.DoAck(basicDeliverEventArgs, subscriptionInfo);
            WaitHandler.Set();
        }

        public void DoAckForTest(RabbitMQ.Client.Events.BasicDeliverEventArgs basicDeliverEventArgs, IBasicConsumer subscriptionInfo)
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
