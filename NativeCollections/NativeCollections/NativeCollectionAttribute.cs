using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native collection attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class NativeCollectionAttribute : Attribute
#if NET7_0_OR_GREATER
    ;
#else
    {
    }
#endif
}