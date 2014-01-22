using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityConsumerManagerTests
{
    [TestClass]
    public class MethodCreateConsumer
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
            var consumer1 = consumerManager.CreateConsumer<string>(channel, "name", x => { }, 1);

            // Assert
            Assert.IsInstanceOfType(consumer1, typeof(PriorityBurrowConsumer));
        }

    }
}
// ReSharper restore InconsistentNaming