using System;
using Burrow.Extras.Internal;

namespace Burrow.Extras
{
    internal interface IPrioritySubscriptionOption
    {
        uint MaxPriorityLevel { get; set; }
        Type ComparerType { get; set; }
        string SubscriptionName { get; set; }
        IRouteFinder RouteFinder { get; set; }
        IPriorityQueueSuffix QueueSuffixNameConvention { get; set; }
        uint QueuePrefetchSize { get; }
        Func<uint, uint> QueuePrefetchSizeSelector { get; set; }
    }
}