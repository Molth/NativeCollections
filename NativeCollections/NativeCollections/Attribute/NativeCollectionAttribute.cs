using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Specifies that the decorated struct is a native collection wrapper that holds a handle pointer
    ///     to manage the underlying unmanaged resource.
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