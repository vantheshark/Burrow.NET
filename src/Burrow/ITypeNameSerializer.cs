using System;

namespace Burrow
{
    public interface ITypeNameSerializer
    {
        string Serialize(Type type);
    }
}