using System;

namespace Burrow.Extras.Internal
{
    /// <summary>
    /// Implement this interface to provide the suffix convention name for priority queues
    /// </summary>
    public interface IPriorityQueueSuffix
    {
        string Get(Type messagesType, uint priority);
    }

    internal class PriorityQueueSuffix : IPriorityQueueSuffix
    {
        public string Get(Type messagesType, uint priority)
        {
            return string.Format("_Priority{0}", priority);
        }
    }
}
