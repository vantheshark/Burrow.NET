using System;
using System.Threading.Tasks;
using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client.Events;

namespace Burrow.Tests.Extras.Internal.PriorityMessageHandlerFactoryTests
{
    [TestClass]
    public class MethodCreate
    {
        [TestMethod]
        public void Should_return_PriorityMessageHandler()
        {
            // Arrange
            var factory = new PriorityMessageHandlerFactory(NSubstitute.Substitute.For<IConsumerErrorHandler>(),
                                                            NSubstitute.Substitute.For<IRabbitWatcher>());

            // Action
            var handler = factory.Create(NSubstitute.Substitute.For<Func<BasicDeliverEventArgs, Task>>());

            // Assert
            Assert.IsInstanceOfType(handler, typeof(PriorityMessageHandler));
        }
    }
}
