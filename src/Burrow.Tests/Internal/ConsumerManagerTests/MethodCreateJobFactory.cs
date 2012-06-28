using System.Threading;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.ConsumerManagerTests
{
    [TestClass]
    public class MethodCreateJobFactory
    {
        private ConsumerManagerForTest consumer;

        [TestInitialize]
        public void LockTest()
        {
            consumer = new ConsumerManagerForTest(Substitute.For<IRabbitWatcher>(), Substitute.For<IMessageHandlerFactory>(), Substitute.For<ISerializer>());
        }

        [TestCleanup]
        public void ReleaseTest()
        {
            consumer.Dispose();
        }

        [TestMethod]
        public void Should_return_a_delegate_to_create_Task_when_called()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var func = consumer.CreateJobFactoryForTest<Customer>(x => are.Set());
            var basicProperties = Substitute.For<IBasicProperties>();
            basicProperties.Type.Returns(Global.DefaultTypeNameSerializer.Serialize(typeof(Customer)));
            
            // Action
            func(new BasicDeliverEventArgs
            {
                BasicProperties = basicProperties
            });

            // Assert
            are.WaitOne();

        }

        [TestMethod]
        public void Should_invoke_onReceiveMessage_delegate_with_copied_MessageDeliverEventArgs()
        {
            // Arrange
            MessageDeliverEventArgs eventArgs = null;
            var are = new AutoResetEvent(false);
            
            var func = consumer.CreateJobFactoryForTest<Customer>("subscriptionName", (c, e) => { eventArgs = e; are.Set(); });
            var basicProperties = Substitute.For<IBasicProperties>();
            basicProperties.Type.Returns(Global.DefaultTypeNameSerializer.Serialize(typeof(Customer)));

            // Action
            func(new BasicDeliverEventArgs
            {
                BasicProperties = basicProperties,
                ConsumerTag = "ct",
                DeliveryTag = 1000,
            });

            // Assert
            are.WaitOne();
            Assert.AreEqual("ct", eventArgs.ConsumerTag);
            Assert.AreEqual((ulong)1000, eventArgs.DeliveryTag);
            Assert.AreEqual("subscriptionName", eventArgs.SubscriptionName);

        }
    }
}
// ReSharper restore InconsistentNaming