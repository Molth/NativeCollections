using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the decorated type contains references or is a reference type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    internal sealed class IsReferenceOrContainsReferencesAttribute : Attribute
    {
    }
}