using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Must be zeroed attribute
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