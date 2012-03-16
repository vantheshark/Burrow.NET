using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Burrow.Internal
{
    public class DurableConnection : IDurableConnection
    {
        private readonly IRetryPolicy _retryPolicy;
        private readonly IRabbitWatcher _watcher;
        private IConnection _connection;

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
                _watcher.DebugFormat("Trying to connect");
                _connection = ConnectionFactory.CreateConnection();
                _connection.ConnectionShutdown += ConnectionShutdown;

                if (Connected != null)
                {
                    Connected();
                }

                _retryPolicy.Reset();
                _watcher.InfoFormat("Connected to RabbitMQ. Broker: '{0}', VHost: '{1}'", ConnectionFactory.HostName, ConnectionFactory.VirtualHost);
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

        private void ConnectionShutdown(IConnection connection, ShutdownEventArgs reason)
        {
            if (Disconnected != null) Disconnected();

            _watcher.WarnFormat("Disconnected from RabbitMQ Broker");

            _retryPolicy.WaitForNextRetry(Connect);
        }

        public bool IsConnected
        {
            get { return _connection != null && _connection.IsOpen; }
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
            
            var channel = _connection.CreateModel();
            channel.ModelShutdown += ChannelShutdown;
            return channel;
        }

        private void ChannelShutdown(IModel model, ShutdownEventArgs reason)
        {
            _watcher.WarnFormat("Channel shutdown: {0}", reason.ReplyText);
        }

        public void Dispose()
        {
            try
            {
                _connection.Dispose();
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
        }
    }
}
