
using System;

namespace Burrow
{
    interface IRetryPolicy
    {
        /// <summary>
        /// In miliseconds
        /// </summary>
        int DelayTime { get; }

        void WaitForNextRetry(Action retryingAction);

        void Reset();
    }
}
