using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Burrow.Internal
{
    public class DurableConnection : IDurableConnection
    {
        //One AppDomain should create only 1 connection to server except connect to different virtual hosts
        internal static volatile Dictionary<string, IConnection> SharedConnections = new Dictionary<string, IConnection>();

        private readonly IRetryPolicy _retryPolicy;
        private readonly IRabbitWatcher _watcher;
        private static readonly object _syncConnection = new object();
        public event Action Connected;
        public event Action Disconnected;

        public DurableConnection(IRetryPolicy retryPolicy, IRabbitWatcher watcher, ConnectionFactory connectionFactory)
        {
            if (retryPolicy == null)
            {
                throw new ArgumentNullException("retryPolicy");
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }

            _retryPolicy = retryPolicy;
            _watcher = watcher;
            ConnectionFactory = connectionFactory;
        }

        public void Connect()
        {
            try
            {
                lock (_syncConnection)
                {
                    if (IsConnected || _retryPolicy.IsWaiting)
                    {
                        return;
                    }

                    _watcher.DebugFormat("Trying to connect to endpoint: {0}", ConnectionFactory.Endpoint);
                    var newConnection = ConnectionFactory.CreateConnection();
                    newConnection.ConnectionShutdown += SharedConnectionShutdown;
                    //Console.WriteLine(ConnectionFactory.Endpoint + ConnectionFactory.VirtualHost);
                    SharedConnections.Add(ConnectionFactory.Endpoint + ConnectionFactory.VirtualHost, newConnection);
                    if (Connected != null)
                    {
                        Connected();
                    }

                    _retryPolicy.Reset();
                    _watcher.InfoFormat("Connected to RabbitMQ. Broker: '{0}', VHost: '{1}'", ConnectionFactory.Endpoint, ConnectionFactory.VirtualHost);
                }
            }
            catch (BrokerUnreachableException brokerUnreachableException)
            {
                _watcher.ErrorFormat("Failed to connect to Broker: '{0}', VHost: '{1}'. Retrying in {2} ms\n" +
                    "Check HostName, VirtualHost, Username and Password.\n" +
                    "ExceptionMessage: {3}",
                    ConnectionFactory.HostName,
                    ConnectionFactory.VirtualHost,
                    _retryPolicy.DelayTime,
                    brokerUnreachableException.Message);

                _retryPolicy.WaitForNextRetry(Connect);
            }
        }

        private void SharedConnectionShutdown(IConnection connection, ShutdownEventArgs reason)
        {
            if (Disconnected != null) Disconnected();

            _watcher.WarnFormat("Disconnected from RabbitMQ Broker");

            foreach(var c in SharedConnections)
            {
                if (c.Key == ConnectionFactory.Endpoint + ConnectionFactory.VirtualHost)
                {
                    SharedConnections.Remove(c.Key);
                    break;
                }
            }

            if (reason != null && reason.ReplyText != "Connection disposed by application" && reason.ReplyText != Subscription.CloseByApplication)
            {
                _retryPolicy.WaitForNextRetry(Connect);
            }
        }

        public bool IsConnected
        {
            get 
            { 
                return SharedConnections.Any(c => c.Key == ConnectionFactory.Endpoint + ConnectionFactory.VirtualHost 
                                               && ConnectionFactory.Endpoint.ToString().Equals(c.Value.Endpoint.ToString())
                                               && c.Value != null 
                                               && c.Value.IsOpen); 
            }
        }

        public ConnectionFactory ConnectionFactory { get; protected set; }

        public IModel CreateChannel()
        {
            if (!IsConnected)
            {
                Connect();
            }
            
            if (!IsConnected)
            {
                throw new Exception("Cannot connect to Rabbit server.");
            }

            var connection = SharedConnections[ConnectionFactory.Endpoint + ConnectionFactory.VirtualHost];
            var channel = connection.CreateModel();
            return channel;
        }
 
        public void Dispose()
        {
            //Should not dispose connection here since other tunnel might use it
            //try
            //{
            //    CloseAllConnections();
            //}
            //catch (Exception ex)
            //{
            //    _watcher.Error(ex);
            //}
        }

        internal static void CloseAllConnections()
        {
            SharedConnections.Values.ToList().ForEach(c =>
            {
                c.Close(200, "Connection disposed by application");
                c.Dispose();
            });
            SharedConnections.Clear();
        }
    }
}
