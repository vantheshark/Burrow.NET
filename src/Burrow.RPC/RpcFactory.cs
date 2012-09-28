using System;
using System.Diagnostics.CodeAnalysis;
using Castle.DynamicProxy;

namespace Burrow.RPC
{
    public static class RpcFactory
    {
        /// <summary>
        /// Create Rpc client using dynamic proxy without providing a real implementatin of generic interface
        /// </summary>
        /// <typeparam name="T">the interface which we use for RPC call</typeparam>
        /// <param name="routeFinder">Provide a valid route finder to route your request to correct targets, default will be DefaultRpcRouteFinder</param>
        /// <param name="rabbitMqConnectionString"></param>
        /// <param name="filters">custom filters to determine whether a method is valid/async for RPC call</param>
        /// <returns></returns>
        public static T CreateClient<T>(IRpcRouteFinder routeFinder = null, string rabbitMqConnectionString = null, params IMethodFilter[] filters) where T : class
        {
            return CreateClient<T>(new RpcClientInterceptor(new BurrowRpcClientCoordinator<T>(rabbitMqConnectionString, routeFinder ?? new DefaultRpcRouteFinder<T>()), filters));
        }
        
        /// <summary>
        /// Create Rpc client using dynamic proxy without providing a rea implementatin of generic interface
        /// </summary>
        /// <typeparam name="T">the interface which we use for RPC call</typeparam>
        /// <param name="coordinator">an implementation of rpc client coordinator which can send requests to server</param>
        /// <param name="filters">custom filters to determine whether a method is valid/async for RPC call</param>
        /// <returns></returns>
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
        /// <typeparam name="T">the interface which we use for RPC call</typeparam>
        /// <param name="realImplementation">an instance of the class implemented the generic interface, it will eventually handle the rpc method call from the client</param>
        /// <param name="routeFinder">If null, the DefaultRpcRouteFinder will be used and client/server will contact to each other directly through default built-in exchange</param>
        /// <param name="rabbitMqConnectionString"></param>
        /// <param name="serverId">will be used to determine whether the request queue is durable. It is also used as the subscription name when the server subscribe to request queue</param>
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
        /// <typeparam name="T">the interface which we use for RPC call</typeparam>
        /// <param name="realImplementation">an instance of the class implemented the generic interface, it will eventually handle the rpc method call from the client</param>
        /// <param name="requestQueueName">If provided, the value will be used as the request queue name, otherwise default value will be used</param>
        /// <param name="rabbitMqConnectionString"></param>
        /// <param name="serverId">will be used to determine whether the request queue is durable. It is also used as the subscription name when the server subscribe to request queue</param>
        /// <returns></returns>
        public static IRpcServerCoordinator CreateServer<T>(T realImplementation, string requestQueueName, string rabbitMqConnectionString = null, string serverId = null) where T : class
        {
            return new BurrowRpcServerCoordinator<T>(realImplementation, requestQueueName, rabbitMqConnectionString, serverId);
        }

        /// <summary>
        /// Create Rpc server using a realImplementation which will handle 'fanout' rpc request
        /// </summary>
        /// <typeparam name="T">the interface which we use for RPC call</typeparam>
        /// <param name="realImplementation">an instance of the class implemented the generic interface, it will eventually handle the rpc method call from the client</param>
        /// <param name="requestQueueName">if provided, this value will be used as the request queue name</param>
        /// <param name="rabbitMqConnectionString"></param>
        /// <param name="serverId">will be used to determine whether the request queue is durable. It is also used as the subscription name when the server subscribe to request queue<para>
        /// Otherwise, the request will name will be generated based on provided serverId and name of the generic T type.
        /// </para></param>
        /// <returns></returns>
        public static IRpcServerCoordinator CreateFanoutServer<T>(T realImplementation, string requestQueueName = null, string rabbitMqConnectionString = null, string serverId = null) where T : class
        {
            return new BurrowRpcServerCoordinator<T>(realImplementation, 
                                                     new DefaultFanoutRpcRequestRouteFinder<T>(requestQueueName: requestQueueName,  serverId: serverId),
                                                     rabbitMqConnectionString,
                                                     serverId);
        }

        /// <summary>
        /// Change the IMethodMatcher of the library if you wish to but I don't think of any good reason to do that ;) 
        /// </summary>
        /// <param name="methodMatcher"></param>
        [ExcludeFromCodeCoverage]
        public static void RegisterMethodMatcher(IMethodMatcher methodMatcher)
        {
            if (methodMatcher == null)
            {
                throw new ArgumentNullException("methodMatcher");
            }
            InternalDependencies.MethodMatcher = methodMatcher;
        }

        /// <summary>
        /// Change the IRpcQueueHelper of the library if you wish to but I don't think of any good reason to do that ;)
        /// </summary>
        /// <param name="helper"></param>
        [ExcludeFromCodeCoverage]
        public static void RegisterHelper(IRpcQueueHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException("helper");
            }
            InternalDependencies.RpcQueueHelper = helper;
        }
    }
}