using System;
using System.Collections.Generic;
using Burrow.Extras;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.RabbitSetupTests
{
    [TestClass]
    public class MethodBindQueue
    {
        [TestMethod]
        public void Should_not_bind_and_write_warning_log_if_exchangeName_is_null_or_empty()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.BindQueue<Customer>(model, new QueueSetupData(), null, "", "");

            // Assert
            model.DidNotReceive().QueueBind(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            setup.Watcher.Received(1).WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
            setup.Watcher.DidNotReceive().Error(Arg.Any<Exception>());
        }


        [TestMethod]
        public void Should_catch_all_exception()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);
            model.When(x => x.QueueBind(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IDictionary<string, object>>()))
                 .Do(info => { throw new Exception();});

            // Action
            setup.BindQueue<Customer>(model, new QueueSetupData(), "Exchange", "Queue", "Key");

            // Assert
            setup.Watcher.Received(1).Error(Arg.Any<Exception>());
        }
    }
}
// ReSharper restore InconsistentNaming