using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the decorated constructor or parameter must be zero-initialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Parameter)]
    internal sealed class MustBeZeroedAttribute : Attribute
    {
    }
}