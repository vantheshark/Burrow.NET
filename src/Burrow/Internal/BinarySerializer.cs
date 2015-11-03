using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Burrow.Internal
{
    public class BinarySerializer : ISerializer
    {
        public byte[] Serialize<T>(T message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var formatter = new BinaryFormatter();
            byte[] messageBody;

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, message);
                messageBody = stream.GetBuffer();
            }
            return messageBody;
        }

        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream(bytes))
            {
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
