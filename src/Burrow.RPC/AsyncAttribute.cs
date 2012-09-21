using System;
using System.Diagnostics.CodeAnalysis;

namespace Burrow.RPC
{
    /// <summary>
    /// Use this attribute to decorate on method which does not have return type to make the rpc call asynchronously
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Method)]
    public class AsyncAttribute : Attribute
    {
    }
}