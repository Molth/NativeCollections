using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Is assignable to attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class IsAssignableToAttribute : Attribute
    {
        /// <summary>
        ///     Types
        /// </summary>
        public readonly Type[]? Types;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="types">Types</param>
        public IsAssignableToAttribute(params Type[]? types) => Types = types;
    }
}