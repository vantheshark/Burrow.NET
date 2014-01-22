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
        public void Should_bind_with_provided_params()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            var queueSetupData = new QueueSetupData();
            queueSetupData.Arguments.Add("Key1", "Val1");
            setup.BindQueue<Customer>(model, queueSetupData, "ExchangeName", "QueueName", "RoutingKey");

            // Assert
            model.Received().QueueBind(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Is<IDictionary<string, object>>(arg => arg["Key1"] == "Val1"));
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