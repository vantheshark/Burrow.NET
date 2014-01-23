using System.Diagnostics;
using Burrow.Extras.Internal;

namespace Burrow.Extras
{
    /// <summary>
    /// A DTO object which contains information to create priority queues
    /// </summary>
    [DebuggerStepThrough]
    public class PriorityQueueSetupData : QueueSetupData
    {
        /// <summary>
        /// Max priority level 
        /// </summary>
        public uint MaxPriorityLevel { get; private set; }

        /// <summary>
        /// Optional, provide a class to resolve the suffix name for priority queues, default is _Priority[PriorityNumber]
        /// </summary>
        public IPriorityQueueSuffix QueueSuffixConvention { get; set; }

        /// <summary>
        /// Initialize a queue setup data with maxPriorityLevel
        /// <para>If you wish to have Queue_Priority0, Queue_Priority1, Queue_Priority2 then maxPriorityLevel = 2</para>
        /// </summary>
        /// <param name="maxPriorityLevel"></param>
        public PriorityQueueSetupData(uint maxPriorityLevel)
        {
            MaxPriorityLevel = maxPriorityLevel;
        }
    }
}