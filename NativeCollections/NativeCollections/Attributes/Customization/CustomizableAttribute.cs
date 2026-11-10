using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the target type or method is customizable,
    ///     optionally specifying which methods can be customized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
    internal sealed class CustomizableAttribute : Attribute
    {
        /// <summary>
        ///     Gets the signatures of the specified one or more methods.
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
        /// <param name="methods">The signatures of the methods that can be customized.</param>
        public CustomizableAttribute(params string[]? methods) => Methods = methods;
    }
}