using System;

// ReSharper disable ALL

namespace NativeCollections
{
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class UnsafeCollectionAttribute : Attribute
    {
        /// <summary>
        ///     Type
        /// </summary>
        public readonly NativeCollectionType Type;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="type">Type</param>
        public UnsafeCollectionAttribute(NativeCollectionType type) => Type = type;
    }

    /// <summary>
    ///     Native collection attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class NativeCollectionAttribute : Attribute
    {
        /// <summary>
        ///     Type
        /// </summary>
        public readonly NativeCollectionType Type;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="type">Type</param>
        public NativeCollectionAttribute(NativeCollectionType type) => Type = type;
    }
}