using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the decorated constructor or parameter must be zero-initialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Parameter)]
    public sealed class MustBeZeroedAttribute : Attribute
    {
        /// <summary>
        ///     Gets the name of the parameter.
        /// </summary>
        public readonly string? Parameter;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public MustBeZeroedAttribute()
        {
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public MustBeZeroedAttribute(string parameter) => Parameter = parameter;
    }
}