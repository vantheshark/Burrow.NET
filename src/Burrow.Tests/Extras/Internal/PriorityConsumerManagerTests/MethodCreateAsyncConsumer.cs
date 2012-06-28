using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityConsumerManagerTests
{
    [TestClass]
    public class MethodCreateAsyncConsumer
    {
        readonly IRabbitWatcher watcher = NSubstitute.Substitute.For<IRabbitWatcher>();
        readonly IMessageHandlerFactory handlerFactory = NSubstitute.Substitute.For<IMessageHandlerFactory>();
        readonly ISerializer serializer = NSubstitute.Substitute.For<ISerializer>();
        readonly IModel channel = NSubstitute.Substitute.For<IModel>();

        [TestMethod]
        public void Should_return_PriorityBurrowConsumer_object_when_called()
        {
            // Arrange
            var consumerManager = new PriorityConsumerManager(watcher, handlerFactory, serializer);

            // Action
            var consumer1 = consumerManager.CreateAsyncConsumer<string>(channel, "name", "tag", x => { }, 2);
            var consumer2 = consumerManager.CreateAsyncConsumer<string>(channel, "name", "tag", (x,y) => { }, 2);

            // Assert
            Assert.IsInstanceOfType(consumer1, typeof(PriorityBurrowConsumer));
            Assert.IsInstanceOfType(consumer2, typeof(PriorityBurrowConsumer));
        }

    }
}
// ReSharper restore InconsistentNaming