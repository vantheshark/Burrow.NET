using System;
using System.Collections;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Burrow.Extras
{
    public class RabbitSetup
    {
        protected readonly Func<string, string, IRouteFinder> _routeFinderFactory;
        protected readonly IRabbitWatcher _watcher;
        protected readonly string _environment;
        protected ConnectionFactory _connectionFactory;

        /// <summary>
        /// Initialize an instance of RabbitSetup class to use for setting up exchanges and queues in RabbitMQ server
        /// </summary>
        /// <param name="routeFinderFactory">a factory object to create an instance of routefinder for provided ENVIRONMENT and EXCHANGE type</param>
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
            var routingKey = queue.RoutingKey ?? routeFinder.FindRoutingKey<T>();

            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    // Declare Exchange
                    DeclareExchange(exchange, model, exchangeName);
                }
                using (var model = connection.CreateModel())
                {
                    // Declare Queue
                    DeclareQueue(queue, queueName, model);
                }
                using (var model = connection.CreateModel())
                {
                    // Bind Queue to Exchange
                    BindQueue(model, queue, exchangeName, queueName, routingKey);
                }
            }
        }

        protected virtual void BindQueue(IModel model, QueueSetupData queue, string exchangeName, string queueName, string routingKey)
        {
            try
            {
                model.QueueBind(queueName, exchangeName, routingKey);
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
        }

        protected virtual void DeclareQueue(QueueSetupData queue, string queueName, IModel model)
        {
            try
            {
                IDictionary arguments = new Dictionary<string, object>();
                if (queue.MessageTimeToLive > 0)
                {
                    arguments.Add("x-message-ttl", queue.MessageTimeToLive);
                }
                if (queue.AutoExpire > 0)
                {
                    arguments.Add("x-expires", queue.AutoExpire);
                }
                model.QueueDeclare(queueName, queue.Durable, false, queue.AutoDelete, arguments);
            }
            catch (OperationInterruptedException oie)
            {
                if (oie.ShutdownReason.ReplyText.StartsWith("PRECONDITION_FAILED - "))
                {
                    _watcher.ErrorFormat(oie.ShutdownReason.ReplyText);
                }
                else
                {
                    _watcher.Error(oie);
                }
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
        }

        protected virtual void DeclareExchange(ExchangeSetupData exchange, IModel model, string exchangeName)
        {
            try
            {
                model.ExchangeDeclare(exchangeName, exchange.ExchangeType, exchange.Durable, exchange.AutoDelete, null);
            }
            catch (OperationInterruptedException oie)
            {
                if (oie.ShutdownReason.ReplyText.StartsWith("PRECONDITION_FAILED - "))
                {
                    _watcher.ErrorFormat(oie.ShutdownReason.ReplyText);
                }
                else
                {
                    _watcher.Error(oie);
                }
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
        }

        public virtual void Destroy<T>(ExchangeSetupData exchange, QueueSetupData queue)
        {
            var conventions = _routeFinderFactory(_environment, exchange.ExchangeType);
            var queueName = conventions.FindQueueName<T>(queue.SubscriptionName);
            var exchangeName = conventions.FindExchangeName<T>();

            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    // Delete Queue
                    DeleteQueue(model, queue, queueName);
                }

                using (var model = connection.CreateModel())
                {
                    // Delete Exchange
                    try
                    {
                        model.ExchangeDelete(exchangeName);
                    }
                    catch (OperationInterruptedException oie)
                    {
                        if (oie.ShutdownReason.ReplyText.StartsWith("NOT_FOUND - no exchange "))
                        {
                            _watcher.WarnFormat(oie.ShutdownReason.ReplyText);
                        }
                        else
                        {
                            _watcher.Error(oie);
                        }
                    }
                    catch (Exception ex)
                    {
                        _watcher.Error(ex);
                    }
                }
            }
        }

        protected virtual void DeleteQueue(IModel model, QueueSetupData queue, string queueName)
        {
            try
            {
                model.QueueDelete(queueName);
            }
            catch(OperationInterruptedException oie)
            {
                if (oie.ShutdownReason.ReplyText.StartsWith("NOT_FOUND - no queue "))
                {
                    _watcher.WarnFormat(oie.ShutdownReason.ReplyText);
                }
                else
                {
                    _watcher.Error(oie);
                }
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
        }
    }
}
