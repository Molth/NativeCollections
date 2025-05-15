using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Customizable attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class CustomizableAttribute : Attribute
    {
        /// <summary>
        ///     Methods
        /// </summary>
        public readonly string[]? Methods;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="methods">Methods</param>
        public CustomizableAttribute(params string[]? methods) => Methods = methods;
    }
}