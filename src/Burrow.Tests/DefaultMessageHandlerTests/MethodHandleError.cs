using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NSubstitute;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.DefaultMessageHandlerTests
{
    [TestClass]
    public class MethodHandleError
    {
        [TestMethod]
        public void Should_call_error_handler_to_handle_the_error()
        {
            // Arrange
            var errorHanlder = Substitute.For<IConsumerErrorHandler>();
            var handler = new DefaultMessageHandler(errorHanlder, Substitute.For<Func<BasicDeliverEventArgs, Task>>(), Substitute.For<IRabbitWatcher>());

            // Action
            handler.HandleError(Substitute.For<IBasicConsumer>(), 
                                new BasicDeliverEventArgs("tag", 1, false, "e", "r", Substitute.For<IBasicProperties>(), new byte[0]), 
                                new Exception());

            // Assert
            errorHanlder.Received().HandleError(Arg.Any<BasicDeliverEventArgs>(), Arg.Any<Exception>());
        }
    }
}
// ReSharper restore InconsistentNaming