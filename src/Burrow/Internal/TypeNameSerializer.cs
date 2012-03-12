using System;

namespace Burrow.Internal
{
    internal class TypeNameSerializer : ITypeNameSerializer
    {
        public string Serialize(Type type)
        {
            if (type == null || type.FullName == null)
            {
                throw new ArgumentNullException("type");
            }
            return type.FullName.Replace('.', '_') + ":" + type.Assembly.GetName().Name.Replace('.', '_');
        }
    }
}
