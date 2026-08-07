using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the target type or method is customizable, optionally specifying which methods can be customized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
    public sealed class CustomizableAttribute : Attribute
    {
        /// <summary>
        ///     Methods
        /// </summary>
        public readonly string[]? Methods;

        /// <summary>
        ///     Structure
        /// </summary>
        public CustomizableAttribute()
        {
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="methods">Methods</param>
        public CustomizableAttribute(params string[]? methods) => Methods = methods;
    }
}