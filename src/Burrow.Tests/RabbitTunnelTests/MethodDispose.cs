using System;
using System.IO;
using System.Threading;
using Burrow.Tests.Extras.RabbitSetupTests;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestFixture]
    public class MethodDispose
    {
        [Test]
        public void Should_dispose_everything()
        {
            // Arrange
            var countDownEvent = new CountdownEvent(4);
            var newChannel = Substitute.For<IModel>();
            newChannel.When(x => x.Dispose()).Do(callInfo => countDownEvent.Signal());
            newChannel.When(x => x.Abort()).Do(callInfo => countDownEvent.Signal());

            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);
            tunnel.Subscribe<Customer>("subscriptionName", x => { });

            // Action
            tunnel.Dispose();
            Assert.IsTrue(countDownEvent.Wait(1000));

            // Assert
            // One for the dedicated publish channel and the other for the above subcribe channel
            newChannel.Received(2).Dispose();
            newChannel.Received(2).Abort();
            durableConnection.Received(1).Dispose();
        }

        [Test]
        public void Should_not_throw_IOException()
        {
            // Arrange
            var countDownEvent = new CountdownEvent(2);
            var newChannel = Substitute.For<IModel>();
            newChannel.When(x => x.Abort()).Do(callInfo =>
            {
                countDownEvent.Signal(); 
                throw new IOException(); 
            });
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);
            tunnel.Subscribe<Customer>("subscriptionName", x => { });

            // Action
            tunnel.Dispose();
            Assert.IsTrue(countDownEvent.Wait(1000));

            // Assert
            durableConnection.Received(1).Dispose();
        }

        [Test]
        public void Should_not_throw_any_other_Exception()
        {
            // Arrange
            var countDownEvent = new CountdownEvent(2);
            var newChannel = Substitute.For<IModel>();
            newChannel.When(x => x.Abort()).Do(callInfo =>
            {
                countDownEvent.Signal(); 
                throw new Exception();
            });
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);
            tunnel.Subscribe<Customer>("subscriptionName", x => { });

            // Action
            tunnel.Dispose();
            Assert.IsTrue(countDownEvent.Wait(1000));

            // Assert
            durableConnection.Received(1).Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming