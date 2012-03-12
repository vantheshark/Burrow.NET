using System;
using System.Configuration;
using Burrow.Internal;

namespace Burrow
{
    public static class TunnelFactory
    {
        public static ITunnel Create()
        {
            var rabbitConnectionString = ConfigurationManager.ConnectionStrings["RabbitMQ"];
            if (rabbitConnectionString == null)
            {
                throw new Exception(
                    "Could not find a connection string for RabbitMQ. " +
                    "Please add a connection string in the <ConnectionStrings> secion" +
                    "of the application's configuration file. For example: " +
                    "<add name=\"RabbitMQ\" connectionString=\"host=localhost\" />");
            }

            return Create(rabbitConnectionString.ConnectionString);
        }

        public static ITunnel Create(string connectionString)
        {
            return Create(connectionString, new ConsoleWatcher());
        }

        public static ITunnel Create(string connectionString, IRabbitWatcher watcher)
        {
            var connectionValues = new ConnectionString(connectionString);

            return Create(connectionValues.Host,
                          connectionValues.VirtualHost,
                          connectionValues.UserName,
                          connectionValues.Password,
                          watcher);
        }

        public static ITunnel Create(string hostName, string virtualHost, string username, string password, IRabbitWatcher watcher)
        {
            var connectionFactory = new RabbitMQ.Client.ConnectionFactory
                                        {
                                            HostName = hostName,
                                            VirtualHost = virtualHost,
                                            UserName = username,
                                            Password = password
                                        };

            var durableConnection = new DurableConnection(new DefaultIRetryPolicy(), watcher, connectionFactory);
            var errorHandler = new ConsumerErrorHandler(connectionFactory, Global.DefaultSerializer, Global.DefaultWatcher);
            var consumerManager = new ConsumerManager(watcher, errorHandler, Global.DefaultSerializer, Global.DefaultConsumerBatchSize);

            return new RabbitTunnel(consumerManager, 
                                    watcher, 
                                    new DefaultRouteFinder(), 
                                    durableConnection,
                                    Global.DefaultSerializer,
                                    Global.DefaultCorrelationIdGenerator,
                                    Global.SetDefaultPersistentMode);

        }
    }
}
