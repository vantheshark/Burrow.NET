
namespace Burrow.Extras
{
    public static class TunnelFactoryExtensions
    {
        public static void RegisterResolver(this TunnelFactory factory, IBurrowResolver burrowResolver)
        {
            new DependencyInjectionTunnelFactory(burrowResolver);

            if (burrowResolver.Resolve<ITypeNameSerializer>() != null)
            {
                Global.DefaultTypeNameSerializer = burrowResolver.Resolve<ITypeNameSerializer>();
            }
        }
    }
}
