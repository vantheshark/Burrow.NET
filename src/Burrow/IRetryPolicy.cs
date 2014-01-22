
using System;

namespace Burrow
{
    /// <summary>
    /// Implement this interface to define the policy how the connection should be created after a failed attempt
    /// </summary>
    public interface IRetryPolicy
    {
        /// <summary>
        /// In miliseconds
        /// </summary>
        int DelayTime { get; }

        /// <summary>
        /// Determine whether it's waiting to establish a connection
        /// </summary>
        bool IsWaiting { get; }

        /// <summary>
        /// An async method to wait and execute an retry action
        /// </summary>
        /// <param name="retryingAction"></param>
        void WaitForNextRetry(Action retryingAction);

        /// <summary>
        /// Reset the policy once the connection is established
        /// </summary>
        void Reset();
    }
}
