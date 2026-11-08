using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Specifies that the decorated struct is a stack-allocated collection that expects a fixed buffer
    ///     to be provided by the caller (e.g., via <see langword="stackalloc" /> or a fixed managed buffer).
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    internal sealed class StackallocCollectionAttribute : Attribute
    {
        /// <summary>
        ///     Gets the origin type of the collection implementation.
        /// </summary>
        public readonly FromType Type;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="type">The origin type of the collection implementation.</param>
        public StackallocCollectionAttribute(FromType type) => Type = type;
    }
}