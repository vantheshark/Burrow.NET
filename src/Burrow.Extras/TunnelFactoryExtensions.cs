
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
        public static void RegisterResolver(this TunnelFactory factory, IBurrowResolver burrowResolver)
        {
            new DependencyInjectionTunnelFactory(burrowResolver);

            if (burrowResolver.Resolve<ITypeNameSerializer>() != null)
            {
                Global.DefaultTypeNameSerializer = burrowResolver.Resolve<ITypeNameSerializer>();
            }
        }

        public static TunnelFactory WithPrioritySupport(this TunnelFactory factory)
        {
            if (factory is PriorityTunnelFactory)
            {
                return factory;
            }
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

        /// <summary>
        /// By default, Burrow.NET works around the priority messages by creating different queues for different messages at different level of priority.
        /// The name for those queues will be OriginalQueueName.Priority[n], if you want to change that, implement IPriorityQueueSuffix and give it to this method
        /// before creating queues, anywhere at the very begining of the application
        /// </summary>
        /// <param name="tunnel"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static ITunnelWithPrioritySupport ChangePriorityQueueSuffixConvention(this ITunnelWithPrioritySupport tunnel, IPriorityQueueSuffix suffix)
        {
            if (suffix == null)
            {
                throw new ArgumentNullException("suffix");
            }
            PriorityQueuesRabbitSetup.GlobalPriorityQueueSuffix = suffix;
            return tunnel;
        }
    }
}
