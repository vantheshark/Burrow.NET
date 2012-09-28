using System;
using System.Collections.Generic;
using System.Reflection;

namespace Burrow.RPC
{
    /// <summary>
    /// A wrapper object that keep all information of a RPC method request from client
    /// </summary>
    public class RpcRequest
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// The unique response queue that the client will subscribe to for the response.
        /// If the client interface is singleton, there should be 1 responseaddress. Otherwise, every instance of the same interface should have
        /// different response address.
        /// This value is set from UniqueResponseQueue property of the provided RpcRouteFinder
        /// </summary>
        public string ResponseAddress { get; set; }
        
        /// <summary>
        /// the parameters of the called method. The list of params should be kept in correct order as all of them will be passed to the method in the exact order
        /// </summary>
        public Dictionary<string, object> Params { get; set; }
        
        public string MethodName { get; set; }

        public string DeclaringType { get; set; }

        public MemberTypes MemberType { get; set; }
        
        /// <summary>
        /// A unique or hash string. It's used to retrive the method info from the Request
        /// </summary>
        public string MethodSignature { get; set; }
        
        public DateTime? UtcExpiryTime { get; set; }

        public RpcRequest()
        {
            Id = Guid.NewGuid();
        }
    }
}