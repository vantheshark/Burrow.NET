using System;
using System.Collections.Generic;
using System.Linq;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.RpcClientInterceptorTests
{
    [TestClass]
    public class MethodIntercept
    {
        [TestMethod, ExpectedException(typeof(Exception))]
        public void Should_throw_exception_if_response_is_null()
        {
            // Arrange
            var coordinator = Substitute.For<IRpcClientCoordinator>();
            var interceptor = new RpcClientInterceptor(coordinator);
            var service = RpcFactory.CreateClient<ISomeService>(interceptor);
            int totalCount;

            // Assert
            service.Get(1, 200, out totalCount);
        }

        [TestMethod]
        public void Should_set_time_to_live_in_the_request()
        {
            // Arrange
            DefaultMethodFilter.CheckedMethodCaches.Clear();
            var coordinator = Substitute.For<IRpcClientCoordinator>();
            coordinator.Send(Arg.Any<RpcRequest>())
                       .Returns(new RpcResponse
                       {
                           ChangedParams = new Dictionary<string, object> { { "result", "out string" } }
                       });
            var interceptor = new RpcClientInterceptor(coordinator);
            var service = RpcFactory.CreateClient<ISomeService>(interceptor);

            // Assert
            string outValue;
            service.TryParse(out outValue);


            // Assert
            coordinator.Received(1).Send(Arg.Is<RpcRequest>(arg => arg.UtcExpiryTime != null));
            Assert.AreEqual("out string", outValue);
        }

        [TestMethod]
        public void Should_set_out_value_and_return_value()
        {
            // Arrange
            var coordinator = Substitute.For<IRpcClientCoordinator>();
            coordinator.Send(Arg.Any<RpcRequest>())
                       .Returns(new RpcResponse
                       {
                           ReturnValue = new List<SomeMessage>{new SomeMessage()},
                           ChangedParams = new Dictionary<string, object>{{"totalCount", 1000}}
                       });
            var interceptor = new RpcClientInterceptor(coordinator);
            var service = RpcFactory.CreateClient<ISomeService>(interceptor);
            int totalCount;

            // Assert
            var result = service.Get(1, 200, out totalCount);


            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(1000, totalCount);
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public void Should_throw_exception_if_missing_out_value_in_response_object()
        {
            // Arrange
            var coordinator = Substitute.For<IRpcClientCoordinator>();
            coordinator.Send(Arg.Any<RpcRequest>())
                       .Returns(new RpcResponse
                       {
                           ReturnValue = new List<SomeMessage> { new SomeMessage() }
                       });
            var interceptor = new RpcClientInterceptor(coordinator);
            var service = RpcFactory.CreateClient<ISomeService>(interceptor);
            int totalCount;

            // Assert
            service.Get(1, 200, out totalCount);
        }

        [TestMethod]
        public void Should_send_async_if_method_is_async()
        {
            // Arrange
            var coordinator = Substitute.For<IRpcClientCoordinator>();
            coordinator.Send(Arg.Any<RpcRequest>())
                       .Returns(new RpcResponse());
            var interceptor = new RpcClientInterceptor(coordinator);
            var service = RpcFactory.CreateClient<ISomeService>(interceptor);

            // Assert
            var msg = new SomeMessage();
            service.Save(ref msg);

            // Assert
            coordinator.Received(1).SendAsync(Arg.Is<RpcRequest>(arg => arg.UtcExpiryTime == null));
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public void Should_throw_exception_if_resonse_has_exception()
        {
            // Arrange
            var coordinator = Substitute.For<IRpcClientCoordinator>();
            coordinator.Send(Arg.Any<RpcRequest>())
                       .Returns(new RpcResponse
                       {
                           Exception = new Exception()
                       });
            var interceptor = new RpcClientInterceptor(coordinator);
            var service = RpcFactory.CreateClient<ISomeService>(interceptor);

            // Assert
            string outValue;
            service.TryParse(out outValue);
        }

        [TestMethod]
        public void Should_update_byref_param_if_any()
        {
            // Arrange
            var coordinator = Substitute.For<IRpcClientCoordinator>();
            coordinator.Send(Arg.Any<RpcRequest>())
                       .Returns(new RpcResponse
                       {
                           ChangedParams = new Dictionary<string, object> { { "message", new SomeMessage{Money = "$1000"} } }
                       });
            var interceptor = new RpcClientInterceptor(coordinator);
            var service = RpcFactory.CreateClient<ISomeService>(interceptor);

            // Action
            var message = new SomeMessage();
            service.SaveNotAsync(ref message);


            Assert.AreEqual("$1000", message.Money);
        }
    }
}
// ReSharper restore InconsistentNaming