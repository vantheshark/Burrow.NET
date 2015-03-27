namespace Burrow
{
    /// <summary>
    /// The status of the consumer
    /// </summary>
    public enum ConsumerStatus
    {
        /// <summary>
        /// Active
        /// </summary>
        Active,
        /// <summary>
        /// Waiting for running tasks to finish 
        /// </summary>
        Disposing,
        /// <summary>
        /// Disposed
        /// </summary>
        Disposed
    }
}