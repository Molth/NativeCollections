using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a collection of methods for interoperating with <see cref="Memory{T}" />,
    ///     <see cref="ReadOnlyMemory{T}" />, <see cref="Span{T}" />, <see cref="ReadOnlySpan{T}" />.
    /// </summary>
    internal static class MemoryMarshalHelpers
    {
        /// <summary>
        ///     Casts to a Span of byte.
        /// </summary>
        /// <typeparam name="T">The type of the value to reinterpret as bytes.</typeparam>
        /// <param name="value">The value to reinterpret as bytes.</param>
        /// <returns>A span over the bytes that represent <paramref name="value" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> AsBytes<T>(ref T value) => MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref value), Unsafe.SizeOf<T>());

        /// <summary>
        ///     Casts to a ReadOnlySpan of byte.
        /// </summary>
        /// <typeparam name="T">The type of the value to reinterpret as bytes.</typeparam>
        /// <param name="value">The value to reinterpret as bytes.</param>
        /// <returns>A read-only span over the bytes that represent <paramref name="value" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> AsReadOnlyBytes<T>(ref T value) where T : unmanaged => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref value), Unsafe.SizeOf<T>());

        /// <summary>
        ///     Converts a <see cref="ReadOnlySpan{T}" /> to a mutable <see cref="Span{T}" />.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="value">The read-only span to convert.</param>
        /// <returns>A mutable span that references the same memory location as <paramref name="value" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(ReadOnlySpan<T> value) => MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(value), value.Length);

        /// <summary>
        ///     Returns a reference to the 0th element of <paramref name="array" />. If the
        ///     array is empty, returns a reference to where the 0th element would have been
        ///     stored. Such a reference may be used for pinning but must never be dereferenced.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="array">The array whose first element is referenced.</param>
        /// <returns>A reference to the 0th element of <paramref name="array" />.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="array" /> is <see langword="null" /> on .NET versions
        ///     earlier than .NET 5.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     Thrown when <paramref name="array" /> is <see langword="null" /> on .NET 5 and later.
        /// </exception>
        /// <remarks>
        ///     This method does not perform array variance checks. The caller must manually perform
        ///     any array variance checks if the caller wishes to write to the returned reference.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetArrayDataReference<T>(T[] array)
        {
#if NET5_0_OR_GREATER
            return ref MemoryMarshal.GetArrayDataReference(array);
#else
            ThrowHelpers.ThrowIfNull(array, ExceptionArgument.array);
            return ref MemoryMarshal.GetReference(array.AsSpan());
#endif
        }
    }
}