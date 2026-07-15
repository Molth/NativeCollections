using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Maybe supported types attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class MaybeSupportedTypesAttribute : Attribute
    {
        /// <summary>
        ///     Types
        /// </summary>
        public readonly Type[]? Types;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="types">Types</param>
        public MaybeSupportedTypesAttribute(params Type[]? types) => Types = types;
    }
}