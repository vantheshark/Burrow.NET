
using System;

namespace Burrow
{
    public interface IRetryPolicy
    {
        /// <summary>
        /// In miliseconds
        /// </summary>
        int DelayTime { get; }

        void WaitForNextRetry(Action retryingAction);

        void Reset();
    }
}
