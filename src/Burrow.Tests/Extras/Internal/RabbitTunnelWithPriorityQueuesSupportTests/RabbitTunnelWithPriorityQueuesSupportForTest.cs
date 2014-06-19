using System;
using Burrow.Extras.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using NSubstitute;
using RabbitMQ.Client;

namespace Burrow.Tests.Extras.Internal.RabbitTunnelWithPriorityQueuesSupportTests
{
    internal class RabbitTunnelWithPriorityQueuesSupportForTest : RabbitTunnelWithPriorityQueuesSupport
    {
        public RabbitTunnelWithPriorityQueuesSupportForTest(IRouteFinder routeFinder, IDurableConnection connection) 
            : base(routeFinder, connection)
        {
        }

        public RabbitTunnelWithPriorityQueuesSupportForTest(IConsumerManager consumerManager, IRabbitWatcher watcher, IRouteFinder routeFinder, IDurableConnection connection, ISerializer serializer, ICorrelationIdGenerator correlationIdGenerator, bool setPersistent)
            : base(consumerManager, watcher, routeFinder, connection, serializer, correlationIdGenerator, setPersistent)
        {
        }

        public static RabbitTunnelWithPriorityQueuesSupport CreateTunnel(IModel channel, out IDurableConnection durableConnection, bool isChannelOpen = true)
        {
            if (channel != null)
            {
                channel.IsOpen.Returns(isChannelOpen);
            }

            var routeFinder = Substitute.For<IRouteFinder>();
            routeFinder.FindQueueName<Customer>(null).ReturnsForAnyArgs("Queue");
            durableConnection = Substitute.For<IDurableConnection>();
            //durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            var conn = durableConnection;
            durableConnection.When(x => x.Connect()).Do(callInfo => // Because this is a mock objectd
            {
                //Rase connected event
                conn.Connected += Raise.Event<Action>();
                conn.IsConnected.Returns(true);
            });
            durableConnection.CreateChannel().Returns(channel);

            var tunnel = new RabbitTunnelWithPriorityQueuesSupport(routeFinder, durableConnection);
            tunnel.OnOpened += () => { };
            tunnel.OnClosed += () => { };
            return tunnel;
        }
    }
}