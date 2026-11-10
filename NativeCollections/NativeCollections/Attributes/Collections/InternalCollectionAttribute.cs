using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Specifies that the decorated struct is an internal implementation type
    ///     that is not part of the public API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    internal sealed class InternalCollectionAttribute : Attribute
    {
        /// <summary>
        ///     Gets the origin type of the collection implementation.
        /// </summary>
        public readonly FromType Type;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="type">The origin type of the collection implementation.</param>
        public InternalCollectionAttribute(FromType type) => Type = type;
    }
}