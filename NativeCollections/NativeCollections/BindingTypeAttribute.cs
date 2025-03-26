using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Binding type attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class BindingTypeAttribute : Attribute
    {
        /// <summary>
        ///     Type
        /// </summary>
        public readonly Type Type;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="type">Type</param>
        public BindingTypeAttribute(Type type) => Type = type;
    }
}