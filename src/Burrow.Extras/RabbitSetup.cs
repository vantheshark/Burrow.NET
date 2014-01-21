using System;
using System.Collections;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Burrow.Extras
{
    /// <summary>
    /// A helper class to create/destroy Exchange/Queue
    /// </summary>
    public class RabbitSetup
    {
        protected readonly IRabbitWatcher _watcher;
        private readonly string _connectionString;
        private ConnectionFactory _factory;
        protected ConnectionFactory ConnectionFactory
        {
            get
            {
                if (_factory == null)
                {
                    _factory = CreateFactory();
                }
                return _factory;
            }
            set { _factory = value; }
        }

        /// <summary>
        /// Initialize an instance of RabbitSetup class to use for setting up exchanges and queues in RabbitMQ server
        /// </summary>
        /// <param name="connectionString"></param>
        public RabbitSetup(string connectionString)
            : this(Global.DefaultWatcher, connectionString)
        {
        }

        /// <summary>
        /// Initialize an instance of RabbitSetup class to use for setting up exchanges and queues in RabbitMQ server
        /// </summary>
        /// <param name="watcher"></param>
        /// <param name="connectionString">RabbitMQ connection string</param>
        public RabbitSetup(IRabbitWatcher watcher, string connectionString)
        {
            _watcher = watcher;
            _connectionString = connectionString;
        }

        private ConnectionFactory CreateFactory()
        {
            if (_factory == null)
            {
                var clusterConnections = _connectionString.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (clusterConnections.Length > 0)
                {
                    _watcher.InfoFormat("Found multiple Connection String, will use '{0}' to setup queues", clusterConnections[0]);
                }
                ConnectionString connectionValues = clusterConnections.Length > 1
                                                        ? new ConnectionString(clusterConnections[0])
                                                        : new ConnectionString(_connectionString);
                _factory = new ConnectionFactory
                {
                    HostName = connectionValues.Host,
                    Port = connectionValues.Port,
                    VirtualHost = connectionValues.VirtualHost,
                    UserName = connectionValues.UserName,
                    Password = connectionValues.Password,
                };
            }
            return ConnectionFactory;
        }

        /// <summary>
        /// Create Exchange and queue using RouteSetupData
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="routeSetupData"></param>
        public void CreateRoute<T>(RouteSetupData routeSetupData)
        {
            var queueName = routeSetupData.RouteFinder.FindQueueName<T>(routeSetupData.SubscriptionName);
            var exchangeName = routeSetupData.RouteFinder.FindExchangeName<T>();
            var routingKey = routeSetupData.RouteFinder.FindRoutingKey<T>();

            using (var connection = ConnectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    // Declare Exchange
                    DeclareExchange(routeSetupData.ExchangeSetupData, model, exchangeName);
                }
                using (var model = connection.CreateModel())
                {
                    // Declare Queue
                    DeclareQueue<T>(routeSetupData.QueueSetupData, queueName, model);
                }
                using (var model = connection.CreateModel())
                {
                    // Bind Queue to Exchange
                    BindQueue<T>(model, routeSetupData.QueueSetupData, exchangeName, queueName, routingKey, routeSetupData.OptionalBindingData);
                }
            }
        }

        protected virtual void BindQueue<T>(IModel model, QueueSetupData queue, string exchangeName, string queueName, string routingKey, IDictionary<string, object> bindingData = null)
        {
            if (string.IsNullOrEmpty(exchangeName))
            {
                _watcher.WarnFormat("Attempt to bind queue {0} to a empty name Exchange, that's the default built-in exchange so the action will be ignored", queueName);
                return;
            }

            try
            {
                model.QueueBind(queueName, exchangeName, routingKey, bindingData);
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
        }

        protected virtual void DeclareQueue<T>(QueueSetupData queue, string queueName, IModel model)
        {
            try
            {
                SetQueueSetupArguments(queue);
                model.QueueDeclare(queueName, queue.Durable, false, queue.AutoDelete, queue.Arguments);
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

        protected void SetQueueSetupArguments(QueueSetupData queue)
        {
            if (queue.MessageTimeToLive > 0)
            {
                queue.Arguments["x-message-ttl"] = queue.MessageTimeToLive;
            }
            if (queue.AutoExpire > 0)
            {
                queue.Arguments["x-expires"] = queue.AutoExpire;
            }
            if (queue.DeadLetterExchange != null)
            {
                queue.Arguments["x-dead-letter-exchange"] = queue.DeadLetterExchange;
            }
            if (queue.DeadLetterRoutingKey != null)
            {
                queue.Arguments["x-dead-letter-routing-key"] = queue.DeadLetterRoutingKey;
            }
        }

        protected virtual void DeclareExchange(ExchangeSetupData exchange, IModel model, string exchangeName)
        {
            if (string.IsNullOrEmpty(exchangeName))
            {
                _watcher.WarnFormat("Attempt to declare a Exchange with empty string, that's the default built-in exchange so the action will be ignored");
                return;
            }
            try
            {
                model.ExchangeDeclare(exchangeName, exchange.ExchangeType, exchange.Durable, exchange.AutoDelete, exchange.Arguments);
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

        /// <summary>
        /// Delete exchange and queue which suppose to be created by RouteSetupData
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="routeSetupData"></param>
        public virtual void DestroyRoute<T>(RouteSetupData routeSetupData)
        {
            var queueName = routeSetupData.RouteFinder.FindQueueName<T>(routeSetupData.SubscriptionName);
            var exchangeName = routeSetupData.RouteFinder.FindExchangeName<T>();

            using (var connection = ConnectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    // Delete Queue
                    DeleteQueue<T>(model, routeSetupData.QueueSetupData, queueName);
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



        protected virtual void DeleteQueue<T>(IModel model, QueueSetupData queue, string queueName)
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
