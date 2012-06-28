using System.Collections.Generic;

namespace Burrow.Extras.Internal
{
    /// <summary>
    /// Default comparer which considers messages with higher number priority to have higher priority
    /// </summary>
    public class PriorityComparer<T> : IComparer<T> where T : IPriorityMessage
    {
        // Methods
        public int Compare(T x, T y)
        {
            uint? priority = x.Priority;
            uint? nullable2 = y.Priority;
            if ((priority.GetValueOrDefault() > nullable2.GetValueOrDefault()) && (priority.HasValue & nullable2.HasValue))
            {
                return 1;
            }
            uint? nullable3 = x.Priority;
            uint? nullable4 = y.Priority;
            if ((nullable3.GetValueOrDefault() < nullable4.GetValueOrDefault()) && (nullable3.HasValue & nullable4.HasValue))
            {
                return -1;
            }
            if (x.Duration > y.Duration)
            {
                return 1;
            }
            if (x.Duration < y.Duration)
            {
                return -1;
            }
            return 0;
        }
    }
}
