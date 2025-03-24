using System;

// ReSharper disable ALL

namespace NativeCollections
{
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