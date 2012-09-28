using System;
using System.Collections.Generic;

namespace Burrow.RPC
{
    /// <summary>
    /// A wrapper object that contain information about a result of RPC call sent from server
    /// </summary>
    public class RpcResponse
    {
        /// <summary>
        /// Any serializable exception from the server if set will be thrown in the client
        /// </summary>
        public Exception Exception { get; set; }
        
        public Guid RequestId { get; set; }

        /// <summary>
        /// The return value of RPC call if the method has return value
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// Any changes in byref/out params
        /// </summary>
        public Dictionary<string, object> ChangedParams { get; set; }
    }
}