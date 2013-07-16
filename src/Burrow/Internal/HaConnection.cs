using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Burrow.Internal
{
    public class HaConnection : DurableConnection
    {
        private readonly IRetryPolicy _retryPolicy;
        private readonly IRabbitWatcher _watcher;
        private readonly RoundRobinList<ConnectionFactory> _connectionFactories;
        private int _nodeTried;

        public HaConnection(IRetryPolicy retryPolicy, IRabbitWatcher watcher, IList<ManagedConnectionFactory> connectionFactories)
            : base(retryPolicy, watcher, connectionFactories.FirstOrDefault())
        {            
            _connectionFactories = new RoundRobinList<ConnectionFactory>(connectionFactories);
            _retryPolicy = retryPolicy;
            _watcher = watcher;
        }

        public override ConnectionFactory ConnectionFactory
        {
            get { return _connectionFactories.Current; }
        }

        public override void Connect()
        {
            try
            {
                Monitor.Enter(ManagedConnectionFactory.SyncConnection);
                if (IsConnected || _retryPolicy.IsWaiting)
                {
                    return;
                }

                _watcher.DebugFormat("Trying to connect to endpoint: '{0}'", ConnectionFactory.Endpoint);
                var newConnection = ConnectionFactory.CreateConnection();
                newConnection.ConnectionShutdown += SharedConnectionShutdown;
                    
                FireConnectedEvent();
                _retryPolicy.Reset();
                _nodeTried = 0;
                _watcher.InfoFormat("Connected to RabbitMQ. Broker: '{0}', VHost: '{1}'", ConnectionFactory.Endpoint, ConnectionFactory.VirtualHost);
            }
            catch(ConnectFailureException connectFailureException)
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
            var oldFactory = ConnectionFactory;
            var nextFactory = _connectionFactories.GetNext();
            if (++_nodeTried < _connectionFactories.All.Count())
            {
                _watcher.ErrorFormat("Failed to connect to Broker: '{0}:{1}', VHost: '{2}'. Failing over to '{3}:{4}'\nExceptionMessage: {5}",
                                     oldFactory.HostName,
                                     oldFactory.Port,
                                     oldFactory.VirtualHost,
                                     nextFactory.HostName,
                                     nextFactory.Port,
                                     ex.Message);
                Connect();
            }
            else
            {
                _watcher.ErrorFormat("Failed to connect to Broker: '{0}:{1}', VHost: '{2}'. Failing over to '{3}{4}' after {5}ms\nExceptionMessage: {6}",
                                     oldFactory.HostName,
                                     oldFactory.Port,
                                     oldFactory.VirtualHost,
                                     nextFactory.HostName,
                                     nextFactory.Port,
                                     _retryPolicy.DelayTime,
                                     ex.Message);
                _retryPolicy.WaitForNextRetry(Connect);
            }
        }

        // For Unit tests >.<
        internal RoundRobinList<ConnectionFactory> ConnectionFactories
        {
            get { return _connectionFactories; }
        }
    }
}