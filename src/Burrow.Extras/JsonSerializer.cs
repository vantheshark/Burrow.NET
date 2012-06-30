using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json;

namespace Burrow.Extras
{
    /// <summary>
    /// This class is responsible for serializing and deserializing objects using Json.NET
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
        };

        public byte[] Serialize<T>(T message)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message, Formatting.Indented, _settings));
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes), _settings);
        }
    }
}
