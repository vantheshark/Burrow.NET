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
        void CreateQueues(string connectionString, Action<IModel> createExchangesAndQueues);
        string TryGetValidConnectionString(string providedConnectionString);
    }

    [ExcludeFromCodeCoverage]
    internal class Helper : IRpcQueueHelper
    {
        public void CreateQueues(string connectionString, Action<IModel> createExchangesAndQueues)
        {
            var connectionValues = new ConnectionString(connectionString);
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

        public string TryGetValidConnectionString(string providedConnectionString)
        {
            var rabbitMqConnectionString = providedConnectionString;
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
