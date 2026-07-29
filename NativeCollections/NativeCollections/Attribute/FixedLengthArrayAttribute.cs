using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Indicates that the instance's storage is sequentially replicated <see cref="Length" /> times.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    internal sealed class FixedLengthArrayAttribute : Attribute
    {
        /// <summary>
        ///     Creates a new instance with the specified length.
        /// </summary>
        /// <param name="type">The element type of the replicated storage.</param>
        /// <param name="length">The number of sequential fields to replicate in the inline array type.</param>
        public FixedLengthArrayAttribute(Type type, int length)
        {
            Type = type;
            Length = length;
        }

        /// <summary>
        ///     Gets the element type of the replicated storage.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     Gets the number of sequential fields to replicate in the inline array type.
        /// </summary>
        /// <returns>The number of sequential fields to replicate in the inline array type.</returns>
        public int Length { get; }
    }
}