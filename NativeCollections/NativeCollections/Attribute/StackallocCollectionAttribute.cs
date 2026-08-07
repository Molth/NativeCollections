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
        ///     Type
        /// </summary>
        public readonly FromType Type;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="type">Type</param>
        public StackallocCollectionAttribute(FromType type) => Type = type;
    }
}