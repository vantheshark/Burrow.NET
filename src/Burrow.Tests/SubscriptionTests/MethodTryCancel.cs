using System.IO;

using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.SubscriptionTests
{
    [TestFixture]
    public class MethodTryCancel
    {
        [Test]
        public void Should_catch_AlreadyClosedException_and_IOException()
        {
            var channel = Substitute.For<IModel>();
            channel.IsOpen.Returns(true);
            var subscription = new Subscription(channel)
                                   {
                                       ConsumerTag = "ConsumerTag"
                                   };

            // Action
            subscription.TryCancel(m => { throw new AlreadyClosedException(new ShutdownEventArgs(ShutdownInitiator.Application, 0, "")); }, null, Substitute.For<IRabbitWatcher>());
            subscription.TryCancel(m => { throw new IOException(); }, null, Substitute.For<IRabbitWatcher>());
        }
    }
}
// ReSharper restore InconsistentNaming