using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Specifies that the decorated struct is a specialized collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    internal sealed class SpecializedCollectionAttribute : Attribute
    {
        /// <summary>
        ///     Gets the origin type of the collection implementation.
        /// </summary>
        public readonly FromType Type;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="type">The origin type of the collection implementation.</param>
        public SpecializedCollectionAttribute(FromType type) => Type = type;
    }
}