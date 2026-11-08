using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.PaddingHelpers;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a cache-line-padded wrapper around an atomic pointer.
    /// </summary>
    /// <typeparam name="T">The unmanaged type pointed to by the atomic value.</typeparam>
    [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
    internal unsafe struct CachePaddedAtomicPtr<T> where T : unmanaged
    {
        /// <summary>
        ///     The underlying atomic pointer.
        /// </summary>
        private UnsafeAtomicPtr<T> _data;

        /// <summary>
        ///     Returns a reference to the underlying atomic pointer.
        /// </summary>
        /// <returns>A reference to the atomic pointer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T* AsRef() => ref _data.AsRef();

        /// <summary>
        ///     Returns a pointer, loaded as an atomic operation.
        /// </summary>
        /// <param name="order">The memory ordering constraint applied to the load operation.</param>
        /// <returns>The loaded pointer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Load(Ordering order) => _data.Load(order);

        /// <summary>
        ///     Sets a pointer to a specified value and returns the original pointer, as an atomic operation.
        /// </summary>
        /// <param name="value">The pointer to store.</param>
        /// <returns>The original pointer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Exchange(T* value) => _data.Exchange(value);

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <param name="value">
        ///     The value that replaces the destination value if the comparison results in equality.
        /// </param>
        /// <param name="comparand">The value that is compared to the value at the destination.</param>
        /// <returns>The original pointer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* CompareExchange(T* value, T* comparand) => _data.CompareExchange(value, comparand);
    }
}