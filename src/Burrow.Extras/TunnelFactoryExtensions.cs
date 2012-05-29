
using System;
using Burrow.Extras.Internal;

namespace Burrow.Extras
{
    public static class TunnelFactoryExtensions
    {
        /// <summary>
        /// Call this method to register a dependency resolver and set default TunnelFactory to DependencyInjectionTunnelFactory
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="burrowResolver"></param>
        public static void RegisterResolver(this ITunnelFactory factory, IBurrowResolver burrowResolver)
        {
            new DependencyInjectionTunnelFactory(burrowResolver);

            if (burrowResolver.Resolve<ITypeNameSerializer>() != null)
            {
                Global.DefaultTypeNameSerializer = burrowResolver.Resolve<ITypeNameSerializer>();
            }
        }

        public static ITunnelFactory WithPrioritySupport(this ITunnelFactory factory)
        {
            return new PriorityTunnelFactory();
        }

        public static ITunnelWithPrioritySupport WithPrioritySupport(this ITunnel tunnel)
        {
            if (!(tunnel is ITunnelWithPrioritySupport))
            {
                throw new InvalidCastException("Current tunnel object is not supporting priority queues");
            }
            return tunnel as ITunnelWithPrioritySupport;
        }
    }
}
