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
        /// <param name="destination">A reference to the first byte of the destination memory.</param>
        /// <param name="source">A reference to the first byte of the source memory.</param>
        /// <param name="byteCount">The number of bytes to copy.</param>
        /// <remarks>
        ///     This method performs no bounds validation; callers must ensure that
        ///     <paramref name="source" /> refers to a readable block of at least
        ///     <paramref name="byteCount" /> bytes and that <paramref name="destination" />
        ///     refers to a writable block of at least <paramref name="byteCount" /> bytes.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(ref byte destination, ref byte source, nuint byteCount)
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
        ///     Copies a block of memory from memory location <paramref name="source" />
        ///     to memory location <paramref name="destination" />.
        /// </summary>
        /// <param name="destination">A reference to the first byte of the destination memory.</param>
        /// <param name="source">A reference to the first byte of the source memory.</param>
        /// <param name="byteCount">The number of bytes to copy.</param>
        /// <remarks>
        ///     This method performs no bounds validation; callers must ensure that
        ///     <paramref name="source" /> refers to a readable block of at least
        ///     <paramref name="byteCount" /> bytes and that <paramref name="destination" />
        ///     refers to a writable block of at least <paramref name="byteCount" /> bytes.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Move(ref byte destination, ref byte source, nuint byteCount)
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
        ///     Copies a block of memory from memory location <paramref name="source" />
        ///     to memory location <paramref name="destination" />.
        /// </summary>
        /// <param name="destination">A pointer to the destination memory.</param>
        /// <param name="source">A pointer to the source memory.</param>
        /// <param name="byteCount">The number of bytes to copy.</param>
        /// <remarks>
        ///     This method performs no bounds validation; callers must ensure that
        ///     <paramref name="source" /> points to a readable block of at least
        ///     <paramref name="byteCount" /> bytes and that <paramref name="destination" />
        ///     points to a writable block of at least <paramref name="byteCount" /> bytes.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Move(void* destination, void* source, nuint byteCount)
        {
#if NET7_0_OR_GREATER
            NativeMemory.Copy(source, destination, byteCount);
#else
            Buffer.MemoryCopy(source, destination, U64.FromUsizeUnchecked(byteCount), U64.FromUsizeUnchecked(byteCount));
#endif
        }

        /// <summary>
        ///     Initializes a block of memory at the given location with a given initial
        ///     value without assuming architecture dependent alignment of the address.
        /// </summary>
        /// <param name="startAddress">A reference to the first byte of the memory to initialize.</param>
        /// <param name="value">The value to assign to each byte.</param>
        /// <param name="byteCount">The number of bytes to initialize.</param>
        /// <remarks>
        ///     This method performs no bounds validation; callers must ensure that <paramref name="startAddress" />
        ///     refers to a writable block of at least <paramref name="byteCount" /> bytes.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(ref byte startAddress, byte value, nuint byteCount)
        {
#if NET7_0_OR_GREATER
            fixed (void* pinnedStartAddress = &startAddress)
            {
                NativeMemory.Fill(pinnedStartAddress, byteCount, value);
            }
#else
            for (uint count; byteCount > Usize.FromI32Unchecked(0); byteCount = byteCount.WrappingSub(Usize.FromU32Unchecked(count)), startAddress = ref Unsafe.AddByteOffset(ref startAddress, Isize.FromU32Unchecked(count)))
            {
                count = byteCount > Usize.FromU32Unchecked(uint.MaxValue) ? uint.MaxValue : U32.FromUsizeUnchecked(byteCount);
                Unsafe.InitBlockUnaligned(ref startAddress, value, count);
            }
#endif
        }

        /// <summary>
        ///     Fills the contents of this buffer with the given value.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the values to write.</typeparam>
        /// <param name="refData">A reference to the first element of the memory to fill.</param>
        /// <param name="numElements">The number of elements to fill.</param>
        /// <param name="value">The value to assign to each element.</param>
        /// <remarks>
        ///     This method performs no bounds validation; callers must ensure that <paramref name="refData" />
        ///     refers to a writable block of at least <paramref name="numElements" /> elements.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(ref T refData, nuint numElements, T value) where T : unmanaged
        {
            if (Unsafe.SizeOf<T>() == 1)
            {
                Set(ref Unsafe.As<T, byte>(ref refData), Unsafe.As<T, byte>(ref value), numElements);
                return;
            }

            fixed (void* pinnedRefData = &refData)
            {
                if (NativeMemoryAllocator.IsAligned(pinnedRefData, NativeMemoryAllocator.AlignOf<T>()))
                {
                    refData = ref Unsafe.AsRef<T>(pinnedRefData);
                    for (int count; numElements > Usize.FromI32Unchecked(0); numElements = numElements.WrappingSub(Usize.FromI32Unchecked(count)), refData = ref Unsafe.Add(ref refData, Isize.FromI32Unchecked(count)))
                    {
                        count = numElements > Usize.FromI32Unchecked(int.MaxValue) ? int.MaxValue : I32.FromUsizeUnchecked(numElements);
                        MemoryMarshal.CreateSpan(ref refData, count).Fill(value);
                    }

                    return;
                }
            }

            for (var i = Usize.FromI32Unchecked(0); i < numElements; i = i.WrappingAdd(Usize.FromI32Unchecked(1)), refData = ref Unsafe.Add(ref refData, Isize.FromI32Unchecked(1)))
                UnsafeHelpers.WriteUnaligned(ref refData, value);
        }

        /// <summary>
        ///     Determines whether two sequences are equal.
        /// </summary>
        /// <param name="left">A reference to the first sequence.</param>
        /// <param name="right">A reference to the second sequence.</param>
        /// <param name="byteCount">The number of bytes to compare.</param>
        /// <returns><see langword="true" /> if the sequences are equal; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(ref byte left, ref byte right, nuint byteCount)
        {
            for (int count; byteCount > Usize.FromI32Unchecked(0); byteCount = byteCount.WrappingSub(Usize.FromI32Unchecked(count)), left = ref Unsafe.Add(ref left, Isize.FromI32Unchecked(count)), right = ref Unsafe.Add(ref right, Isize.FromI32Unchecked(count)))
            {
                count = byteCount > Usize.FromI32Unchecked(int.MaxValue) ? int.MaxValue : I32.FromUsizeUnchecked(byteCount);
                if (!MemoryMarshal.CreateReadOnlySpan(ref left, count).SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref right, count)))
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Determines the relative order of the sequences.
        /// </summary>
        /// <param name="left">A reference to the first sequence.</param>
        /// <param name="right">A reference to the second sequence.</param>
        /// <param name="byteCount">The number of bytes to compare.</param>
        /// <returns>A value that indicates the relative order of the sequences.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(ref byte left, ref byte right, nuint byteCount)
        {
            var comparison = 0;
            for (int count; byteCount > Usize.FromI32Unchecked(0) && comparison == 0; byteCount = byteCount.WrappingSub(Usize.FromI32Unchecked(count)), left = ref Unsafe.Add(ref left, Isize.FromI32Unchecked(count)), right = ref Unsafe.Add(ref right, Isize.FromI32Unchecked(count)))
            {
                count = byteCount > Usize.FromI32Unchecked(int.MaxValue) ? int.MaxValue : I32.FromUsizeUnchecked(byteCount);
                comparison = MemoryMarshal.CreateReadOnlySpan(ref left, count).SequenceCompareTo(MemoryMarshal.CreateReadOnlySpan(ref right, count));
            }

            return comparison;
        }

        /// <summary>
        ///     Determines whether two values are equal.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to compare.</typeparam>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true" /> if the values are equal; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<T>(ref T left, ref T right) where T : unmanaged => Equals(ref Unsafe.As<T, byte>(ref left), ref Unsafe.As<T, byte>(ref right), Usize.FromI32Unchecked(Unsafe.SizeOf<T>()));

        /// <summary>
        ///     Determines the relative order of the values.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to compare.</typeparam>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>A value that indicates the relative order of the values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare<T>(ref T left, ref T right) where T : unmanaged => Compare(ref Unsafe.As<T, byte>(ref left), ref Unsafe.As<T, byte>(ref right), Usize.FromI32Unchecked(Unsafe.SizeOf<T>()));

        /// <summary>
        ///     Searches for any value other than the specified <paramref name="value" />.
        /// </summary>
        /// <typeparam name="T">The type of the values in the span.</typeparam>
        /// <param name="buffer">The span to search.</param>
        /// <param name="value">The value to exclude from the search.</param>
        /// <returns>
        ///     <see langword="true" /> if any value other than <paramref name="value" /> is present in the
        ///     span. If all of the values are <paramref name="value" />, returns <see langword="false" />.
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
            for (var i = 0; i < buffer.Length; i = i.WrappingAdd(1))
            {
                if (!Unsafe.Add(ref reference, Isize.FromI32Unchecked(i)).Equals(value))
                    return true;
            }

            return false;
#endif
        }

        /// <summary>
        ///     Searches for the specified value and returns <see langword="true" /> if found. If not
        ///     found, returns <see langword="false" />. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <typeparam name="T">The type of the values in the span.</typeparam>
        /// <param name="buffer">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="value" /> is found in
        ///     <paramref name="buffer" />; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(ReadOnlySpan<T> buffer, T value) where T : unmanaged, IEquatable<T>
        {
#if NET6_0_OR_GREATER
            return buffer.Contains(value);
#else
            return buffer.IndexOf(value) >= 0;
#endif
        }
    }
}