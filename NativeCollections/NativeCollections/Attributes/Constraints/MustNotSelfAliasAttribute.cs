using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the decorated parameter must not alias the storage
    ///     of the instance it is passed to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class MustNotSelfAliasAttribute : Attribute
    {
    }
}