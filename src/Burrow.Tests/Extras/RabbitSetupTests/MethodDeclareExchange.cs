using System;
using System.Collections.Generic;
using Burrow.Extras;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.RabbitSetupTests
{
    [TestFixture]
    public class MethodDeclareExchange
    {
        [Test]
        public void Should_not_create_exchange_and_write_warning_log_if_exchangeName_is_null_or_empty()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.DeclareExchange(new ExchangeSetupData(), model, null);

            // Assert
            model.DidNotReceive().ExchangeDeclare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<IDictionary<string, object>>());
            setup.Watcher.Received(1).WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void Should_catch_OperationInterruptedException()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);
            model.When(x => x.ExchangeDeclare(Arg.Any<string>(),Arg.Any<string>(),Arg.Any<bool>(),Arg.Any<bool>(),Arg.Any<IDictionary<string, object>>()))
                 .Do(callInfo =>
                {
                    throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Library, 101, "PRECONDITION_FAILED - Exchange exists"));
                });

            // Action
            setup.DeclareExchange(new ExchangeSetupData(), model, "Exchange name");

            // Assert
            model.Received(1).ExchangeDeclare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<IDictionary<string, object>>());
            setup.Watcher.Received(1).ErrorFormat(Arg.Any<string>());
        }

        [Test]
        public void Should_catch_OperationInterruptedException_and_write_errorLog()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);
            model.When(x => x.ExchangeDeclare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<IDictionary<string, object>>()))
                 .Do(callInfo =>
                 {
                     throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Library, 101, "Some errors"));
                 });

            // Action
            setup.DeclareExchange(new ExchangeSetupData(), model, "Exchange name");

            // Assert
            model.Received(1).ExchangeDeclare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<IDictionary<string, object>>());
            setup.Watcher.Received(1).Error(Arg.Any<Exception>());
        }

        [Test]
        public void Should_catch_all_other_error()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);
            model.When(x => x.ExchangeDeclare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<IDictionary<string, object>>()))
                 .Do(callInfo =>
                 {
                     throw new Exception();
                 });

            // Action
            setup.DeclareExchange(new ExchangeSetupData(), model, "Exchange name");

            // Assert
            model.Received(1).ExchangeDeclare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<IDictionary<string, object>>());
            setup.Watcher.Received(1).Error(Arg.Any<Exception>());
        }
    }
}
// ReSharper restore InconsistentNaming