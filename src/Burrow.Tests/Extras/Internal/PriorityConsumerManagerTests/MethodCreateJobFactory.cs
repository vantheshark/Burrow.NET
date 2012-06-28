using System.Collections.Specialized;
using System.Threading;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.v0_9_1;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityConsumerManagerTests
{
    [TestClass]
    public class MethodCreateJobFactory
    {
        private readonly IRabbitWatcher _watcher = NSubstitute.Substitute.For<IRabbitWatcher>();
        private readonly IMessageHandlerFactory _handlerFactory = NSubstitute.Substitute.For<IMessageHandlerFactory>();
        private readonly ISerializer _serializer = NSubstitute.Substitute.For<ISerializer>();

        [TestMethod]
        public void Should_return_job_factory_that_can_parse_msg_priority()
        {
            // Arrange
            var waitHandler = new AutoResetEvent(false);
            var consumerManager = new PriorityConsumerManagerForTest(_watcher, _handlerFactory, _serializer);
            uint priority = 0;
            
            // Action
            var factory = consumerManager.CreateJobFactoryForTest<Customer>("BurrowUnitTest", (msg, evt) =>
            {
                waitHandler.Set();
                priority = evt.MessagePriority;
            });
            var task = factory(new BasicDeliverEventArgs
            {
                BasicProperties = new BasicProperties
                {
                    Type = Global.DefaultTypeNameSerializer.Serialize(typeof(Customer)),
                    Headers = new HybridDictionary { { "Priority", new[] { (byte)'1', (byte)'0' } } }
                }
            });
            task.Wait();
            waitHandler.WaitOne();
            
            // Assert
            Assert.AreEqual((uint)10, priority);
        }
    }
}
// ReSharper restore InconsistentNaming