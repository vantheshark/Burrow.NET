namespace Burrow.Extras.Internal
{
    public interface IPriorityMessage
    {
        /// <summary>
        /// The priority of the message
        /// </summary>
        uint? Priority { get; }

        /// <summary>
        /// The age of the message
        /// </summary>
        long Duration { get; }
    }
}
