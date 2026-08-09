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
        ///     Type
        /// </summary>
        public readonly FromType Type;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="type">Type</param>
        public SpecializedCollectionAttribute(FromType type) => Type = type;
    }
}