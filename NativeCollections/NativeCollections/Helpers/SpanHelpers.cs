using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Span helpers
    /// </summary>
    internal static class SpanHelpers
    {
        /// <summary>
        ///     Fills the contents of this buffer with the given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(ref T refData, nuint numElements, T value) where T : unmanaged
        {
            if (Environment.Is64BitProcess || NativeMemoryAllocator.AlignOf<T>() == 1)
            {
                for (nuint count; numElements > 0; numElements -= count, refData = ref Unsafe.Add(ref refData, (nint)count))
                {
                    count = numElements > int.MaxValue ? int.MaxValue : numElements;
                    MemoryMarshal.CreateSpan(ref refData, (int)count).Fill(value);
                }

                return;
            }

            for (nuint i = 0; i < numElements; ++i, refData = ref Unsafe.Add(ref refData, new IntPtr(1)))
                UnsafeHelpers.WriteUnaligned(ref refData, value);
        }

        /// <summary>Searches for any value other than the specified <paramref name="value" />.</summary>
        /// <param name="buffer">The span to search.</param>
        /// <param name="value">The value to exclude from the search.</param>
        /// <typeparam name="T" />
        /// <returns>
        ///     <see langword="true" /> if any value other than <paramref name="value" /> is present in the span.
        ///     If all of the values are <paramref name="value" />, returns <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyExcept<T>(ReadOnlySpan<T> buffer, T value) where T : unmanaged, IEquatable<T>
        {
#if NET8_0_OR_GREATER
            return buffer.ContainsAnyExcept(value);
#elif NET7_0_OR_GREATER
            return buffer.IndexOfAnyExcept(value) >= 0;
#else
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            for (var i = 0; i < buffer.Length; ++i)
            {
                if (!Unsafe.Add(ref reference, (nint)i).Equals(value))
                    return true;
            }

            return false;
#endif
        }

        /// <summary>
        ///     Searches for the specified value and returns true if found. If not found, returns false. Values are compared using
        ///     IEquatable{T}.Equals(T).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(ReadOnlySpan<T> buffer, T value) where T : unmanaged, IEquatable<T>
        {
#if NET6_0_OR_GREATER
            return buffer.Contains(value);
#else
            return buffer.IndexOf(value) >= 0;
#endif
        }

        /// <summary>
        ///     Determines whether two sequences are equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(ref byte left, ref byte right, nuint byteCount)
        {
            for (nuint count; byteCount > 0; byteCount -= count, left = ref Unsafe.AddByteOffset(ref left, (nint)count), right = ref Unsafe.AddByteOffset(ref right, (nint)count))
            {
                count = byteCount > int.MaxValue ? int.MaxValue : byteCount;
                if (!MemoryMarshal.CreateReadOnlySpan(ref left, (int)count).SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref right, (int)count)))
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Determines the relative order of the sequences.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(ref byte left, ref byte right, nuint byteCount)
        {
            var comparison = 0;
            for (nuint count; byteCount > 0 && comparison == 0; byteCount -= count, left = ref Unsafe.AddByteOffset(ref left, (nint)count), right = ref Unsafe.AddByteOffset(ref right, (nint)count))
            {
                count = byteCount > int.MaxValue ? int.MaxValue : byteCount;
                comparison = MemoryMarshal.CreateReadOnlySpan(ref left, (int)count).SequenceCompareTo(MemoryMarshal.CreateReadOnlySpan(ref right, (int)count));
            }

            return comparison;
        }
    }
}