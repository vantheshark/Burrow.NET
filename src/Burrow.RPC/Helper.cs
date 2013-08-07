using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Burrow.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Burrow.RPC
{
    /// <summary>
    /// Helper interface to create queues and resolve rabbitmq connection string
    /// </summary>
    public interface IRpcQueueHelper
    {
        /// <summary>
        /// Create queues required by Burrow.RPC library
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="createExchangesAndQueues"></param>
        void CreateQueues(string connectionString, Action<IModel> createExchangesAndQueues);

        /// <summary>
        /// Try to get connection string from configuration file if not provided
        /// </summary>
        /// <param name="preferConnectionString">If provided, it will be used</param>
        /// <returns></returns>
        string TryGetValidConnectionString(string preferConnectionString);
    }

    [ExcludeFromCodeCoverage]
    internal class Helper : IRpcQueueHelper
    {
        /// <summary>
        /// Create queues required by Burrow.RPC library
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="createExchangesAndQueues"></param>
        public void CreateQueues(string connectionString, Action<IModel> createExchangesAndQueues)
        {
            var clusterConnections = connectionString.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (clusterConnections.Length > 1)
            {
                Global.DefaultWatcher.InfoFormat("Found multiple Connection String, will use '{0}' to setup queues", clusterConnections[0]);
            }
            ConnectionString connectionValues = clusterConnections.Length > 1
                                              ? new ConnectionString(clusterConnections[0])
                                              : new ConnectionString(connectionString);


            var connectionFactory = new ManagedConnectionFactory
            {
                HostName = connectionValues.Host,
                Port = connectionValues.Port,
                VirtualHost = connectionValues.VirtualHost,
                UserName = connectionValues.UserName,
                Password = connectionValues.Password,
            };

            using (var connection = connectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    try
                    {
                        createExchangesAndQueues(model);
                    }
                    catch (OperationInterruptedException oie)
                    {
                        if (oie.ShutdownReason.ReplyText.StartsWith("PRECONDITION_FAILED - "))
                        {
                            Global.DefaultWatcher.ErrorFormat(oie.ShutdownReason.ReplyText);
                        }
                        else
                        {
                            Global.DefaultWatcher.Error(oie);
                        }
                    }
                    catch (Exception ex)
                    {
                        Global.DefaultWatcher.Error(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Try to get connection string from configuration file if not provided
        /// </summary>
        /// <param name="preferConnectionString">If provided, it will be used</param>
        /// <returns></returns>
        public string TryGetValidConnectionString(string preferConnectionString)
        {
            var rabbitMqConnectionString = preferConnectionString;
            var connectionSetting = ConfigurationManager.ConnectionStrings["RabbitMQ"];
            if (string.IsNullOrEmpty(rabbitMqConnectionString))
            {
                rabbitMqConnectionString = connectionSetting != null
                                         ? connectionSetting.ConnectionString
                                         : null;

                if (string.IsNullOrEmpty(rabbitMqConnectionString))
                {
                    throw new ArgumentException("Could not find any available RabbitMQ connectionstring. Check your app.config or provide a valid connection string", rabbitMqConnectionString);
                }
            }
            return rabbitMqConnectionString;
        }
    }
}
