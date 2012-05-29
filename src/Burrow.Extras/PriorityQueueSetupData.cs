using System.Diagnostics;

namespace Burrow.Extras
{
    [DebuggerStepThrough]
    public class PriorityQueueSetupData : QueueSetupData
    {
        public uint MaxPriorityLevel { get; private set; }

        public PriorityQueueSetupData(uint maxPriorityLevel)
        {
            MaxPriorityLevel = maxPriorityLevel;
        }
    }
}