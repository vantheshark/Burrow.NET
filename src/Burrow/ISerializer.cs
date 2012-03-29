
namespace Burrow
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T message);
        T Deserialize<T>(byte[] bytes);
    }
}
