using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using RabbitMQ.Client.Exceptions;

namespace Burrow.Extras
{
    public class PriorityQueuesRabbitSetup : RabbitSetup
    {
        public PriorityQueuesRabbitSetup(Func<string, string, IRouteFinder> routeFinderFactory, IRabbitWatcher watcher, string connectionString, string environment) 
            : base(routeFinderFactory, watcher, connectionString, environment)
        {
        }

        protected override void DeclareExchange(ExchangeSetupData exchange, RabbitMQ.Client.IModel model, string exchangeName)
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

        protected override void BindQueue(RabbitMQ.Client.IModel model, QueueSetupData queue, string exchangeName, string queueName, string routingKey)
        {
            if (queue is PriorityQueueSetupData)
            {
                var maxPriority = (queue as PriorityQueueSetupData).MaxPriorityLevel;
                for (var i = 0; i <= maxPriority; i++)
                {
                    try
                    {
                        IDictionary arguments = new HybridDictionary();
                        arguments.Add("x-match", "all");
                        arguments.Add("Priority", i.ToString(CultureInfo.InvariantCulture));
                        arguments.Add("RoutingKey", routingKey);
                        //http://www.rabbitmq.com/tutorials/amqp-concepts.html
                        //http://lostechies.com/derekgreer/2012/03/28/rabbitmq-for-windows-exchange-types/
                        model.QueueBind(GetPriorityQueueName(queueName, i), exchangeName, routingKey/*It'll be ignored as AMQP spec*/, arguments);
                    }
                    catch (Exception ex)
                    {
                        _watcher.Error(ex);
                    }
                }
            }
            else
            {
                base.BindQueue(model, queue, exchangeName, queueName, routingKey);
            }
        }

        private string GetPriorityQueueName(string originalQueueName, int priority)
        {
            return string.Format("{0}_Priority{1}", originalQueueName, priority);
        }

        protected override void DeclareQueue(QueueSetupData queue, string queueName, RabbitMQ.Client.IModel model)
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

            if (queue is PriorityQueueSetupData)
            {
                var maxPriority = (queue as PriorityQueueSetupData).MaxPriorityLevel;
                for (int i = 0; i <= maxPriority; i++)
                {
                    try
                    {
                        model.QueueDeclare(GetPriorityQueueName(queueName, i), queue.Durable, false,
                                           queue.AutoDelete, arguments);
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
                base.DeclareQueue(queue, queueName, model);
            }
        }

        protected override void DeleteQueue(RabbitMQ.Client.IModel model, QueueSetupData queue, string queueName)
        {

            if (queue is PriorityQueueSetupData)
            {
                var maxPriority = (queue as PriorityQueueSetupData).MaxPriorityLevel;
                for (int i = 0; i <= maxPriority; i++)
                {
                    try
                    {
                        model.QueueDelete(GetPriorityQueueName(queueName, i));
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
                base.DeleteQueue(model, queue, queueName);
            }
        }
    }
}
