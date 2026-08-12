using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Specifies that the decorated struct is a stack-allocated collection that expects a fixed buffer
    ///     to be provided by the caller (e.g., via <c>stackalloc</c> or a fixed managed buffer).
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class StackallocCollectionAttribute : Attribute
    {
        /// <summary>
        ///     Gets the origin type of the collection implementation.
        /// </summary>
        public readonly FromType Type;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public StackallocCollectionAttribute(FromType type) => Type = type;
    }
}