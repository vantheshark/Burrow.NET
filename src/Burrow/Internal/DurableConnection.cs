using System;
using System.Linq;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Burrow.Internal
{
    /// <summary>
    /// A default implementation of <see cref="IDurableConnection"/>.
    /// It will try to create connection to RabbitMQ server and retry if the connection lost
    /// </summary>
    public class DurableConnection : IDurableConnection
    {
        internal DurableConnection(IRetryPolicy retryPolicy, IRabbitWatcher watcher)
        {
            if (retryPolicy == null)
            {
                throw new ArgumentNullException("retryPolicy");
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            _retryPolicy = retryPolicy;
            _watcher = watcher;
        }

        protected readonly IRetryPolicy _retryPolicy;
        protected readonly IRabbitWatcher _watcher;
        protected Action _unsubscribeEvents = () =>{};

        /// <summary>
        /// An event that will be fired if Connection established
        /// </summary>
        public event Action Connected;
        /// <summary>
        /// An event that will be fired if Connection lost
        /// </summary>
        public event Action Disconnected;

        /// <summary>
        /// Initialize a <see cref="DurableConnection"/> object
        /// </summary>
        /// <param name="retryPolicy"></param>
        /// <param name="watcher"></param>
        /// <param name="connectionFactory"></param>
        public DurableConnection(IRetryPolicy retryPolicy, IRabbitWatcher watcher, ConnectionFactory connectionFactory)
            : this(retryPolicy, watcher)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }

            
            _connectionFactory = ManagedConnectionFactory.CreateFromConnectionFactory(connectionFactory);
            ConnectionEstablished handler = (endpoint, virtualHost) =>
            {
                if (_connectionFactory.Endpoint + _connectionFactory.VirtualHost == endpoint + virtualHost)
                {
                    //NOTE: Fire connected event whenever a new connection to 1 of the servers in the cluster is made
                    FireConnectedEvent();
                }
            };
            ManagedConnectionFactory.ConnectionEstablished += handler;
            _unsubscribeEvents = () => { ManagedConnectionFactory.ConnectionEstablished -= handler; };
        }
        
        
        /// <summary>
        /// Try to connect to rabbitmq server, retry if it cann't connect to the broker.
        /// </summary>
        public virtual void Connect()
        {
            Monitor.Enter(ManagedConnectionFactory.SyncConnection);
            try
            {
                
                if (IsConnected || _retryPolicy.IsWaiting)
                {
                    return;
                }

                _watcher.DebugFormat("Trying to connect to endpoint: '{0}'", ConnectionFactory.Endpoint);
                var newConnection = ConnectionFactory.CreateConnection();
                newConnection.ConnectionShutdown += SharedConnectionShutdown;
                    
                _retryPolicy.Reset();
                _watcher.InfoFormat("Connected to RabbitMQ. Broker: '{0}', VHost: '{1}'", ConnectionFactory.Endpoint, ConnectionFactory.VirtualHost);
            }
            catch (ConnectFailureException connectFailureException)
            {
                HandleConnectionException(connectFailureException);
            }
            catch (BrokerUnreachableException brokerUnreachableException)
            {
                HandleConnectionException(brokerUnreachableException);
            }
            finally
            {
                Monitor.Exit(ManagedConnectionFactory.SyncConnection);
            }
        }

        private void HandleConnectionException(Exception ex)
        {
            _watcher.ErrorFormat("Failed to connect to Broker: '{0}', VHost: '{1}'. Retrying in {2} ms\n" +
                    "Check HostName, VirtualHost, Username and Password.\n" +
                    "ExceptionMessage: {3}",
                    ConnectionFactory.HostName,
                    ConnectionFactory.VirtualHost,
                    _retryPolicy.DelayTime,
                    ex.Message);

            _retryPolicy.WaitForNextRetry(Connect);
        }

        /// <summary>
        /// This should be called whenever a physical connection to rabbitMQ which has the same endpoint/virtual host is made
        /// </summary>
        protected void FireConnectedEvent()
        {
            if (Connected != null)
            {
                Connected();
            }
        }

        protected void FireDisconnectedEvent()
        {
            if (Disconnected != null)
            {
                Disconnected();
            }
        }

        protected void SharedConnectionShutdown(IConnection connection, ShutdownEventArgs reason)
        {
            FireDisconnectedEvent();
            _watcher.WarnFormat("Disconnected from RabbitMQ Broker '{0}': {1}", connection.Endpoint, reason != null ? reason.ReplyText : "");
            if (reason != null && reason.ReplyText != "Connection disposed by application" && reason.ReplyText != Subscription.CloseByApplication)
            {
                _retryPolicy.WaitForNextRetry(Connect);
            }
        }

        /// <summary>
        /// Determine whether it is an alive connection
        /// </summary>
        public bool IsConnected
        {
            get
            {
                var key = ConnectionFactory.Endpoint + ConnectionFactory.VirtualHost;
                if (!ManagedConnectionFactory.SharedConnections.ContainsKey(key))
                {
                    return false;
                }
                var c = ManagedConnectionFactory.SharedConnections[key];
                return c.IsOpen && c.Endpoint != null && ConnectionFactory.Endpoint.ToString().Equals(c.Endpoint.ToString());
            }
        }

        public string HostName
        {
            get { return ConnectionFactory.HostName; }
        }

        public string VirtualHost
        {
            get { return ConnectionFactory.VirtualHost; }
        }

        public string UserName
        {
            get { return ConnectionFactory.UserName; }
        }

        /// <summary>
        /// Return current ConnectionFactory
        /// </summary>
        internal protected virtual ConnectionFactory ConnectionFactory
        {
            get { return _connectionFactory; }
        }
        private readonly ConnectionFactory _connectionFactory;

        /// <summary>
        /// Create a RabbitMQ channel
        /// </summary>
        /// <returns></returns>
        public IModel CreateChannel()
        {
            if (!IsConnected)
            {
                Connect();
            }
            
            if (!IsConnected)
            {
                throw new BrokerUnreachableException(new Exception("Cannot connect to Rabbit server."));
            }

            var connection = ManagedConnectionFactory.SharedConnections[ConnectionFactory.Endpoint + ConnectionFactory.VirtualHost];
            var channel = connection.CreateModel();
            return channel;
        }
 
        public void Dispose()
        {
            _unsubscribeEvents();
            //Should not dispose any connections here since other tunnel might use one of them
            //try
            //{
            //    CloseAllConnections();
            //}
            //catch (Exception ex)
            //{
            //    _watcher.Error(ex);
            //}
        }
    }
}
