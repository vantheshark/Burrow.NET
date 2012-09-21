using System;
using System.Collections.Generic;
using System.Reflection;

namespace Burrow.RPC
{
    public class RpcRequest
    {
        public Guid Id { get; set; }
        public string ResponseAddress { get; set; }
        public Dictionary<string, object> Params { get; set; }
        public string MethodName { get; set; }
        public string DeclaringType { get; set; }
        public MemberTypes MemberType { get; set; }
        public string MethodSignature { get; set; }
        public DateTime? UtcExpiryTime { get; set; }

        public RpcRequest()
        {
            Id = Guid.NewGuid();
        }
    }
}