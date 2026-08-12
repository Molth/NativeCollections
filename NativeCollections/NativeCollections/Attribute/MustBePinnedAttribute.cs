using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the decorated target must be pinned (fixed in memory) to prevent garbage collection from relocating
    ///     it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class MustBePinnedAttribute : Attribute
    {
        /// <summary>
        ///     Gets the name of the parameter.
        /// </summary>
        public readonly string? Parameter;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public MustBePinnedAttribute()
        {
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public MustBePinnedAttribute(string parameter) => Parameter = parameter;
    }
}