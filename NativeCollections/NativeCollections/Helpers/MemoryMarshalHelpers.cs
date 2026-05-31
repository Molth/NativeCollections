using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Memory marshal helpers
    /// </summary>
    internal static class MemoryMarshalHelpers
    {
        /// <summary>
        ///     Re-interprets a span of bytes as a reference to structure of type T.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(Span<byte> buffer) where T : unmanaged => ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(buffer));

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
            if (array == null)
                ThrowHelpers.ThrowNullReferenceException();
            return ref MemoryMarshal.GetReference(array.AsSpan());
#endif
        }
    }
}