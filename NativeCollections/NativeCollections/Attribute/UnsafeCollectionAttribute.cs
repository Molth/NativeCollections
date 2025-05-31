using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe collection attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class UnsafeCollectionAttribute : Attribute
    {
        /// <summary>
        ///     Type
        /// </summary>
        public readonly FromType Type;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="type">Type</param>
        public UnsafeCollectionAttribute(FromType type) => Type = type;
    }
}