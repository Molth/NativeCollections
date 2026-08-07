using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Specifies the associated type that the decorated type is bound to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class BindingTypeAttribute : Attribute
    {
        /// <summary>
        ///     Gets the bound type.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="type">Type</param>
        public BindingTypeAttribute(Type type) => Type = type;
    }
}