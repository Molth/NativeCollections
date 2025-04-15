using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Must be zeroed attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class MustBeZeroedAttribute : Attribute
    {
        /// <summary>
        ///     Parameter
        /// </summary>
        public readonly string Parameter;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="parameter">Parameter</param>
        public MustBeZeroedAttribute(string parameter) => Parameter = parameter;
    }
}