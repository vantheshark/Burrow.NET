using System.Linq;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

namespace Burrow.Tests.Internal.DurableConnectionTests
{
    public class DurableConnectionTestHelper
    {
        [TestCleanup]
        public void CleanUp()
        {
            DurableConnection.SharedConnections.Values.ToList().ForEach(c =>
            {
                try
                {
                    c.Close(200, "Connection disposed by application");
                    c.Dispose();
                }
                catch
                {
                }
            });
            DurableConnection.SharedConnections.Clear();
            
        }

        public static ConnectionFactory CreateMockConnectionFactory(string virtualHost, AmqpTcpEndpoint endpoint = null)
        {
            IConnection connection;
            return CreateMockConnectionFactory(virtualHost, out connection, endpoint);
        }

        public static ConnectionFactory CreateMockConnectionFactory(string virtualHost, out IConnection connection, AmqpTcpEndpoint endpoint = null)
        {
            connection = Substitute.For<IConnection>();

            var connectionFactory = Substitute.For<ConnectionFactory>();
            connectionFactory.VirtualHost = virtualHost;
            connectionFactory.CreateConnection().Returns(connection);
            if (endpoint != null)
            {
                connectionFactory.Endpoint = endpoint;
            }
            connection.Endpoint.Returns(connectionFactory.Endpoint);
            connection.IsOpen.Returns(true);

            return connectionFactory;
        }
    }
}
