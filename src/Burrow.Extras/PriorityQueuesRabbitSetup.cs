using System;
using System.Collections.Generic;
using System.Globalization;
using Burrow.Extras.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Burrow.Extras
{
    /// <summary>
    /// Initialize a setup class to create exchanges & priority queues
    /// </summary>
    public class PriorityQueuesRabbitSetup : RabbitSetup
    {
        internal static volatile IPriorityQueueSuffix GlobalPriorityQueueSuffix = new PriorityQueueSuffix();

        /// <summary>
        /// Initialize a setup with provided connection string
        /// </summary>
        /// <param name="connectionString"></param>
        public PriorityQueuesRabbitSetup(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Initialize a setup with provided connection string and watcher
        /// </summary>
        public PriorityQueuesRabbitSetup(IRabbitWatcher watcher, string connectionString) : base(watcher, connectionString)
        {
        }

        protected override void DeclareExchange(ExchangeSetupData exchange, IModel model, string exchangeName)
        {
            if (exchange != null && "headers".Equals(exchange.ExchangeType, StringComparison.InvariantCultureIgnoreCase))
            {
                base.DeclareExchange(exchange, model, exchangeName);
            }
            else
            {
                throw new Exception("Expect exchange type headers");    
            }
        }

        protected override void BindQueue<T>(IModel model, QueueSetupData queue, string exchangeName, string queueName, string routingKey, IDictionary<string, object> bindingData = null)
        {
            if (queue is PriorityQueueSetupData)
            {
                var maxPriority = (queue as PriorityQueueSetupData).MaxPriorityLevel;
                for (uint i = 0; i <= maxPriority; i++)
                {
                    try
                    {
                        var arguments = GetArgumentDictionary(bindingData);
                        arguments["x-match"] = "all";
                        arguments["Priority"]  = i.ToString(CultureInfo.InvariantCulture);
                        arguments["RoutingKey"] = routingKey;
                        //http://www.rabbitmq.com/tutorials/amqp-concepts.html
                        //http://lostechies.com/derekgreer/2012/03/28/rabbitmq-for-windows-exchange-types/
                        model.QueueBind(GetPriorityQueueName<T>(queueName, i), exchangeName, routingKey/*It'll be ignored as AMQP spec*/, arguments);
                    }
                    catch (Exception ex)
                    {
                        _watcher.Error(ex);
                    }
                }
            }
            else
            {
                base.BindQueue<T>(model, queue, exchangeName, queueName, routingKey, bindingData);
            }
        }

        private IDictionary<string, object> GetArgumentDictionary(IEnumerable<KeyValuePair<string, object>> originalDictionary)
        {
            var dic = new Dictionary<string, object>();
            if (originalDictionary == null)
            {
                return dic;
            }
            foreach (var entry in originalDictionary)
            {
                dic[entry.Key] = entry.Value;
            }
            return dic;
        }
        

        private string GetPriorityQueueName<T>(string originalQueueName, uint priority)
        {
            return string.Format("{0}{1}", originalQueueName, GlobalPriorityQueueSuffix.Get(typeof(T), priority));
        }

        protected override void DeclareQueue<T>(QueueSetupData queue, string queueName, IModel model)
        {
            SetQueueSetupArguments(queue);

            if (queue is PriorityQueueSetupData)
            {
                var maxPriority = (queue as PriorityQueueSetupData).MaxPriorityLevel;
                for (uint i = 0; i <= maxPriority; i++)
                {
                    try
                    {
                        model.QueueDeclare(GetPriorityQueueName<T>(queueName, i), queue.Durable, false, queue.AutoDelete, queue.Arguments);
                    }
                    catch (OperationInterruptedException oie)
                    {
                        if (oie.ShutdownReason.ReplyText.StartsWith("PRECONDITION_FAILED - "))
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
            else
            {
                base.DeclareQueue<T>(queue, queueName, model);
            }
        }

        protected override void DeleteQueue<T>(IModel model, QueueSetupData queue, string queueName)
        {

            if (queue is PriorityQueueSetupData)
            {
                var maxPriority = (queue as PriorityQueueSetupData).MaxPriorityLevel;
                for (uint i = 0; i <= maxPriority; i++)
                {
                    try
                    {
                        model.QueueDelete(GetPriorityQueueName<T>(queueName, i));
                    }
                    catch (OperationInterruptedException oie)
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
            else
            {
                base.DeleteQueue<T>(model, queue, queueName);
            }
        }
    }
}
