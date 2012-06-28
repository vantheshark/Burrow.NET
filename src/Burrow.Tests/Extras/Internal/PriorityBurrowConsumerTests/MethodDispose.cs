using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityBurrowConsumerTests
{
    [TestClass]
    public class MethodDispose
    {
        [TestMethod]
        public void Can_called_many_times()
        {
            // Arrange
            var consumer = new PriorityBurrowConsumer(Substitute.For<IModel>(), Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), false, 1);
            consumer.Init(Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>(), new CompositeSubscription(), 1, "sem");

            // Action
            consumer.Dispose();
            consumer.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming