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
        ///     Parameter
        /// </summary>
        public readonly string? Parameter;

        /// <summary>
        ///     Structure
        /// </summary>
        public MustBeZeroedAttribute()
        {
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="parameter">Parameter</param>
        public MustBeZeroedAttribute(string parameter) => Parameter = parameter;
    }
}