using System;
using System.Collections.Generic;
using System.Diagnostics;
using Burrow.Extras.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityMessageHandlerTests
{
    [TestFixture]
    public class MethodHandleMessage
    {
        [Test]
        public void Should_invoke_the_callback_with_MessageDeliverEventArgs_that_has_PriorityValue()
        {
            // Arrange
            var properties = Substitute.For<IBasicProperties>();
            properties.Priority = 10;
            properties.Type = Global.DefaultTypeNameSerializer.Serialize(typeof(Customer));
            properties.Headers = new Dictionary<string, object>();
            properties.Headers.Add("Priority", new[] { (byte)'1', (byte)'0' });
            properties.Headers.Add("RoutingKey", "Customer");
            MessageDeliverEventArgs evt = null;
            var handler = new PriorityMessageHandlerForTest<Customer>("SubscriptionName",
                                                                     (x, e) => { evt = e; },
                                                                     Substitute.For<IConsumerErrorHandler>(),
                                                                     Substitute.For<ISerializer>(),
                                                                     Substitute.For<IRabbitWatcher>());

            // Action
            bool handled;
            handler.PublicHandleMessage(new BasicDeliverEventArgs { BasicProperties = properties }, out handled);

            // Assert
            Assert.IsNotNull(evt);
            Assert.AreEqual((uint)10, evt.MessagePriority);
            Assert.IsTrue(handled);
        }
    }

    [DebuggerStepThrough]
    internal class PriorityMessageHandlerForTest<T> : PriorityMessageHandler<T>
    {
        public PriorityMessageHandlerForTest(string subscriptionName, Action<T, MessageDeliverEventArgs> msgHandlingAction, IConsumerErrorHandler consumerErrorHandler, ISerializer messageSerializer, IRabbitWatcher watcher) 
            : base(subscriptionName, msgHandlingAction, consumerErrorHandler, messageSerializer, watcher)
        {
        }

        public void PublicHandleMessage(BasicDeliverEventArgs eventArgs, out bool handled)
        {
            HandleMessage(eventArgs, out handled);
        }
    }
}
// ReSharper restore InconsistentNaming