
using System;

namespace Burrow
{
    public interface IRetryPolicy
    {
        /// <summary>
        /// In miliseconds
        /// </summary>
        int DelayTime { get; }

        bool IsWaiting { get; }

        void WaitForNextRetry(Action retryingAction);

        void Reset();
    }
}
