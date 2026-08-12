using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Specifies that the decorated struct is a low‑level unsafe collection implemented directly as a
    ///     value type without a handle pointer, offering a lightweight but less managed usage.
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
        public InternalCollectionAttribute(FromType type) => Type = type;
    }
}