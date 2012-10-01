using System;
using RabbitMQ.Client.Events;

namespace Burrow
{
    public delegate void MessageHandlingEvent(BasicDeliverEventArgs eventArgs);

    public delegate void MessageWasNotHandledEvent(BasicDeliverEventArgs eventArgs);

    public interface IMessageHandler
    {
        /// <summary>
        /// This contain the logic flow to handle the message. This method should never thrown any exception if you are implementing it.
        /// The default implementations will follow this rule
        /// </summary>
        /// <param name="eventArg"></param>
        void HandleMessage(BasicDeliverEventArgs eventArg);

        /// <summary>
        /// Provide the logic to handle the message once something goes wrong.
        /// The default logic is implemented in ConsumerErrorHandler because this method simply delete the call to that class
        /// </summary>
        /// <param name="eventArg"></param>
        /// <param name="excepton"></param>
        void HandleError(BasicDeliverEventArgs eventArg, Exception excepton);
        
        /// <summary>
        /// Once the message is finished successfully or not, this event should be fired at the end
        /// </summary>
        event MessageHandlingEvent HandlingComplete;


        /// <summary>
        /// If there is something wrong before the msg is delivered to client code, this event should be raised.
        /// This can happen in method BeforeHandlingMessage or there is an error in object deserializing
        /// https://github.com/vanthoainguyen/Burrow.NET/issues/4
        /// </summary>
        event MessageWasNotHandledEvent MessageWasNotHandled;
    }
}
