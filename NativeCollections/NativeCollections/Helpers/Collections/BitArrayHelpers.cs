using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics;
#if !NET7_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
#endif
#endif

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides helper methods for bit array operations.
    /// </summary>
    internal static class BitArrayHelpers
    {
        /// <summary>
        ///     Performs the bitwise AND operation between the elements of <paramref name="destination" />
        ///     and the corresponding elements of <paramref name="source" />, storing the result in
        ///     <paramref name="destination" />.
        /// </summary>
        /// <param name="destination">The span of 32-bit words that receives the result of the operation.</param>
        /// <param name="source">The span of 32-bit words to combine with <paramref name="destination" />.</param>
        /// <param name="count">The number of 32-bit words to process, starting at index <c>0</c>.</param>
        /// <remarks>
        ///     The span lengths are validated only for counts of seven or fewer, which are accessed
        ///     through the span indexers; larger counts are accessed through references without span
        ///     validation, so both spans must contain at least <paramref name="count" /> elements.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void And(Span<int> destination, Span<int> source, uint count)
        {
            switch (count)
            {
                case 7U:
                    destination[6] &= source[6];
                    goto case 6U;

                case 6U:
                    destination[5] &= source[5];
                    goto case 5U;

                case 5U:
                    destination[4] &= source[4];
                    goto case 4U;

                case 4U:
                    destination[3] &= source[3];
                    goto case 3U;

                case 3U:
                    destination[2] &= source[2];
                    goto case 2U;

                case 2U:
                    destination[1] &= source[1];
                    goto case 1U;

                case 1U:
                    destination[0] &= source[0];
                    return;

                case 0U:
                    return;
            }

            ref var left = ref MemoryMarshal.GetReference(destination);
            ref var right = ref MemoryMarshal.GetReference(source);
            var i = 0U;
#if NET5_0_OR_GREATER
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated && count >= U32.FromI32Unchecked(Vector512<int>.Count))
            {
                var n = count.WrappingSub(U32.FromI32Unchecked(Vector512<int>.Count).WrappingSub(1U));
                for (; i < n; i = i.WrappingAdd(U32.FromI32Unchecked(Vector512<int>.Count)))
                {
                    var result = Vector512.LoadUnsafe(ref left, Usize.FromU32Unchecked(i)) & Vector512.LoadUnsafe(ref right, Usize.FromU32Unchecked(i));
                    result.StoreUnsafe(ref left, Usize.FromU32Unchecked(i));
                }
            }
            else
#endif
            if (IsHardwareAccelerated256 && count >= U32.FromI32Unchecked(Vector256<int>.Count))
            {
                var n = count.WrappingSub(U32.FromI32Unchecked(Vector256<int>.Count).WrappingSub(1U));
                for (; i < n; i = i.WrappingAdd(U32.FromI32Unchecked(Vector256<int>.Count)))
                {
                    var result = And(Load256(ref left, Usize.FromU32Unchecked(i)), Load256(ref right, Usize.FromU32Unchecked(i)));
                    Store256(ref left, Usize.FromU32Unchecked(i), result);
                }
            }
            else if (IsHardwareAccelerated128 && count >= U32.FromI32Unchecked(Vector128<int>.Count))
            {
                var n = count.WrappingSub(U32.FromI32Unchecked(Vector128<int>.Count).WrappingSub(1U));
                for (; i < n; i = i.WrappingAdd(U32.FromI32Unchecked(Vector128<int>.Count)))
                {
                    var result = And(Load128(ref left, Usize.FromU32Unchecked(i)), Load128(ref right, Usize.FromU32Unchecked(i)));
                    Store128(ref left, Usize.FromU32Unchecked(i), result);
                }
            }
#endif
            for (; i < count; i = i.WrappingAdd(1U))
                Unsafe.Add(ref left, Isize.FromU32Unchecked(i)) &= Unsafe.Add(ref right, Isize.FromU32Unchecked(i));
        }

        /// <summary>
        ///     Performs the bitwise OR operation between the elements of <paramref name="destination" />
        ///     and the corresponding elements of <paramref name="source" />, storing the result in
        ///     <paramref name="destination" />.
        /// </summary>
        /// <param name="destination">The span of 32-bit words that receives the result of the operation.</param>
        /// <param name="source">The span of 32-bit words to combine with <paramref name="destination" />.</param>
        /// <param name="count">The number of 32-bit words to process, starting at index <c>0</c>.</param>
        /// <remarks>
        ///     The span lengths are validated only for counts of seven or fewer, which are accessed
        ///     through the span indexers; larger counts are accessed through references without span
        ///     validation, so both spans must contain at least <paramref name="count" /> elements.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Or(Span<int> destination, Span<int> source, uint count)
        {
            switch (count)
            {
                case 7U:
                    destination[6] |= source[6];
                    goto case 6U;

                case 6U:
                    destination[5] |= source[5];
                    goto case 5U;

                case 5U:
                    destination[4] |= source[4];
                    goto case 4U;

                case 4U:
                    destination[3] |= source[3];
                    goto case 3U;

                case 3U:
                    destination[2] |= source[2];
                    goto case 2U;

                case 2U:
                    destination[1] |= source[1];
                    goto case 1U;

                case 1U:
                    destination[0] |= source[0];
                    return;

                case 0U:
                    return;
            }

            ref var left = ref MemoryMarshal.GetReference(destination);
            ref var right = ref MemoryMarshal.GetReference(source);
            var i = 0U;
#if NET5_0_OR_GREATER
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated && count >= U32.FromI32Unchecked(Vector512<int>.Count))
            {
                var n = count.WrappingSub(U32.FromI32Unchecked(Vector512<int>.Count).WrappingSub(1U));
                for (; i < n; i = i.WrappingAdd(U32.FromI32Unchecked(Vector512<int>.Count)))
                {
                    var result = Vector512.LoadUnsafe(ref left, Usize.FromU32Unchecked(i)) | Vector512.LoadUnsafe(ref right, Usize.FromU32Unchecked(i));
                    result.StoreUnsafe(ref left, Usize.FromU32Unchecked(i));
                }
            }
            else
#endif
            if (IsHardwareAccelerated256 && count >= U32.FromI32Unchecked(Vector256<int>.Count))
            {
                var n = count.WrappingSub(U32.FromI32Unchecked(Vector256<int>.Count).WrappingSub(1U));
                for (; i < n; i = i.WrappingAdd(U32.FromI32Unchecked(Vector256<int>.Count)))
                {
                    var result = Or(Load256(ref left, Usize.FromU32Unchecked(i)), Load256(ref right, Usize.FromU32Unchecked(i)));
                    Store256(ref left, Usize.FromU32Unchecked(i), result);
                }
            }
            else if (IsHardwareAccelerated128 && count >= U32.FromI32Unchecked(Vector128<int>.Count))
            {
                var n = count.WrappingSub(U32.FromI32Unchecked(Vector128<int>.Count).WrappingSub(1U));
                for (; i < n; i = i.WrappingAdd(U32.FromI32Unchecked(Vector128<int>.Count)))
                {
                    var result = Or(Load128(ref left, Usize.FromU32Unchecked(i)), Load128(ref right, Usize.FromU32Unchecked(i)));
                    Store128(ref left, Usize.FromU32Unchecked(i), result);
                }
            }
#endif
            for (; i < count; i = i.WrappingAdd(1U))
                Unsafe.Add(ref left, Isize.FromU32Unchecked(i)) |= Unsafe.Add(ref right, Isize.FromU32Unchecked(i));
        }

        /// <summary>
        ///     Performs the bitwise XOR operation between the elements of <paramref name="destination" />
        ///     and the corresponding elements of <paramref name="source" />, storing the result in
        ///     <paramref name="destination" />.
        /// </summary>
        /// <param name="destination">The span of 32-bit words that receives the result of the operation.</param>
        /// <param name="source">The span of 32-bit words to combine with <paramref name="destination" />.</param>
        /// <param name="count">The number of 32-bit words to process, starting at index <c>0</c>.</param>
        /// <remarks>
        ///     The span lengths are validated only for counts of seven or fewer, which are accessed
        ///     through the span indexers; larger counts are accessed through references without span
        ///     validation, so both spans must contain at least <paramref name="count" /> elements.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Xor(Span<int> destination, Span<int> source, uint count)
        {
            switch (count)
            {
                case 7U:
                    destination[6] ^= source[6];
                    goto case 6U;

                case 6U:
                    destination[5] ^= source[5];
                    goto case 5U;

                case 5U:
                    destination[4] ^= source[4];
                    goto case 4U;

                case 4U:
                    destination[3] ^= source[3];
                    goto case 3U;

                case 3U:
                    destination[2] ^= source[2];
                    goto case 2U;

                case 2U:
                    destination[1] ^= source[1];
                    goto case 1U;

                case 1U:
                    destination[0] ^= source[0];
                    return;

                case 0U:
                    return;
            }

            ref var left = ref MemoryMarshal.GetReference(destination);
            ref var right = ref MemoryMarshal.GetReference(source);
            var i = 0U;
#if NET5_0_OR_GREATER
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated && count >= U32.FromI32Unchecked(Vector512<int>.Count))
            {
                var n = count.WrappingSub(U32.FromI32Unchecked(Vector512<int>.Count).WrappingSub(1U));
                for (; i < n; i = i.WrappingAdd(U32.FromI32Unchecked(Vector512<int>.Count)))
                {
                    var result = Vector512.LoadUnsafe(ref left, Usize.FromU32Unchecked(i)) ^ Vector512.LoadUnsafe(ref right, Usize.FromU32Unchecked(i));
                    result.StoreUnsafe(ref left, Usize.FromU32Unchecked(i));
                }
            }
            else
#endif
            if (IsHardwareAccelerated256 && count >= U32.FromI32Unchecked(Vector256<int>.Count))
            {
                var n = count.WrappingSub(U32.FromI32Unchecked(Vector256<int>.Count).WrappingSub(1U));
                for (; i < n; i = i.WrappingAdd(U32.FromI32Unchecked(Vector256<int>.Count)))
                {
                    var result = Xor(Load256(ref left, Usize.FromU32Unchecked(i)), Load256(ref right, Usize.FromU32Unchecked(i)));
                    Store256(ref left, Usize.FromU32Unchecked(i), result);
                }
            }
            else if (IsHardwareAccelerated128 && count >= U32.FromI32Unchecked(Vector128<int>.Count))
            {
                var n = count.WrappingSub(U32.FromI32Unchecked(Vector128<int>.Count).WrappingSub(1U));
                for (; i < n; i = i.WrappingAdd(U32.FromI32Unchecked(Vector128<int>.Count)))
                {
                    var result = Xor(Load128(ref left, Usize.FromU32Unchecked(i)), Load128(ref right, Usize.FromU32Unchecked(i)));
                    Store128(ref left, Usize.FromU32Unchecked(i), result);
                }
            }
#endif
            for (; i < count; i = i.WrappingAdd(1U))
                Unsafe.Add(ref left, Isize.FromU32Unchecked(i)) ^= Unsafe.Add(ref right, Isize.FromU32Unchecked(i));
        }

        /// <summary>
        ///     Inverts all the bits in the specified span, so that bits set to
        ///     <see langword="true" /> are changed to <see langword="false" />, and bits
        ///     set to <see langword="false" /> are changed to <see langword="true" />.
        /// </summary>
        /// <param name="destination">The span of 32-bit words to complement in place.</param>
        /// <param name="count">The number of 32-bit words to process, starting at index <c>0</c>.</param>
        /// <remarks>
        ///     The span length is validated only for counts of seven or fewer, which are accessed
        ///     through the span indexer; larger counts are accessed through references without span
        ///     validation, so the span must contain at least <paramref name="count" /> elements.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Not(Span<int> destination, uint count)
        {
            switch (count)
            {
                case 7U:
                    destination[6] = ~destination[6];
                    goto case 6U;

                case 6U:
                    destination[5] = ~destination[5];
                    goto case 5U;

                case 5U:
                    destination[4] = ~destination[4];
                    goto case 4U;

                case 4U:
                    destination[3] = ~destination[3];
                    goto case 3U;

                case 3U:
                    destination[2] = ~destination[2];
                    goto case 2U;

                case 2U:
                    destination[1] = ~destination[1];
                    goto case 1U;

                case 1U:
                    destination[0] = ~destination[0];
                    return;

                case 0U:
                    return;
            }

            ref var location = ref MemoryMarshal.GetReference(destination);
            var i = 0U;
#if NET5_0_OR_GREATER
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated && count >= U32.FromI32Unchecked(Vector512<int>.Count))
            {
                var n = count.WrappingSub(U32.FromI32Unchecked(Vector512<int>.Count).WrappingSub(1U));
                for (; i < n; i = i.WrappingAdd(U32.FromI32Unchecked(Vector512<int>.Count)))
                {
                    var result = ~Vector512.LoadUnsafe(ref location, Usize.FromU32Unchecked(i));
                    result.StoreUnsafe(ref location, Usize.FromU32Unchecked(i));
                }
            }
            else
#endif
            if (IsHardwareAccelerated256 && count >= U32.FromI32Unchecked(Vector256<int>.Count))
            {
                var n = count.WrappingSub(U32.FromI32Unchecked(Vector256<int>.Count).WrappingSub(1U));
                for (; i < n; i = i.WrappingAdd(U32.FromI32Unchecked(Vector256<int>.Count)))
                {
                    var result = Not(Load256(ref location, Usize.FromU32Unchecked(i)));
                    Store256(ref location, Usize.FromU32Unchecked(i), result);
                }
            }
            else if (IsHardwareAccelerated128 && count >= U32.FromI32Unchecked(Vector128<int>.Count))
            {
                var n = count.WrappingSub(U32.FromI32Unchecked(Vector128<int>.Count).WrappingSub(1U));
                for (; i < n; i = i.WrappingAdd(U32.FromI32Unchecked(Vector128<int>.Count)))
                {
                    var result = Not(Load128(ref location, Usize.FromU32Unchecked(i)));
                    Store128(ref location, Usize.FromU32Unchecked(i), result);
                }
            }
#endif
            for (; i < count; i = i.WrappingAdd(1U))
                Unsafe.Add(ref location, Isize.FromU32Unchecked(i)) = ~ Unsafe.Add(ref location, Isize.FromU32Unchecked(i));
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Gets a value that indicates whether 256-bit vector operations are
        ///     subject to hardware acceleration through JIT intrinsic support.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if 256-bit vector operations are subject
        ///     to hardware acceleration; otherwise, <see langword="false" />.
        /// </value>
        private static bool IsHardwareAccelerated256 =>
#if NET7_0_OR_GREATER
            Vector256.IsHardwareAccelerated;
#else
            Avx2.IsSupported;
#endif

        /// <summary>
        ///     Gets a value that indicates whether 128-bit vector operations are
        ///     subject to hardware acceleration through JIT intrinsic support.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if 128-bit vector operations are subject
        ///     to hardware acceleration; otherwise, <see langword="false" />.
        /// </value>
        private static bool IsHardwareAccelerated128 =>
#if NET7_0_OR_GREATER
            Vector128.IsHardwareAccelerated;
#else
            Sse2.IsSupported || AdvSimd.IsSupported;
#endif

        /// <summary>
        ///     Loads a vector from the given source and element offset.
        /// </summary>
        /// <param name="source">
        ///     The source to which <paramref name="elementOffset" /> will be added before loading the vector.
        /// </param>
        /// <param name="elementOffset">
        ///     The element offset from <paramref name="source" /> from which the vector will be loaded.
        /// </param>
        /// <returns>The vector loaded from <paramref name="source" /> plus <paramref name="elementOffset" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<int> Load256(ref int source, nuint elementOffset) => Unsafe.ReadUnaligned<Vector256<int>>(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref source, Isize.FromUsizeUnchecked(elementOffset))));

        /// <summary>
        ///     Loads a vector from the given source and element offset.
        /// </summary>
        /// <param name="source">
        ///     The source to which <paramref name="elementOffset" /> will be added before loading the vector.
        /// </param>
        /// <param name="elementOffset">
        ///     The element offset from <paramref name="source" /> from which the vector will be loaded.
        /// </param>
        /// <returns>The vector loaded from <paramref name="source" /> plus <paramref name="elementOffset" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<int> Load128(ref int source, nuint elementOffset) => Unsafe.ReadUnaligned<Vector128<int>>(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref source, Isize.FromUsizeUnchecked(elementOffset))));

        /// <summary>
        ///     Stores a vector at the given destination.
        /// </summary>
        /// <param name="source">The vector that will be stored.</param>
        /// <param name="destination">
        ///     The destination to which <paramref name="elementOffset" />
        ///     will be added before the vector will be stored.
        /// </param>
        /// <param name="elementOffset">
        ///     The element offset from <paramref name="destination" /> from which the vector will be stored.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Store256(ref int destination, nuint elementOffset, Vector256<int> source) => Unsafe.WriteUnaligned(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref destination, Isize.FromUsizeUnchecked(elementOffset))), source);

        /// <summary>
        ///     Stores a vector at the given destination.
        /// </summary>
        /// <param name="source">The vector that will be stored.</param>
        /// <param name="destination">
        ///     The destination to which <paramref name="elementOffset" />
        ///     will be added before the vector will be stored.
        /// </param>
        /// <param name="elementOffset">
        ///     The element offset from <paramref name="destination" /> from which the vector will be stored.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Store128(ref int destination, nuint elementOffset, Vector128<int> source) => Unsafe.WriteUnaligned(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref destination, Isize.FromUsizeUnchecked(elementOffset))), source);

        /// <summary>
        ///     Computes the bitwise-and of two vectors.
        /// </summary>
        /// <param name="left">The vector to bitwise-and with <paramref name="right" />.</param>
        /// <param name="right">The vector to bitwise-and with <paramref name="left" />.</param>
        /// <returns>The bitwise-and of <paramref name="left" /> and <paramref name="right" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<int> And(Vector256<int> left, Vector256<int> right)
        {
#if NET7_0_OR_GREATER
            return left & right;
#else
            return Avx2.And(left, right);
#endif
        }

        /// <summary>
        ///     Computes the bitwise-or of two vectors.
        /// </summary>
        /// <param name="left">The vector to bitwise-or with <paramref name="right" />.</param>
        /// <param name="right">The vector to bitwise-or with <paramref name="left" />.</param>
        /// <returns>The bitwise-or of <paramref name="left" /> and <paramref name="right" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<int> Or(Vector256<int> left, Vector256<int> right)
        {
#if NET7_0_OR_GREATER
            return left | right;
#else
            return Avx2.Or(left, right);
#endif
        }

        /// <summary>
        ///     Computes the exclusive-or of two vectors.
        /// </summary>
        /// <param name="left">The vector to exclusive-or with <paramref name="right" />.</param>
        /// <param name="right">The vector to exclusive-or with <paramref name="left" />.</param>
        /// <returns>The exclusive-or of <paramref name="left" /> and <paramref name="right" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<int> Xor(Vector256<int> left, Vector256<int> right)
        {
#if NET7_0_OR_GREATER
            return left ^ right;
#else
            return Avx2.Xor(left, right);
#endif
        }

        /// <summary>
        ///     Computes the ones-complement of a vector.
        /// </summary>
        /// <param name="vector">The vector whose ones-complement is to be computed.</param>
        /// <returns>
        ///     A vector whose elements are the ones-complement of the
        ///     corresponding elements in <paramref name="vector" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<int> Not(Vector256<int> vector)
        {
#if NET7_0_OR_GREATER
            return ~vector;
#else
            return Avx2.Xor(vector, Vector256.Create(-1));
#endif
        }

        /// <summary>
        ///     Computes the bitwise-and of two vectors.
        /// </summary>
        /// <param name="left">The vector to bitwise-and with <paramref name="right" />.</param>
        /// <param name="right">The vector to bitwise-and with <paramref name="left" />.</param>
        /// <returns>The bitwise-and of <paramref name="left" /> and <paramref name="right" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<int> And(Vector128<int> left, Vector128<int> right)
        {
#if NET7_0_OR_GREATER
            return left & right;
#else
            return Sse2.IsSupported ? Sse2.And(left, right) : AdvSimd.And(left, right);
#endif
        }

        /// <summary>
        ///     Computes the bitwise-or of two vectors.
        /// </summary>
        /// <param name="left">The vector to bitwise-or with <paramref name="right" />.</param>
        /// <param name="right">The vector to bitwise-or with <paramref name="left" />.</param>
        /// <returns>The bitwise-or of <paramref name="left" /> and <paramref name="right" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<int> Or(Vector128<int> left, Vector128<int> right)
        {
#if NET7_0_OR_GREATER
            return left | right;
#else
            return Sse2.IsSupported ? Sse2.Or(left, right) : AdvSimd.Or(left, right);
#endif
        }

        /// <summary>
        ///     Computes the exclusive-or of two vectors.
        /// </summary>
        /// <param name="left">The vector to exclusive-or with <paramref name="right" />.</param>
        /// <param name="right">The vector to exclusive-or with <paramref name="left" />.</param>
        /// <returns>The exclusive-or of <paramref name="left" /> and <paramref name="right" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<int> Xor(Vector128<int> left, Vector128<int> right)
        {
#if NET7_0_OR_GREATER
            return left ^ right;
#else
            return Sse2.IsSupported ? Sse2.Xor(left, right) : AdvSimd.Xor(left, right);
#endif
        }

        /// <summary>
        ///     Computes the ones-complement of a vector.
        /// </summary>
        /// <param name="vector">The vector whose ones-complement is to be computed.</param>
        /// <returns>
        ///     A vector whose elements are the ones-complement of the
        ///     corresponding elements in <paramref name="vector" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<int> Not(Vector128<int> vector)
        {
#if NET7_0_OR_GREATER
            return ~vector;
#else
            return Sse2.IsSupported ? Sse2.Xor(vector, Vector128.Create(-1)) : AdvSimd.Not(vector);
#endif
        }
#endif
    }
}