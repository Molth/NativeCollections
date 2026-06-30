using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Must be distinct attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Parameter)]
    public sealed class MustBeDistinctAttribute : Attribute
    {
        /// <summary>
        ///     Parameter
        /// </summary>
        public readonly string? Parameter;

        /// <summary>
        ///     Structure
        /// </summary>
        public MustBeDistinctAttribute()
        {
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="parameter">Parameter</param>
        public MustBeDistinctAttribute(string parameter) => Parameter = parameter;
    }
}