using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native pointer attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class NativePointerAttribute : Attribute
    {
        /// <summary>
        ///     Type
        /// </summary>
        public readonly Type Type;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="type">Type</param>
        public NativePointerAttribute(Type type) => Type = type;
    }
}