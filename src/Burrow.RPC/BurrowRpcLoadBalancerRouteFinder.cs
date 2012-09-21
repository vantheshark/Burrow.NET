using System;

namespace Burrow.RPC
{
    public class BurrowRpcLoadBalancerRouteFinder : BurrowRpcRouteFinder
    {
        public override string FindExchangeName<T>()
        {
            if (typeof(T) == typeof(RpcResponse))
            {
                return string.Empty; // Default built-in Exchange
            }

            if (typeof(T) == typeof(RpcRequest))
            {
                return "Burrow.Exchange.Rpc.Requests";
            }

            throw new NotSupportedException();
        }
    }
}