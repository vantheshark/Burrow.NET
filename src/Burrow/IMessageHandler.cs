using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Burrow
{
    public delegate void MessageHandlingEvent(BasicDeliverEventArgs eventArgs);

    public interface IMessageHandler
    {
        void BeforeHandlingMessage(IBasicConsumer consumer, BasicDeliverEventArgs eventArg);
        void HandleMessage(IBasicConsumer consumer, BasicDeliverEventArgs eventArg);
        void AfterHandlingMessage(IBasicConsumer consumer, BasicDeliverEventArgs eventArg);

        void HandleError(IBasicConsumer consumer, BasicDeliverEventArgs eventArg, Exception excepton);
        event MessageHandlingEvent HandlingComplete;
    }
}
