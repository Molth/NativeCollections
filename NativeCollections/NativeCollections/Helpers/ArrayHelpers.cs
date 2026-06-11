using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Array helpers
    /// </summary>
    internal static class ArrayHelpers
    {
        /// <summary>Gets the maximum number of elements that may be contained in an array.</summary>
        /// <returns>The maximum count of elements allowed in any array.</returns>
        /// <remarks>
        ///     <para>
        ///         This property represents a runtime limitation, the maximum number of elements (not bytes)
        ///         the runtime will allow in an array. There is no guarantee that an allocation under this length
        ///         will succeed, but all attempts to allocate a larger array will fail.
        ///     </para>
        ///     <para>
        ///         This property only applies to single-dimension, zero-bound (SZ) arrays.
        ///         <see cref="Array.Length" /> property may return larger value than this property for multi-dimensional arrays.
        ///     </para>
        /// </remarks>
        public static int MaxLength
        {
            get
            {
#if NET6_0_OR_GREATER
                return Array.MaxLength;
#else
                return 2147483591;
#endif
            }
        }
    }
}