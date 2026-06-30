using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Supported types attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class SupportedTypesAttribute : Attribute
    {
        /// <summary>
        ///     Type
        /// </summary>
        public readonly Type[]? Type;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="type">Type</param>
        public SupportedTypesAttribute(params Type[]? type) => Type = type;
    }
}