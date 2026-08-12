using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the decorated constructor or parameter must have distinct values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Parameter)]
    public sealed class MustBeDistinctAttribute : Attribute
    {
        /// <summary>
        ///     Gets the name of the parameter.
        /// </summary>
        public readonly string? Parameter;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public MustBeDistinctAttribute()
        {
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public MustBeDistinctAttribute(string parameter) => Parameter = parameter;
    }
}