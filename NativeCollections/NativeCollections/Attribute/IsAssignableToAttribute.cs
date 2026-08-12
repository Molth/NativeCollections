using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Specifies one or more types that the decorated struct is assignable to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class IsAssignableToAttribute : Attribute
    {
        /// <summary>
        ///     Gets the specified one or more types.
        /// </summary>
        public readonly Type[]? Types;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public IsAssignableToAttribute(params Type[]? types) => Types = types;
    }
}