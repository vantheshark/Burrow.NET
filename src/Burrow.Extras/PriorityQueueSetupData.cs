using System.Diagnostics;

namespace Burrow.Extras
{
    [DebuggerStepThrough]
    public class PriorityQueueSetupData : QueueSetupData
    {
        public int MaxPriorityLevel { get; private set; }

        public PriorityQueueSetupData(int maxPriorityLevel)
        {
            MaxPriorityLevel = maxPriorityLevel;
        }
    }
}