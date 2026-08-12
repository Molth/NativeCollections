using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the decorated field of type <see cref="IntPtr" /> represents a pointer
    ///     to the specified unmanaged type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class NativePointerAttribute : Attribute
    {
        /// <summary>
        ///     Gets the specified unmanaged type.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public NativePointerAttribute(Type type) => Type = type;
    }
}