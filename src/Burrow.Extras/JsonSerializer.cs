using System.Text;
using Newtonsoft.Json;

namespace Burrow.Extras
{
    public class JsonSerializer : ISerializer
    {
        public byte[] Serialize<T>(T message)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
        }
    }
}
