
namespace Burrow.RPC
{
    /// <summary>
    /// An internal static endpoint provides some helpers for the library.
    /// Using this way to reduce the amount of dependencies to the public classes but still allows unit testing ;)
    /// </summary>
    internal static class InternalDependencies
    {
        public static IMethodMatcher MethodMatcher = new MethodMatcher();
        public static IRpcQueueHelper RpcQueueHelper = new Helper();
    }
}
