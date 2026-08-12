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
        ///     Gets the names of the specified one or more methods.
        /// </summary>
        public readonly string[]? Methods;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public CustomizableAttribute()
        {
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public CustomizableAttribute(params string[]? methods) => Methods = methods;
    }
}