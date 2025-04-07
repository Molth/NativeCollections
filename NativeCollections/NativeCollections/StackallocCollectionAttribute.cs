using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Stackalloc collection attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class StackallocCollectionAttribute : Attribute
    {
        /// <summary>
        ///     Type
        /// </summary>
        public readonly FromType Type;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="type">Type</param>
        public StackallocCollectionAttribute(FromType type) => Type = type;
    }
}