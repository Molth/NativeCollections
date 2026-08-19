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
    ///     Manages a compact array of bit values, which are represented as <see cref="bool" />, where
    ///     <see langword="true" /> indicates that the bit is on (1) and <see langword="false" /> indicates
    ///     the bit is off (0).
    /// </summary>
    internal static class BitArrayHelpers
    {
        /// <summary>
        ///     Performs the bitwise AND operation between the elements of the current object and the
        ///     corresponding elements in the specified array. The current object will be modified to
        ///     store the result of the bitwise AND operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void And(Span<int> destination, Span<int> source, uint count)
        {
            switch (count)
            {
                case 7:
                    destination[6] &= source[6];
                    goto case 6;
                case 6:
                    destination[5] &= source[5];
                    goto case 5;
                case 5:
                    destination[4] &= source[4];
                    goto case 4;
                case 4:
                    destination[3] &= source[3];
                    goto case 3;
                case 3:
                    destination[2] &= source[2];
                    goto case 2;
                case 2:
                    destination[1] &= source[1];
                    goto case 1;
                case 1:
                    destination[0] &= source[0];
                    return;
                case 0:
                    return;
            }

            ref var left = ref MemoryMarshal.GetReference(destination);
            ref var right = ref MemoryMarshal.GetReference(source);
            uint i = 0;
#if NET5_0_OR_GREATER
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated && count >= (uint)Vector512<int>.Count)
            {
                var n = count - ((uint)Vector512<int>.Count - 1);
                for (; i < n; i += (uint)Vector512<int>.Count)
                {
                    var result = Vector512.LoadUnsafe(ref left, i) & Vector512.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }
            else
#endif
            if (IsHardwareAccelerated256 && count >= (uint)Vector256<int>.Count)
            {
                var n = count - ((uint)Vector256<int>.Count - 1);
                for (; i < n; i += (uint)Vector256<int>.Count)
                {
                    var result = And(Load256(ref left, i), Load256(ref right, i));
                    Store256(ref left, i, result);
                }
            }
            else if (IsHardwareAccelerated128 && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var result = And(Load128(ref left, i), Load128(ref right, i));
                    Store128(ref left, i, result);
                }
            }
#endif
            for (; i < count; ++i)
                Unsafe.Add(ref left, (nint)i) &= Unsafe.Add(ref right, (nint)i);
        }

        /// <summary>
        ///     Performs the bitwise OR operation between the elements of the current object and the
        ///     corresponding elements in the specified array. The current object will be modified to
        ///     store the result of the bitwise OR operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Or(Span<int> destination, Span<int> source, uint count)
        {
            switch (count)
            {
                case 7:
                    destination[6] |= source[6];
                    goto case 6;
                case 6:
                    destination[5] |= source[5];
                    goto case 5;
                case 5:
                    destination[4] |= source[4];
                    goto case 4;
                case 4:
                    destination[3] |= source[3];
                    goto case 3;
                case 3:
                    destination[2] |= source[2];
                    goto case 2;
                case 2:
                    destination[1] |= source[1];
                    goto case 1;
                case 1:
                    destination[0] |= source[0];
                    return;
                case 0:
                    return;
            }

            ref var left = ref MemoryMarshal.GetReference(destination);
            ref var right = ref MemoryMarshal.GetReference(source);
            uint i = 0;
#if NET5_0_OR_GREATER
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated && count >= (uint)Vector512<int>.Count)
            {
                var n = count - ((uint)Vector512<int>.Count - 1);
                for (; i < n; i += (uint)Vector512<int>.Count)
                {
                    var result = Vector512.LoadUnsafe(ref left, i) | Vector512.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }
            else
#endif
            if (IsHardwareAccelerated256 && count >= (uint)Vector256<int>.Count)
            {
                var n = count - ((uint)Vector256<int>.Count - 1);
                for (; i < n; i += (uint)Vector256<int>.Count)
                {
                    var result = Or(Load256(ref left, i), Load256(ref right, i));
                    Store256(ref left, i, result);
                }
            }
            else if (IsHardwareAccelerated128 && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var result = Or(Load128(ref left, i), Load128(ref right, i));
                    Store128(ref left, i, result);
                }
            }
#endif
            for (; i < count; ++i)
                Unsafe.Add(ref left, (nint)i) |= Unsafe.Add(ref right, (nint)i);
        }

        /// <summary>
        ///     Performs the bitwise XOR operation between the elements of the current object and the
        ///     corresponding elements in the specified array. The current object will be modified to
        ///     store the result of the bitwise XOR operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Xor(Span<int> destination, Span<int> source, uint count)
        {
            switch (count)
            {
                case 7:
                    destination[6] ^= source[6];
                    goto case 6;
                case 6:
                    destination[5] ^= source[5];
                    goto case 5;
                case 5:
                    destination[4] ^= source[4];
                    goto case 4;
                case 4:
                    destination[3] ^= source[3];
                    goto case 3;
                case 3:
                    destination[2] ^= source[2];
                    goto case 2;
                case 2:
                    destination[1] ^= source[1];
                    goto case 1;
                case 1:
                    destination[0] ^= source[0];
                    return;
                case 0:
                    return;
            }

            ref var left = ref MemoryMarshal.GetReference(destination);
            ref var right = ref MemoryMarshal.GetReference(source);
            uint i = 0;
#if NET5_0_OR_GREATER
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated && count >= (uint)Vector512<int>.Count)
            {
                var n = count - ((uint)Vector512<int>.Count - 1);
                for (; i < n; i += (uint)Vector512<int>.Count)
                {
                    var result = Vector512.LoadUnsafe(ref left, i) ^ Vector512.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }
            else
#endif
            if (IsHardwareAccelerated256 && count >= (uint)Vector256<int>.Count)
            {
                var n = count - ((uint)Vector256<int>.Count - 1);
                for (; i < n; i += (uint)Vector256<int>.Count)
                {
                    var result = Xor(Load256(ref left, i), Load256(ref right, i));
                    Store256(ref left, i, result);
                }
            }
            else if (IsHardwareAccelerated128 && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var result = Xor(Load128(ref left, i), Load128(ref right, i));
                    Store128(ref left, i, result);
                }
            }
#endif
            for (; i < count; ++i)
                Unsafe.Add(ref left, (nint)i) ^= Unsafe.Add(ref right, (nint)i);
        }

        /// <summary>
        ///     Inverts all the bit values in the current, so that elements set to true are changed to false,
        ///     and elements set to false are changed to true.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Not(Span<int> destination, uint count)
        {
            switch (count)
            {
                case 7:
                    destination[6] = ~destination[6];
                    goto case 6;
                case 6:
                    destination[5] = ~destination[5];
                    goto case 5;
                case 5:
                    destination[4] = ~destination[4];
                    goto case 4;
                case 4:
                    destination[3] = ~destination[3];
                    goto case 3;
                case 3:
                    destination[2] = ~destination[2];
                    goto case 2;
                case 2:
                    destination[1] = ~destination[1];
                    goto case 1;
                case 1:
                    destination[0] = ~destination[0];
                    return;
                case 0:
                    return;
            }

            ref var location = ref MemoryMarshal.GetReference(destination);
            uint i = 0;
#if NET5_0_OR_GREATER
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated && count >= (uint)Vector512<int>.Count)
            {
                var n = count - ((uint)Vector512<int>.Count - 1);
                for (; i < n; i += (uint)Vector512<int>.Count)
                {
                    var result = ~Vector512.LoadUnsafe(ref location, i);
                    result.StoreUnsafe(ref location, i);
                }
            }
            else
#endif
            if (IsHardwareAccelerated256 && count >= (uint)Vector256<int>.Count)
            {
                var n = count - ((uint)Vector256<int>.Count - 1);
                for (; i < n; i += (uint)Vector256<int>.Count)
                {
                    var result = Not(Load256(ref location, i));
                    Store256(ref location, i, result);
                }
            }
            else if (IsHardwareAccelerated128 && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var result = Not(Load128(ref location, i));
                    Store128(ref location, i, result);
                }
            }
#endif
            for (; i < count; ++i)
                Unsafe.Add(ref location, (nint)i) = ~ Unsafe.Add(ref location, (nint)i);
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Gets a value that indicates whether 256-bit vector operations
        ///     are subject to hardware acceleration through JIT intrinsic support.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if 256-bit vector operations are subject to hardware acceleration;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsHardwareAccelerated256 =>
#if NET7_0_OR_GREATER
            Vector256.IsHardwareAccelerated;
#else
            Avx2.IsSupported;
#endif

        /// <summary>
        ///     Gets a value that indicates whether 128-bit vector operations
        ///     are subject to hardware acceleration through JIT intrinsic support.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if 128-bit vector operations are subject to hardware acceleration;
        ///     otherwise, <see langword="false" />.
        /// </returns>
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
        ///     The source to which <paramref name="elementOffset" />
        ///     will be added before loading the vector.
        /// </param>
        /// <param name="elementOffset">
        ///     The element offset from <paramref name="source" />
        ///     from which the vector will be loaded.
        /// </param>
        /// <returns>The vector loaded from <paramref name="source" /> plus <paramref name="elementOffset" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector256<int> Load256(ref int source, nuint elementOffset) => Unsafe.ReadUnaligned<Vector256<int>>(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref source, (nint)elementOffset)));

        /// <summary>
        ///     Loads a vector from the given source and element offset.
        /// </summary>
        /// <param name="source">
        ///     The source to which <paramref name="elementOffset" />
        ///     will be added before loading the vector.
        /// </param>
        /// <param name="elementOffset">
        ///     The element offset from <paramref name="source" />
        ///     from which the vector will be loaded.
        /// </param>
        /// <returns>The vector loaded from <paramref name="source" /> plus <paramref name="elementOffset" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<int> Load128(ref int source, nuint elementOffset) => Unsafe.ReadUnaligned<Vector128<int>>(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref source, (nint)elementOffset)));

        /// <summary>
        ///     Stores a vector at the given destination.
        /// </summary>
        /// <param name="source">The vector that will be stored.</param>
        /// <param name="destination">
        ///     The destination to which <paramref name="elementOffset" />
        ///     will be added before the vector will be stored.
        /// </param>
        /// <param name="elementOffset">
        ///     The element offset from <paramref name="destination" />
        ///     from which the vector will be stored.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Store256(ref int destination, nuint elementOffset, Vector256<int> source) => Unsafe.WriteUnaligned(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref destination, (nint)elementOffset)), source);

        /// <summary>
        ///     Stores a vector at the given destination.
        /// </summary>
        /// <param name="source">The vector that will be stored.</param>
        /// <param name="destination">
        ///     The destination to which <paramref name="elementOffset" />
        ///     will be added before the vector will be stored.
        /// </param>
        /// <param name="elementOffset">
        ///     The element offset from <paramref name="destination" />
        ///     from which the vector will be stored.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Store128(ref int destination, nuint elementOffset, Vector128<int> source) => Unsafe.WriteUnaligned(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref destination, (nint)elementOffset)), source);

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
        /// <returns>A vector whose elements are the ones-complement of the corresponding elements in <paramref name="vector" />.</returns>
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
        /// <returns>A vector whose elements are the ones-complement of the corresponding elements in <paramref name="vector" />.</returns>
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