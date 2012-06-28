using System;
using System.Text;

namespace Burrow
{
    public static class ByteArrayStringConverter
    {
        internal const int OneMegabyte = 1024*1024;

        public static string ToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes) ;
        }

        public static string ToString(byte[] bytes, int maxSize)
        {
            return Encoding.UTF8.GetString(bytes, 0, maxSize);
        }

        public static string TryConvert(byte[] bytes, int maxSize)
        {
            try
            {
                return ToString(bytes);
            }
            catch (OutOfMemoryException)
            {
                try
                {
                    return ToString(bytes, maxSize);
                }
                catch (OutOfMemoryException)
                {
                    return string.Format("ERROR CONVERTING {0} BYTES TO STRING", bytes.Length);
                }
            }
        }
    }
}
