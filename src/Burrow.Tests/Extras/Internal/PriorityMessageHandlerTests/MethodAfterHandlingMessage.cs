using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.v0_8;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityMessageHandlerTests
{
    [TestClass]
    public class MethodAfterHandlingMessage
    {
        [TestMethod]
        public void Should_do_nothing_if_Consumer_is_not_PriorityBurrowConsumer()
        {
            // Arrange
            var handler = new PriorityMessageHandler(NSubstitute.Substitute.For<IConsumerErrorHandler>(),
                                                     NSubstitute.Substitute.For<Func<BasicDeliverEventArgs, Task>>(), 
                                                     NSubstitute.Substitute.For<IRabbitWatcher>());

            // Action
            handler.AfterHandlingMessage(null, new BasicDeliverEventArgs());
            handler.AfterHandlingMessage(NSubstitute.Substitute.For<IBasicConsumer>(), new BasicDeliverEventArgs());
        }

        [TestMethod]
        public void Should_send_notification_if_Consumer_is_PriorityBurrowConsumer()
        {
            // Arrange
            var sharedBroker = new SharedEventBroker(NSubstitute.Substitute.For<IRabbitWatcher>());
            var p = -1;
            sharedBroker.WhenAConsumerFinishedAMessage += (c, priority) =>
                                                     {
                                                         p = priority;
                                                     };
            var handler = new PriorityMessageHandler(NSubstitute.Substitute.For<IConsumerErrorHandler>(),
                                                     NSubstitute.Substitute.For<Func<BasicDeliverEventArgs, Task>>(),
                                                     NSubstitute.Substitute.For<IRabbitWatcher>());

            var consumer = new PriorityBurrowConsumer(sharedBroker, 10, NSubstitute.Substitute.For<IModel>(), handler,
                                                      NSubstitute.Substitute.For<IRabbitWatcher>(), "consumerTag", false, 10);

            var eventArgs = new BasicDeliverEventArgs{BasicProperties = new BasicProperties {Headers = new HybridDictionary()}};
            eventArgs.BasicProperties.Headers["Priority"] = new[] { (byte)'1', (byte)'0' };
            
            // Action
            handler.AfterHandlingMessage(consumer, eventArgs);

            // Assert
            System.Threading.Thread.Sleep(100);
            Assert.AreEqual(10, p);
        }
    }
}
// ReSharper restore InconsistentNaming