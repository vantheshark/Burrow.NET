using System;
using System.Collections.Generic;
using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.RabbitTunnelWithPriorityQueuesSupportTests
{
    [TestClass]
    public class MethodPublish
    {
        [TestMethod]
        public void Should_use_route_finder_to_find_routing_key_then_publish_serialized_msg()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            newChannel.IsOpen.Returns(true);
            var routeFinder = Substitute.For<IRouteFinder>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.When(x => x.Connect()).Do(callInfo =>
            {
                durableConnection.Connected += Raise.Event<Action>();
                durableConnection.IsConnected.Returns(true);
            });


            durableConnection.CreateChannel().Returns(newChannel);
            var tunnel = new RabbitTunnelWithPriorityQueuesSupport(routeFinder, durableConnection);

            // Action
            tunnel.Publish("Muahaha", 10);

            // Assert
            routeFinder.Received().FindRoutingKey<string>();
            newChannel.Received().BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Is<IBasicProperties>(arg => arg.Headers["Priority"].ToString() == "10"), Arg.Any<byte[]>());
        }

        [TestMethod]
        public void Should_not_overwrite_the_priority_if_customer_headers_has_Priority_value_set()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            newChannel.IsOpen.Returns(true);
            var routeFinder = Substitute.For<IRouteFinder>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.When(x => x.Connect()).Do(callInfo =>
            {
                durableConnection.Connected += Raise.Event<Action>();
                durableConnection.IsConnected.Returns(true);
            });


            durableConnection.CreateChannel().Returns(newChannel);
            var tunnel = new RabbitTunnelWithPriorityQueuesSupport(routeFinder, durableConnection);

            // Action
            tunnel.Publish("Muahaha", 10, new Dictionary<string, object> { { "Priority", "100" }, { "AnotherHeaderKey", "something" } });

            // Assert
            routeFinder.Received(1).FindRoutingKey<string>();
            newChannel.Received(1).BasicPublish(Arg.Any<string>(), 
                                                Arg.Any<string>(),
                                                Arg.Is<IBasicProperties>(arg => arg.Headers["Priority"].ToString() == "10" && arg.Headers["AnotherHeaderKey"].ToString() == "something"), 
                                                Arg.Any<byte[]>());
        }

        [TestMethod, ExpectedException(typeof(Exception), "Publish failed. No channel to rabbit server established.")]
        public void Should_throw_exception_if_dedicated_publish_channel_is_not_created_properly()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            newChannel.IsOpen.Returns(true);
            var routeFinder = Substitute.For<IRouteFinder>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.When(x => x.Connect()).Do(callInfo =>
            {
                durableConnection.Connected += Raise.Event<Action>();
                durableConnection.IsConnected.Returns(true);
            });
            
            var tunnel = new RabbitTunnelWithPriorityQueuesSupport(routeFinder, durableConnection);

            // Action
            tunnel.Publish("Muahaha", 10);
        }

        [TestMethod, ExpectedException(typeof(Exception), "Publish failed. No channel to rabbit server established.")]
        public void Should_throw_exception_if_dedicated_publish_channel_is_not_connected()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            newChannel.IsOpen.Returns(false);
            var routeFinder = Substitute.For<IRouteFinder>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.When(x => x.Connect()).Do(callInfo =>
            {
                durableConnection.Connected += Raise.Event<Action>();
                durableConnection.IsConnected.Returns(true);
            });

            durableConnection.CreateChannel().Returns(newChannel);
            var tunnel = new RabbitTunnelWithPriorityQueuesSupport(routeFinder, durableConnection);

            // Action
            tunnel.Publish("Muahaha", 10);
        }

        [TestMethod, ExpectedException(typeof(Exception), "Publish failed: 'Test message'")]
        public void Should_throw_exception_if_publish_failed()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            newChannel.IsOpen.Returns(true);
            var routeFinder = Substitute.For<IRouteFinder>();
            routeFinder.When(x =>x.FindExchangeName<string>()).Do(callInfo => { throw new Exception("Test message");});
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.When(x => x.Connect()).Do(callInfo =>
            {
                durableConnection.Connected += Raise.Event<Action>();
                durableConnection.IsConnected.Returns(true);
            });

            durableConnection.CreateChannel().Returns(newChannel);
            var tunnel = new RabbitTunnelWithPriorityQueuesSupport(routeFinder, durableConnection);

            // Action
            tunnel.Publish("Muahaha", 10);
        }
    }
}
// ReSharper restore InconsistentNaming