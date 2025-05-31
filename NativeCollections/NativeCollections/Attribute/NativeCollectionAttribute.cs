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
        public readonly FromType Type;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="type">Type</param>
        public NativeCollectionAttribute(FromType type) => Type = type;
    }
}