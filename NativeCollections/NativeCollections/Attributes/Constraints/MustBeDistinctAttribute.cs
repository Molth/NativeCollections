using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the decorated constructor, method, or parameter must have distinct values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Parameter)]
    internal sealed class MustBeDistinctAttribute : Attribute
    {
    }
}