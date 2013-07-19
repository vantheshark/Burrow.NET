using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Burrow.Extras.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Burrow.Extras
{
    public class PriorityQueuesRabbitSetup : RabbitSetup
    {
        internal static volatile IPriorityQueueSuffix GlobalPriorityQueueSuffix = new PriorityQueueSuffix();

        public PriorityQueuesRabbitSetup(string connectionString) : base(connectionString)
        {
        }

        public PriorityQueuesRabbitSetup(IRabbitWatcher watcher, string connectionString) : base(watcher, connectionString)
        {
        }

        protected override void DeclareExchange(ExchangeSetupData exchange, IModel model, string exchangeName)
        {
            if (exchange != null && exchange.ExchangeType == "headers")
            {
                base.DeclareExchange(exchange, model, exchangeName);
            }
            else
            {
                throw new Exception("Expect exchange type headers");    
            }
        }

        protected override void BindQueue<T>(IModel model, QueueSetupData queue, string exchangeName, string queueName, string routingKey, IDictionary bindingData = null)
        {
            if (queue is PriorityQueueSetupData)
            {
                var maxPriority = (queue as PriorityQueueSetupData).MaxPriorityLevel;
                for (uint i = 0; i <= maxPriority; i++)
                {
                    try
                    {
                        IDictionary arguments = GetArgumentDictionary(bindingData);
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

        private IDictionary GetArgumentDictionary(IDictionary originalDictionary)
        {
            var dic = new Dictionary<object, object>();
            if (originalDictionary == null)
            {
                return dic;
            }
            foreach (var key in originalDictionary)
            {
                dic[key] = originalDictionary[key];
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
