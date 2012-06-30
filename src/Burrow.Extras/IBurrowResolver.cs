namespace Burrow.Extras
{
    /// <summary>
    /// Implement this interface to use Dependency injection with Burrow.NET
    /// </summary>
    public interface IBurrowResolver
    {
        T Resolve<T>();
    }
}