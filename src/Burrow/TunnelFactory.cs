using System;
using System.Configuration;
using Burrow.Internal;

namespace Burrow
{
    /// <summary>
    /// This class is responsible for creating ITunnel.
    /// Any derived of this class will automatically registered itself as the default TunnelFactory in the library ;)
    /// </summary>
    public class TunnelFactory 
    {
        public TunnelFactory() : this(true)
        {
        }

        internal TunnelFactory(bool setAsDefault)
        {
            if (setAsDefault)
            {
                RabbitTunnel.Factory = this;
            }
        }

        public void CloseAllConnections()
        {
            DurableConnection.CloseAllConnections();
        }

        public virtual ITunnel Create()
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

        public virtual ITunnel Create(string connectionString)
        {
            return Create(connectionString, Global.DefaultWatcher ?? new ConsoleWatcher());
        }

        public virtual ITunnel Create(string connectionString, IRabbitWatcher watcher)
        {
            var connectionValues = new ConnectionString(connectionString);

            return Create(connectionValues.Host,
                          connectionValues.VirtualHost,
                          connectionValues.UserName,
                          connectionValues.Password,
                          watcher);
        }

        public virtual ITunnel Create(string hostName, string virtualHost, string username, string password, IRabbitWatcher watcher)
        {
            var rabbitWatcher = watcher ?? Global.DefaultWatcher;
            var connectionFactory = new RabbitMQ.Client.ConnectionFactory
                                        {
                                            HostName = hostName,
                                            VirtualHost = virtualHost,
                                            UserName = username,
                                            Password = password
                                        };

            var durableConnection = new DurableConnection(new DefaultRetryPolicy(), rabbitWatcher, connectionFactory);
            var errorHandler = new ConsumerErrorHandler(connectionFactory, Global.DefaultSerializer, rabbitWatcher);
            var msgHandlerFactory = new DefaultMessageHandlerFactory(errorHandler, rabbitWatcher);
            var consumerManager = new ConsumerManager(rabbitWatcher, msgHandlerFactory, Global.DefaultSerializer, Global.DefaultConsumerBatchSize);

            return new RabbitTunnel(consumerManager,
                                    rabbitWatcher, 
                                    new DefaultRouteFinder(), 
                                    durableConnection,
                                    Global.DefaultSerializer,
                                    Global.DefaultCorrelationIdGenerator,
                                    Global.DefaultPersistentMode);

        }
    }
}
