using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Specifies that the decorated type is a native collection wrapper that
    ///     holds a handle pointer to manage the underlying unmanaged resource.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    internal sealed class NativeCollectionAttribute : Attribute
    {
        /// <summary>
        ///     Gets the origin type of the collection implementation.
        /// </summary>
        public readonly FromType Type;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="type">The origin type of the collection implementation.</param>
        public NativeCollectionAttribute(FromType type) => Type = type;
    }
}