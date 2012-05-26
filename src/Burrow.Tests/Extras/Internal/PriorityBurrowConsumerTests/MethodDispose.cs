using System;
using System.Threading.Tasks;
using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityBurrowConsumerTests
{
    [TestClass]
    public class MethodDispose
    {
        [TestMethod]
        public void Should_unsubscribe_event_handlers()
        {
            // Arrange
            var sharedBroker = new SharedEventBroker(NSubstitute.Substitute.For<IRabbitWatcher>());
            var handler = new PriorityMessageHandler(NSubstitute.Substitute.For<IConsumerErrorHandler>(),
                                                     NSubstitute.Substitute.For<Func<BasicDeliverEventArgs, Task>>(),
                                                     NSubstitute.Substitute.For<IRabbitWatcher>());

            var consumer = new PriorityBurrowConsumerForTest(sharedBroker, 0, NSubstitute.Substitute.For<IModel>(), handler,
                                                             NSubstitute.Substitute.For<IRabbitWatcher>(), "consumerTag", false, 3);

            // Action
            consumer.Dispose();
            sharedBroker.TellOthersAPriorityMessageIsHandled(NSubstitute.Substitute.For<IBasicConsumer>(), 10);
            sharedBroker.TellOthersAPriorityMessageIsFinished(NSubstitute.Substitute.For<IBasicConsumer>(), 10);

            // Assert
            Assert.IsNull(consumer.IsInterupted);
        }
    }
}
// ReSharper restore InconsistentNaming