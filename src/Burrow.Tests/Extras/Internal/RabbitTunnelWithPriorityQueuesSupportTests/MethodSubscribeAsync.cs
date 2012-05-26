using Burrow.Extras.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.RabbitTunnelWithPriorityQueuesSupportTests
{
    [TestClass]
    public class MethodSubscribeAsync
    {
        [TestMethod]
        public void Should_create_subscriptions_to_priority_queues()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelWithPriorityQueuesSupportForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            tunnel.SubscribeAsync<Customer>("subscriptionName", 3, x => { });

            // Assert
            newChannel.Received(4).BasicQos(0, Global.DefaultConsumerBatchSize, false);
            newChannel.Received().BasicConsume("Queue_Priority0", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority1", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority2", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority3", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
        }


        [TestMethod]
        public void Should_return_composite_subscription()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelWithPriorityQueuesSupportForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            var subs = tunnel.SubscribeAsync<Customer>("subscriptionName", 3, (x, y) => { });

            // Assert
            newChannel.Received(4).BasicQos(0, Global.DefaultConsumerBatchSize, false);
            newChannel.Received().BasicConsume("Queue_Priority0", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority1", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority2", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority3", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            Assert.IsInstanceOfType(subs, typeof(CompositeSubscription));
            Assert.AreEqual(4, subs.Count);
        }
    }
}
// ReSharper restore InconsistentNaming