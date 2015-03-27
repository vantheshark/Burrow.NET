using Burrow.Extras.Internal;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityConsumerManagerTests
{
    [TestFixture]
    public class MethodCreateAsyncConsumer
    {
        readonly IRabbitWatcher watcher = NSubstitute.Substitute.For<IRabbitWatcher>();
        readonly IMessageHandlerFactory handlerFactory = NSubstitute.Substitute.For<IMessageHandlerFactory>();
        readonly ISerializer serializer = NSubstitute.Substitute.For<ISerializer>();
        readonly IModel channel = NSubstitute.Substitute.For<IModel>();

        [Test]
        public void Should_return_PriorityBurrowConsumer_object_when_called()
        {
            // Arrange
            var consumerManager = new PriorityConsumerManager(watcher, handlerFactory, serializer);

            // Action
            var consumer2 = consumerManager.CreateAsyncConsumer<string>(channel, "name", (x,y) => { }, 2);

            // Assert
            Assert.IsInstanceOfType(typeof(PriorityBurrowConsumer), consumer2);
        }

    }
}
// ReSharper restore InconsistentNaming