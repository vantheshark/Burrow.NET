using System;
using RabbitMQ.Client;

namespace Burrow
{
    /// <summary>
    /// This interface provides properties, methods and events about the connections. 
    /// </summary>
    public interface IDurableConnection : IDisposable
    {
        /// <summary>
        /// A event fired when a connection to rabbitmq server is established
        /// </summary>
        event Action Connected;

        /// A event fired when a connection to rabbitmq server is disconnected
        event Action Disconnected;
        
        /// <summary>
        /// Determine whether there is a connection to rabbitmq server
        /// </summary>
        
        bool IsConnected { get; }

        /// <summary>
        /// Get the current connected host name
        /// </summary>
        string HostName { get; }

        /// <summary>
        /// Get the current connected virtual host
        /// </summary>
        string VirtualHost { get; }

        /// <summary>
        /// Get the current connected username
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Connect to rabbitmq server
        /// </summary>
        
        void Connect();
        /// <summary>
        /// Create a rabbitmq channel which then can be used to publish/subscribe
        /// </summary>
        /// <returns></returns>
        
        IModel CreateChannel();
    }
}
