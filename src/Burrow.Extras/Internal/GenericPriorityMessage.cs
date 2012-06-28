using System;

namespace Burrow.Extras.Internal
{
    public class GenericPriorityMessage<T> : IPriorityMessage
    {
        private readonly DateTime _startTime;
        public uint? Priority { get; private set; }

        public long Duration 
        { 
            get { return DateTime.Now.Subtract(_startTime).Ticks; }
        }

        public T Message { get; private set; }

        public GenericPriorityMessage(T message, uint priority)
        {
            Priority = priority;
            Message = message;
            _startTime = DateTime.Now;
        }
    }
}