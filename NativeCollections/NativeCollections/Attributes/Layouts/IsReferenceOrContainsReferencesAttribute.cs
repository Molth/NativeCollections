using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the decorated struct contains managed references.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    internal sealed class IsReferenceOrContainsReferencesAttribute : Attribute
    {
    }
}