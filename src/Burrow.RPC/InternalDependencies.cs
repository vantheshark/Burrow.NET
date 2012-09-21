
namespace Burrow.RPC
{
    internal static class InternalDependencies
    {
        public static IMethodMatcher MethodMatcher = new MethodMatcher();
        public static IRpcQueueHelper RpcQueueHelper = new Helper();
    }
}
