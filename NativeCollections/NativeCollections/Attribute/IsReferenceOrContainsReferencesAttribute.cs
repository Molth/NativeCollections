using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the decorated type contains references or is a reference type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class IsReferenceOrContainsReferencesAttribute : Attribute
    {
    }
}