using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Is reference or contains references
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class IsReferenceOrContainsReferencesAttribute : Attribute
    {
    }
}