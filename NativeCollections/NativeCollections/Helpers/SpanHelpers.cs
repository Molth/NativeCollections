using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides low-level memory manipulation utilities for spans.
    /// </summary>
    internal static unsafe class SpanHelpers
    {
        /// <summary>
        ///     Copies bytes from the source address to the destination address
        ///     without assuming architecture dependent alignment of the addresses.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(ref byte destination, ref byte source, uint byteCount) => Unsafe.CopyBlockUnaligned(ref destination, ref source, byteCount);

        /// <summary>
        ///     Copies a block of memory from memory location <paramref name="source" />
        ///     to memory location <paramref name="destination" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Move(void* destination, void* source, uint byteCount)
        {
#if NET7_0_OR_GREATER
            NativeMemory.Copy(source, destination, byteCount);
#else
            Buffer.MemoryCopy(source, destination, byteCount, byteCount);
#endif
        }

        /// <summary>
        ///     Copies a block of memory from memory location <paramref name="source" />
        ///     to memory location <paramref name="destination" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Move(ref byte destination, ref byte source, uint byteCount)
        {
            fixed (void* pinnedDestination = &destination)
            {
                fixed (void* pinnedSource = &source)
                {
                    Move(pinnedDestination, pinnedSource, byteCount);
                }
            }
        }

        /// <summary>
        ///     Initializes a block of memory at the given location with a given initial value
        ///     without assuming architecture dependent alignment of the address.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(ref byte startAddress, byte value, uint byteCount) => Unsafe.InitBlockUnaligned(ref startAddress, value, byteCount);

        /// <summary>
        ///     Fills the contents of this buffer with the given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(ref T refData, uint numElements, T value) where T : unmanaged
        {
            if (Environment.Is64BitProcess || NativeMemoryAllocator.AlignOf<T>() == 1 || NativeMemoryAllocator.IsAligned(Unsafe.AsPointer(ref refData), NativeMemoryAllocator.AlignOf<T>()))
            {
                for (uint count; numElements > 0; numElements -= count, refData = ref Unsafe.Add(ref refData, (nint)count))
                {
                    count = numElements > int.MaxValue ? int.MaxValue : numElements;
                    MemoryMarshal.CreateSpan(ref refData, (int)count).Fill(value);
                }

                return;
            }

            for (uint i = 0; i < numElements; ++i, refData = ref Unsafe.Add(ref refData, new IntPtr(1)))
                UnsafeHelpers.WriteUnaligned(ref refData, value);
        }

        /// <summary>
        ///     Searches for any value other than the specified <paramref name="value" />.
        /// </summary>
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
        ///     Searches for the specified value and returns true if found.
        ///     If not found, returns false.
        ///     Values are compared using IEquatable{T}.Equals(T).
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
        public static bool Equals(ref byte left, ref byte right, uint byteCount)
        {
            for (uint count; byteCount > 0; byteCount -= count, left = ref Unsafe.AddByteOffset(ref left, (nint)count), right = ref Unsafe.AddByteOffset(ref right, (nint)count))
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
        public static int Compare(ref byte left, ref byte right, uint byteCount)
        {
            var comparison = 0;
            for (uint count; byteCount > 0 && comparison == 0; byteCount -= count, left = ref Unsafe.AddByteOffset(ref left, (nint)count), right = ref Unsafe.AddByteOffset(ref right, (nint)count))
            {
                count = byteCount > int.MaxValue ? int.MaxValue : byteCount;
                comparison = MemoryMarshal.CreateReadOnlySpan(ref left, (int)count).SequenceCompareTo(MemoryMarshal.CreateReadOnlySpan(ref right, (int)count));
            }

            return comparison;
        }

        /// <summary>
        ///     Determines whether two values are equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<T>(ref T left, ref T right) where T : unmanaged => Equals(ref Unsafe.As<T, byte>(ref left), ref Unsafe.As<T, byte>(ref right), (uint)Unsafe.SizeOf<T>());

        /// <summary>
        ///     Determines the relative order of the values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare<T>(ref T left, ref T right) where T : unmanaged => Compare(ref Unsafe.As<T, byte>(ref left), ref Unsafe.As<T, byte>(ref right), (uint)Unsafe.SizeOf<T>());
    }
}