using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
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
            var tunnel = new RabbitTunnel(routeFinder, durableConnection);

            // Action
            tunnel.Publish("Muahaha");

            // Assert
            routeFinder.Received().FindRoutingKey<string>();
            newChannel.Received().BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>());
        }

		[TestMethod]
        public void Should_publish_message_to_header_exchange_with_custom_headers_if_provided()
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
            var tunnel = new RabbitTunnel(routeFinder, durableConnection);

            // Action
            tunnel.Publish("Muahaha", new Dictionary<string, object>{ { "SomeHeaderKey", "somevalue" }, { "AnotherHeaderKey", "somethingelse" } });

            // Assert
            routeFinder.Received(1).FindRoutingKey<string>();
            newChannel.Received(1).BasicPublish(Arg.Any<string>(), 
                                                Arg.Any<string>(),
                                                Arg.Is<IBasicProperties>(arg => arg.Headers["SomeHeaderKey"].ToString() == "somevalue" && arg.Headers["AnotherHeaderKey"].ToString() == "somethingelse"), 
                                                Arg.Any<byte[]>());
        }

        [TestMethod, ExpectedException(typeof(Exception), "Publish failed. No channel to rabbit server established.")]
        public void Should_throw_exception_if_dedicated_publish_channel_is_not_created_properly()
        {
            // Arrange
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(null, out durableConnection);

            // Action
            tunnel.Publish("Muahaha");
        }

        [TestMethod, ExpectedException(typeof(Exception), "Publish failed. No channel to rabbit server established.")]
        public void Should_throw_exception_if_dedicated_publish_channel_is_not_connected()
        {
            // Arrange
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(Substitute.For<IModel>(), out durableConnection, false);

            // Action
            tunnel.Publish("Muahaha");
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
            var tunnel = new RabbitTunnel(routeFinder, durableConnection);

            // Action
            tunnel.Publish("Muahaha");
        }

        [TestMethod]
        public void Should_create_dedicated_channel_without_reconnect_if_connection_has_been_connected_before()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            newChannel.IsOpen.Returns(true);
            var routeFinder = Substitute.For<IRouteFinder>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.IsConnected.Returns(true);
            durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.CreateChannel().Returns(newChannel);
            var tunnel = new RabbitTunnel(routeFinder, durableConnection);

            // Action
            tunnel.Publish("Muahaha");
            tunnel.Publish("Hohohoho");

            // Assert
            routeFinder.Received().FindRoutingKey<string>();
            durableConnection.Received(1).CreateChannel();
            durableConnection.DidNotReceive().Connect();
            newChannel.Received(2).BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>());
        }
    }
}
// ReSharper restore InconsistentNaming