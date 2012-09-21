using Castle.DynamicProxy;

namespace Burrow.RPC
{
    public static class RpcClientFactory
    {
        public static T Create<T>(string rabbitMqConnectionString = null, IRouteFinder routeFinder = null, params IMethodFilter[] filters) where T : class
        {
            return Create<T>(new RpcClientInterceptor(new BurrowRpcClientCoordinator<T>(rabbitMqConnectionString, routeFinder), filters));
        }

        public static T Create<T>(IRpcClientCoordinator coordinator, params IMethodFilter[] filters) where T : class
        {
            return Create<T>(new RpcClientInterceptor(coordinator, filters));
        }

        internal static T Create<T>(RpcClientInterceptor interceptor) where T : class
        {
            var proxy = new ProxyGenerator();
            return proxy.CreateInterfaceProxyWithoutTarget<T>(interceptor);
        }
    }
}