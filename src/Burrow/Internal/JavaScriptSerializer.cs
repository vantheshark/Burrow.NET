using System.Text;

namespace Burrow.Internal
{
    /// <summary>
    /// 20% slower than JsonSerializer which can be found in Burrow.Extras package
    /// </summary>
    public class JavaScriptSerializer : ISerializer
    {
        private readonly System.Web.Script.Serialization.JavaScriptSerializer _serializer;
        private readonly UTF8Encoding _encoding;
        public JavaScriptSerializer()
        {
            _serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            _encoding = new UTF8Encoding();
        }

        public byte[] Serialize<T>(T message)
        {
            if (message == null)
            {
                return null;
            }
            var result = _serializer.Serialize(message);
            return _encoding.GetBytes(result);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null)
            {
                return default(T);
            }
            return _serializer.Deserialize<T>(_encoding.GetString(bytes));
        }
    }
}
