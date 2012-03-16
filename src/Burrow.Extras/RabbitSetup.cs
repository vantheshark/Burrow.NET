using System;
using System.Collections;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Burrow.Extras
{
    public class RabbitSetup
    {
        private readonly Func<string, string, IRouteFinder> _routeFinderFactory;
        private readonly IRabbitWatcher _watcher;
        private readonly string _environment;
        private readonly ConnectionFactory _connectionFactory;

        /// <summary>
        /// Initialize an instance of RabbitSetup class to use for setting up exchanges and queues in RabbitMQ server
        /// </summary>
        /// <param name="routeFinderFactory">a factory object to create an instance of routefinder by environment and subscription name</param>
        /// <param name="watcher"></param>
        /// <param name="connectionString">RabbitMQ connection string</param>
        /// <param name="environment">TEST, DEV, PROD or anything that can be involved when generating the exchange and queue names</param>
        public RabbitSetup(Func<string, string, IRouteFinder> routeFinderFactory, IRabbitWatcher watcher, string connectionString, string environment)
        {
            _routeFinderFactory = routeFinderFactory;
            _watcher = watcher;
            _environment = environment;

            var connectionValues = new ConnectionString(connectionString);
            _connectionFactory = new ConnectionFactory
            {
                HostName = connectionValues.Host,
                VirtualHost = connectionValues.VirtualHost,
                UserName = connectionValues.UserName,
                Password = connectionValues.Password,
            };
        }

        public void SetupExchangeAndQueueFor<T>(ExchangeSetupData exchange, QueueSetupData queue)
        {
            var routeFinder = _routeFinderFactory(_environment, exchange.ExchangeType);
            var queueName = routeFinder.FindQueueName<T>(queue.SubscriptionName);
            var exchangeName = routeFinder.FindExchangeName<T>();
            var routingKey = routeFinder.FindRoutingKey<T>();

            using (var connection = _connectionFactory.CreateConnection())
            using (var model = connection.CreateModel())
            {
                // Declare Exchange
                try
                {
                    model.ExchangeDeclare(exchangeName, exchange.ExchangeType, exchange.Durable, exchange.AutoDelete, null);
                }
                catch (Exception ex)
                {
                    _watcher.Error(ex);
                }

                // Declare Queue
                try
                {
                    IDictionary arguments = new Dictionary<string, object>();
                    if (queue.MessageTimeToLive > 0)
                    {
                        arguments.Add("x-message-ttl", queue.MessageTimeToLive);
                    }
                    model.QueueDeclare(queueName, queue.Durable, false, queue.AutoDelete, arguments);
                }
                catch (Exception ex)
                {
                    _watcher.Error(ex);
                }


                // Bind Queue to Exchange
                try
                {
                    model.QueueBind(queueName, exchangeName, routingKey);
                }
                catch (Exception ex)
                {
                    _watcher.Error(ex);
                }
            }
        }

        public void Destroy<T>(ExchangeSetupData exchange, QueueSetupData queue)
        {
            var conventions = _routeFinderFactory(_environment, exchange.ExchangeType);
            var queueName = conventions.FindQueueName<T>(queue.SubscriptionName);
            var exchangeName = conventions.FindExchangeName<T>();

            using (var connection = _connectionFactory.CreateConnection())
            using (var model = connection.CreateModel())
            {
                // Delete Queue
                try
                {
                    model.QueueDelete(queueName);
                }
                catch (Exception ex)
                {
                    _watcher.Error(ex);
                }

                // Delete Exchange
                try
                {
                    model.ExchangeDelete(exchangeName);
                }
                catch (Exception ex)
                {
                    _watcher.Error(ex);
                }
            }
        }
    }
}
