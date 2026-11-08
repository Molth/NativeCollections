using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.PaddingHelpers;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a cache-line-padded wrapper around an atomic 32-bit unsigned integer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
    internal struct CachePaddedAtomicU32
    {
        /// <summary>
        ///     The underlying atomic value.
        /// </summary>
        private UnsafeAtomicU32 _data;

        /// <summary>
        ///     Adds the specified value to the current value and returns the new value, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to add to the current value.</param>
        /// <returns>The new value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add(uint value) => _data.Add(value);

        /// <summary>
        ///     Subtracts the specified value from the current value
        ///     and returns the new value, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to subtract from the current value.</param>
        /// <returns>The new value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Sub(uint value) => _data.Sub(value);

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <param name="order">The memory ordering constraint applied to the load operation.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Load(Ordering order) => _data.Load(order);

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <param name="value">
        ///     The value that replaces the destination value if the comparison results in equality.
        /// </param>
        /// <param name="comparand">The value that is compared to the value at the destination.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint CompareExchange(uint value, uint comparand) => _data.CompareExchange(value, comparand);
    }
}