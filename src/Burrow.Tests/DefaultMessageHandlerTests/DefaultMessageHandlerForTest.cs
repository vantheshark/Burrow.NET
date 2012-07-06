using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Burrow.Tests.DefaultMessageHandlerTests
{
    class DefaultMessageHandlerForTest<T> : DefaultMessageHandler<T>
    {
        public DefaultMessageHandlerForTest(string subscriptionName, Action<T, MessageDeliverEventArgs> msgHandlingAction, IConsumerErrorHandler consumerErrorHandler, ISerializer messageSerializer, IRabbitWatcher watcher) 
            : base(subscriptionName, msgHandlingAction, consumerErrorHandler, messageSerializer, watcher)
        {
            HandlingComplete += x => { throw new Exception("HandlingCompleteException"); };
        }

        protected override void AfterHandlingMessage(BasicDeliverEventArgs eventArg)
        {
            throw new Exception("AfterHandlingMessageException");
        }

        public void PublicCheckMessageType(IBasicProperties properties)
        {
            CheckMessageType(properties);
        }
    }
}