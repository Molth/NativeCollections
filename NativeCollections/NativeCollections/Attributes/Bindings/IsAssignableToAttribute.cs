using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Specifies one or more types that the decorated struct is assignable to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    internal sealed class IsAssignableToAttribute : Attribute
    {
        /// <summary>
        ///     Gets the specified one or more types.
        /// </summary>
        public readonly Type[]? Types;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="types">The types to which the decorated struct is assignable.</param>
        public IsAssignableToAttribute(params Type[]? types) => Types = types;
    }
}