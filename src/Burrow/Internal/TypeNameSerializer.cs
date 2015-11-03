using System;

namespace Burrow.Internal
{
    internal class TypeNameSerializer : ITypeNameSerializer
    {
        public string Serialize(Type type)
        {
            if (type?.FullName == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return type.FullName.Replace('.', '_') + ":" + type.Assembly.GetName().Name.Replace('.', '_');
        }
    }
}
