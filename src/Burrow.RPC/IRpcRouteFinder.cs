namespace Burrow.RPC
{
    public interface IRpcRouteFinder
    {
        /// <summary>
        /// If set to true, the library will create exchange and queue for you
        /// </summary>
        bool CreateExchangeAndQueue { get; }

        /// <summary>
        /// Default can be empty as the empty exchange is the built-in exchange
        /// </summary>
        string RequestExchangeName { get; }

        /// <summary>
        /// Should be either direct or fanout
        /// </summary>
        string RequestExchangeType { get; }
        
        /// <summary>
        /// If RequestExchangeName is empty, Burrow.RPC will route the RpcRequest object to this queue by publishing the msg to the empty exchange with the routing key is equal to this queue name
        /// </summary>
        string RequestQueue { get; }

        /// <summary>
        /// The response queue must be unique per instance of the RPC client
        /// <para>If you have 2 instances of the rpc clients, these instances should subscribe to different response queue as the responses from the rpc server must be routed to correct client</para>
        /// </summary>
        string UniqueResponseQueue { get; }
    }
}