using System;
using Burrow.Tests.Extras.RabbitSetupTests;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.DefaultMessageHandlerTests
{
    [TestFixture]
    public class MethodHandleError
    {
        [Test]
        public void Should_call_error_handler_to_handle_the_error()
        {
            // Arrange
            var errorHanlder = Substitute.For<IConsumerErrorHandler>();
            var handler = new DefaultMessageHandler<Customer>("SubscriptionName", Substitute.For<Action<Customer, MessageDeliverEventArgs>>(), errorHanlder, Substitute.For<ISerializer>(), Substitute.For<IRabbitWatcher>());

            // Action
            handler.HandleError(new BasicDeliverEventArgs("tag", 1, false, "e", "r", Substitute.For<IBasicProperties>(), new byte[0]), new Exception());

            // Assert
            errorHanlder.Received().HandleError(Arg.Any<BasicDeliverEventArgs>(), Arg.Any<Exception>());
        }
    }
}
// ReSharper restore InconsistentNaming