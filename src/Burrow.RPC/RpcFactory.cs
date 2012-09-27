using Castle.DynamicProxy;

namespace Burrow.RPC
{
    public static class RpcFactory
    {
        /// <summary>
        /// Create Rpc client using dynamic proxy without providing a real implementatin of generic interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="routeFinder">Provide a valid route finder to route your request to correct targets, default will be DefaultRpcRouteFinder</param>
        /// <param name="rabbitMqConnectionString"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static T CreateClient<T>(IRpcRouteFinder routeFinder = null, string rabbitMqConnectionString = null, params IMethodFilter[] filters) where T : class
        {
            return CreateClient<T>(new RpcClientInterceptor(new BurrowRpcClientCoordinator<T>(rabbitMqConnectionString, routeFinder ?? new DefaultRpcRouteFinder<T>()), filters));
        }

        /// <summary>
        /// Create Rpc client using dynamic proxy without providing a rea implementatin of generic interface
        /// </summary>
        public static T CreateClient<T>(IRpcClientCoordinator coordinator, params IMethodFilter[] filters) where T : class
        {
            return CreateClient<T>(new RpcClientInterceptor(coordinator, filters));
        }

        internal static T CreateClient<T>(RpcClientInterceptor interceptor) where T : class
        {
            var proxy = new ProxyGenerator();
            return proxy.CreateInterfaceProxyWithoutTarget<T>(interceptor);
        }


        /// <summary>
        /// Create Rpc server using a realImplementation which will handle rpc request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="realImplementation"></param>
        /// <param name="routeFinder">If null, the DefaultRpcRouteFinder will be used.</param>
        /// <param name="rabbitMqConnectionString"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public static IRpcServerCoordinator CreateServer<T>(T realImplementation, IRpcRouteFinder routeFinder = null, string rabbitMqConnectionString = null, string serverId = null) where T : class
        {
            return new BurrowRpcServerCoordinator<T>(realImplementation, 
                                                     routeFinder ?? new DefaultRpcRouteFinder<T>(), 
                                                     rabbitMqConnectionString, 
                                                     serverId);
        }

        /// <summary>
        /// Create Rpc server using a realImplementation which will handle rpc request and using DefaultFanoutRpcRequestRouteFinder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="realImplementation"></param>
        /// <param name="requestQueueName">If provided, the value will be used as the request queue name, otherwise default value will be used</param>
        /// <param name="rabbitMqConnectionString"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public static IRpcServerCoordinator CreateServer<T>(T realImplementation, string requestQueueName, string rabbitMqConnectionString = null, string serverId = null) where T : class
        {
            return new BurrowRpcServerCoordinator<T>(realImplementation, requestQueueName, rabbitMqConnectionString, serverId);
        }

        /// <summary>
        /// Create Rpc server using a realImplementation which will handle 'fanout' rpc request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="realImplementation"></param>
        /// <param name="requestQueueName"> </param>
        /// <param name="rabbitMqConnectionString"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public static IRpcServerCoordinator CreateFanoutServer<T>(T realImplementation, string requestQueueName = null, string rabbitMqConnectionString = null, string serverId = null) where T : class
        {
            return new BurrowRpcServerCoordinator<T>(realImplementation, 
                                                     new DefaultFanoutRpcRequestRouteFinder<T>(requestQueueName: requestQueueName,  serverId: serverId),
                                                     rabbitMqConnectionString,
                                                     serverId);
        }

        
    }
}