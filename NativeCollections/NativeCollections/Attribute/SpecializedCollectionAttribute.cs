using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Specifies that the decorated struct is a specialized collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class SpecializedCollectionAttribute : Attribute
    {
        /// <summary>
        ///     Gets the origin type of the collection implementation.
        /// </summary>
        public readonly FromType Type;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public SpecializedCollectionAttribute(FromType type) => Type = type;
    }
}