using System;

namespace Burrow.RPC
{
    /// <summary>
    /// As this library based on Burrow.NET, this adaptor is used to adapt the RpcRouteFinder to traditional IRouteFinder
    /// </summary>
    internal class RpcRouteFinderAdapter : IRouteFinder
    {
        private readonly IRpcRouteFinder _routeFinder;

        public RpcRouteFinderAdapter(IRpcRouteFinder routeFinder)
        {
            _routeFinder = routeFinder;
        }

        public string FindExchangeName<T>()
        {
            if (typeof(T) == typeof(RpcRequest))
            {
                return _routeFinder.RequestExchangeName;
            }

            if (typeof(T) == typeof(RpcResponse))
            {
                return string.Empty;
            }

            throw new NotSupportedException();
        }

        public string FindRoutingKey<T>()
        {
            throw new NotSupportedException();
        }

        public string FindQueueName<T>(string subscriptionName)
        {
            if (typeof(T) == typeof(RpcRequest))
            {
                return _routeFinder.RequestQueue;
            }

            if (typeof(T) == typeof(RpcResponse))
            {
                return _routeFinder.UniqueResponseQueue;
            }

            throw new NotSupportedException();
        }
    }
}