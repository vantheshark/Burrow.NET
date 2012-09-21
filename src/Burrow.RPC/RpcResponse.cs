using System;
using System.Collections.Generic;

namespace Burrow.RPC
{
    public class RpcResponse
    {
        public Exception Exception { get; set; }
        public Guid RequestId { get; set; }
        public object ReturnValue { get; set; }
        public Dictionary<string, object> ChangedParams { get; set; }
    }
}