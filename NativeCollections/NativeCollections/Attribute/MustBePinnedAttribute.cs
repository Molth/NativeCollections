using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Must be pinned attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Parameter)]
    public sealed class MustBePinnedAttribute : Attribute
    {
        /// <summary>
        ///     Parameter
        /// </summary>
        public readonly string? Parameter;

        /// <summary>
        ///     Structure
        /// </summary>
        public MustBePinnedAttribute()
        {
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="parameter">Parameter</param>
        public MustBePinnedAttribute(string parameter) => Parameter = parameter;
    }
}