using System.Collections;
using Burrow.Extras;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.RabbitSetupTests
{
    [TestClass]
    public class MethodDeclareExchange
    {
        [TestMethod]
        public void Should_not_create_exchange_and_write_warning_log_if_exchangeName_is_null_or_empty()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.DeclareExchange(new ExchangeSetupData(), model, null);

            // Assert
            model.DidNotReceive().ExchangeDeclare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<IDictionary>());
            setup.Watcher.Received(1).WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }
    }
}
// ReSharper restore InconsistentNaming