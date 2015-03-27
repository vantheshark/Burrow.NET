using System;
using System.Collections.Generic;
using Burrow.RPC;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcServerCoordinatorTests
{
    [TestFixture]
    public class MethodHandleMesage
    {
        private ITunnel tunnel;
        private IMethodMatcher methodMatcher;
        private const string ConnectionString = "host=localhost;username=guest;password=guest";

        [SetUp]
        public void Setup()
        {
            tunnel = Substitute.For<ITunnel>();
            methodMatcher = Substitute.For<IMethodMatcher>();
            RabbitTunnel.Factory = Substitute.For<TunnelFactory>();
            RabbitTunnel.Factory.Create(Arg.Any<string>()).Returns(tunnel);
            Global.DefaultWatcher = Substitute.For<IRabbitWatcher>();

            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            InternalDependencies.MethodMatcher = methodMatcher;
        }

        [Test]
        public void Should_print_warn_msg_and_return_if_msg_is_expired()
        {
            // Arrange
            Global.DefaultWatcher = Substitute.For<IRabbitWatcher>();
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, ConnectionString, "10");

            var request = new RpcRequest
            {
                UtcExpiryTime = DateTime.UtcNow.AddSeconds(-10)
            };

            // Action
            server.HandleMesage(request);

            // Assert
            Global.DefaultWatcher.Received(1).WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
            tunnel.DidNotReceive().Publish(Arg.Any<RpcResponse>(), Arg.Any<string>());
        }

        [Test]
        public void Should_publish_respones_with_Exception_if_method_not_match()
        {
            // Arrange
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, ConnectionString, "10");
            var request = new RpcRequest
            {
                Id = Guid.NewGuid(),
                ResponseAddress = "Address"
            };

            // Action
            server.Start();
            server.HandleMesage(request);

            // Assert
            tunnel.Received(1).Publish(Arg.Is<RpcResponse>(arg => arg.Exception != null && arg.RequestId == request.Id), "Address");
        }

        [Test]
        public void Should_publish_nothing_if_msg_is_Async()
        {
            // Arrange
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, ConnectionString, "10");
            var request = new RpcRequest
            {
                Id = Guid.NewGuid(),
            };

            // Action
            server.Start();
            server.HandleMesage(request);

            // Assert
            tunnel.DidNotReceive().Publish(Arg.Any<RpcResponse>(), Arg.Any<string>());
        }

        [Test]
        public void Should_invoke_method_on_real_instance_and_map_response_params()
        {
            // Arrange
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, ConnectionString, "10");
            var request = new RpcRequest
            {
                Id = Guid.NewGuid(),
                ResponseAddress = "Address",
                MethodName = "Search",
                Params = new Dictionary<string, object> { { "page", (long)1 /* long value will be converted to proper int value */}, {"query", new SomeMessage
                {
                    Name = "vantheshark"
                }}}
            };
            var methodInfo = typeof(ISomeService).GetMethod("Search");
            methodMatcher.Match<ISomeService>(request)
                         .Returns(methodInfo);

            var returnValue = new List<SomeMessage> { new SomeMessage { Money = "$1" }, new SomeMessage { Money = "$1" } };
            instance.Search(1, Arg.Is<SomeMessage>(arg => arg.Name == "vantheshark"))
                    .Returns(returnValue);

            // Action
            server.Start();
            server.HandleMesage(request);

            // Assert
            tunnel.Received(1).Publish(Arg.Is<RpcResponse>(arg => arg.Exception == null && 
                                                                  arg.ReturnValue == returnValue &&
                                                                  arg.ChangedParams["query"] is SomeMessage &&
                                                                  1.Equals(arg.ChangedParams["page"]) &&
                                                                  arg.RequestId == request.Id), "Address");
        }
    }
}
// ReSharper restore InconsistentNaming