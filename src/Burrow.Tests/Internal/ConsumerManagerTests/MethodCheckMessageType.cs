using System;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.ConsumerManagerTests
{
    [TestClass]
    public class MethodCheckMessageType
    {
        [TestMethod, ExpectedException(typeof(Exception))]
        public void Should_throw_exception_if_mesage_type_not_match()
        {
            // Arrange
            var consumer = new ConsumerManagerForTest(Substitute.For<IRabbitWatcher>(), Substitute.For<IMessageHandlerFactory>(),
                                                      Substitute.For<ISerializer>(), 10);
            
            var basicProperties = Substitute.For<IBasicProperties>();
            basicProperties.Type.Returns("Invalid_type");
            
            // Action
            consumer.CheckMessageTypeForTest<Customer>(basicProperties);
        }

        
    }
}
// ReSharper restore InconsistentNaming