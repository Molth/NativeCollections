using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the decorated field of type <see cref="nint" /> represents a pointer
    ///     to the specified unmanaged type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class NativePointerAttribute : Attribute
    {
        /// <summary>
        ///     Type
        /// </summary>
        public readonly Type Type;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="type">Type</param>
        public NativePointerAttribute(Type type) => Type = type;
    }
}