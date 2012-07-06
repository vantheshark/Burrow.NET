using System;

namespace Burrow
{
    public class BadMessageHandlerException : Exception
    {
        public BadMessageHandlerException(Exception innerException) : base("", innerException)
        {
        }

        public override string Message
        {
            get
            {
                return "Method HandleMessage of the IMessageHandler should never throw any exception. If it's the built-in MessageHandler please contact the author asap!";
            }
        }
    }
}
