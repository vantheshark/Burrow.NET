using System;

namespace Burrow.RPC
{
    public class BurrowRpcRouteFinder : IRouteFinder
    {
        public virtual string FindExchangeName<T>()
        {
            if (typeof(T) == typeof(RpcRequest) || typeof(T) == typeof(RpcResponse))
            {
                return string.Empty; // Default built-in Exchange
            }
            
            throw new NotSupportedException();
        }

        public string FindRoutingKey<T>()
        {
            throw new NotSupportedException();
        }

        public virtual string FindQueueName<T>(string subscriptionName)
        {
            if (typeof(T) == typeof(RpcRequest))
            {
                return string.Format("Burrow.Queue.Rpc.{0}.Requests", subscriptionName);
            }

            if (typeof(T) == typeof(RpcResponse))
            {
                return string.Format("Burrow.Queue.Rpc.{0}.Responses", subscriptionName);
            }

            throw new NotSupportedException();
        }
    }
}