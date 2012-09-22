using System;
using System.Collections.Generic;
using System.Reflection;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcServerCoordinatorTests
{
    [TestClass]
    public class MethodHandleMesage
    {
        private ITunnel tunnel;
        private IMethodMatcher methodMatcher;

        [TestInitialize]
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

        [TestMethod]
        public void Should_print_warn_msg_and_return_if_msg_is_expired()
        {
            // Arrange
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, "queue-connnection-string", "10");

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

        [TestMethod]
        public void Should_publish_respones_with_Exception_if_method_not_match()
        {
            // Arrange
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, "queue-connnection-string", "10");
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

        [TestMethod]
        public void Should_publish_nothing_if_msg_is_Async()
        {
            // Arrange
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, "queue-connnection-string", "10");
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

        [TestMethod]
        public void Should_invoke_method_on_real_instance_and_map_response_params()
        {
            // Arrange
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, "queue-connnection-string", "10");
            var request = new RpcRequest
            {
                Id = Guid.NewGuid(),
                ResponseAddress = "Address",
                MethodName = "Search",
                Params = new Dictionary<string, object> { { "page", 1 }, {"query", new SomeMessage
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