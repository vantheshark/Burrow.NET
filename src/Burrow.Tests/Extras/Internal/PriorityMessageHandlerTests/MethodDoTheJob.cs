using System;
using System.Collections.Specialized;
using System.Diagnostics;
using Burrow.Extras.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityMessageHandlerTests
{
    [TestClass]
    public class MethodDoTheJob
    {
        [TestMethod]
        public void Should_invoke_the_callback_with_MessageDeliverEventArgs_that_has_PriorityValue()
        {
            // Arrange
            var properties = Substitute.For<IBasicProperties>();
            properties.Priority = 10;
            properties.Type = Global.DefaultTypeNameSerializer.Serialize(typeof(Customer));
            properties.Headers = new HybridDictionary();
            properties.Headers.Add("Priority", new byte[] { (byte)'1', (byte)'0' });
            properties.Headers.Add("RoutingKey", "Customer");
            MessageDeliverEventArgs evt = null;
            var handler = new PriorityMessageHandlerForTest<Customer>("SubscriptionName",
                                                                     (x, e) => { evt = e; },
                                                                     Substitute.For<IConsumerErrorHandler>(),
                                                                     Substitute.For<ISerializer>(),
                                                                     Substitute.For<IRabbitWatcher>());

            // Action
            handler.PublicDoTheJob(new BasicDeliverEventArgs{BasicProperties = properties});

            // Assert
            Assert.IsNotNull(evt);
            Assert.AreEqual((uint)10, evt.MessagePriority);

        }
    }

    [DebuggerStepThrough]
    internal class PriorityMessageHandlerForTest<T> : PriorityMessageHandler<T>
    {
        public PriorityMessageHandlerForTest(string subscriptionName, Action<T, MessageDeliverEventArgs> msgHandlingAction, IConsumerErrorHandler consumerErrorHandler, ISerializer messageSerializer, IRabbitWatcher watcher) 
            : base(subscriptionName, msgHandlingAction, consumerErrorHandler, messageSerializer, watcher)
        {
        }

        public void PublicDoTheJob(BasicDeliverEventArgs eventArgs)
        {
            DoTheJob(eventArgs);
        }
    }
}
// ReSharper restore InconsistentNaming