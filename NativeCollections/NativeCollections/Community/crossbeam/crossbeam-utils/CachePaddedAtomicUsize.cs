using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.PaddingHelpers;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a cache-line-padded wrapper around an atomic native-sized unsigned integer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
    internal struct CachePaddedAtomicUsize
    {
        /// <summary>
        ///     The underlying atomic value.
        /// </summary>
        private UnsafeAtomicUsize _data;

        /// <summary>
        ///     Returns a reference to the underlying atomic value.
        /// </summary>
        /// <returns>A reference to the atomic value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref nuint AsRef() => ref _data.AsRef();

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <param name="order">The memory ordering constraint applied to the load operation.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint Load(Ordering order) => _data.Load(order);

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="order">The memory ordering constraint applied to the store operation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(nuint value, Ordering order) => _data.Store(value, order);

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <param name="value">
        ///     The value that replaces the destination value if the comparison results in equality.
        /// </param>
        /// <param name="comparand">The value that is compared to the value at the destination.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint CompareExchange(nuint value, nuint comparand) => _data.CompareExchange(value, comparand);
    }
}