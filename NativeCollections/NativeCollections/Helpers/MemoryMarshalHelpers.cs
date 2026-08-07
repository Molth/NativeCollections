using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a collection of methods for interoperating with
    ///     <see cref="Memory{T}" />,
    ///     <see cref="ReadOnlyMemory{T}" />,
    ///     <see cref="Span{T}" />,
    ///     <see cref="ReadOnlySpan{T}" />.
    /// </summary>
    internal static class MemoryMarshalHelpers
    {
        /// <summary>
        ///     Returns a reference to the 0th element of <paramref name="array" />. If the array is empty, returns a reference to
        ///     where the 0th element
        ///     would have been stored. Such a reference may be used for pinning but must never be dereferenced.
        /// </summary>
        /// <exception cref="NullReferenceException"><paramref name="array" /> is <see langword="null" />.</exception>
        /// <remarks>
        ///     This method does not perform array variance checks. The caller must manually perform any array variance checks
        ///     if the caller wishes to write to the returned reference.
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