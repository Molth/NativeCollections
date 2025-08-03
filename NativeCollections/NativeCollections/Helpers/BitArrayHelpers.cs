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
    ///     BitArray helpers
    /// </summary>
    internal static
#if NET5_0_OR_GREATER && !NET7_0_OR_GREATER
        unsafe
#endif
        class BitArrayHelpers
    {
        /// <summary>
        ///     And
        /// </summary>
        /// <param name="destination">Destination</param>
        /// <param name="source">Source</param>
        /// <param name="count">Count</param>
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
#if NET7_0_OR_GREATER
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
            if (Vector256.IsHardwareAccelerated && count >= (uint)Vector256<int>.Count)
            {
                var n = count - ((uint)Vector256<int>.Count - 1);
                for (; i < n; i += (uint)Vector256<int>.Count)
                {
                    var result = Vector256.LoadUnsafe(ref left, i) & Vector256.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }
            else if (Vector128.IsHardwareAccelerated && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var result = Vector128.LoadUnsafe(ref left, i) & Vector128.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }
#elif NET5_0_OR_GREATER
            if (Avx2.IsSupported && count >= (uint)Vector256<int>.Count)
            {
                var n = count - ((uint)Vector256<int>.Count - 1);
                for (; i < n; i += (uint)Vector256<int>.Count)
                {
                    var local1 = Avx.LoadVector256((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)));
                    var local2 = Avx.LoadVector256((int*)Unsafe.AsPointer(ref Unsafe.Add(ref right, (nint)i)));
                    Avx.Store((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)), Avx2.And(local1, local2));
                }
            }
            else if (Sse2.IsSupported && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var local1 = Sse2.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)));
                    var local2 = Sse2.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref right, (nint)i)));
                    Sse2.Store((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)), Sse2.And(local1, local2));
                }
            }
            else if (AdvSimd.IsSupported && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var local1 = AdvSimd.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)));
                    var local2 = AdvSimd.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref right, (nint)i)));
                    AdvSimd.Store((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)), AdvSimd.And(local1, local2));
                }
            }
#endif
            for (; i < count; ++i)
                Unsafe.Add(ref left, (nint)i) &= Unsafe.Add(ref right, (nint)i);
        }

        /// <summary>
        ///     Or
        /// </summary>
        /// <param name="destination">Destination</param>
        /// <param name="source">Source</param>
        /// <param name="count">Count</param>
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
#if NET7_0_OR_GREATER
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
            if (Vector256.IsHardwareAccelerated && count >= (uint)Vector256<int>.Count)
            {
                var n = count - ((uint)Vector256<int>.Count - 1);
                for (; i < n; i += (uint)Vector256<int>.Count)
                {
                    var result = Vector256.LoadUnsafe(ref left, i) | Vector256.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }
            else if (Vector128.IsHardwareAccelerated && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var result = Vector128.LoadUnsafe(ref left, i) | Vector128.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }
#elif NET5_0_OR_GREATER
            if (Avx2.IsSupported && count >= (uint)Vector256<int>.Count)
            {
                var n = count - ((uint)Vector256<int>.Count - 1);
                for (; i < n; i += (uint)Vector256<int>.Count)
                {
                    var local1 = Avx.LoadVector256((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)));
                    var local2 = Avx.LoadVector256((int*)Unsafe.AsPointer(ref Unsafe.Add(ref right, (nint)i)));
                    Avx.Store((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)), Avx2.Or(local1, local2));
                }
            }
            else if (Sse2.IsSupported && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var local1 = Sse2.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)));
                    var local2 = Sse2.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref right, (nint)i)));
                    Sse2.Store((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)), Sse2.Or(local1, local2));
                }
            }
            else if (AdvSimd.IsSupported && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var local1 = AdvSimd.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)));
                    var local2 = AdvSimd.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref right, (nint)i)));
                    AdvSimd.Store((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)), AdvSimd.Or(local1, local2));
                }
            }
#endif
            for (; i < count; ++i)
                Unsafe.Add(ref left, (nint)i) |= Unsafe.Add(ref right, (nint)i);
        }

        /// <summary>
        ///     Xor
        /// </summary>
        /// <param name="destination">Destination</param>
        /// <param name="source">Source</param>
        /// <param name="count">Count</param>
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
#if NET7_0_OR_GREATER
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
            if (Vector256.IsHardwareAccelerated && count >= (uint)Vector256<int>.Count)
            {
                var n = count - ((uint)Vector256<int>.Count - 1);
                for (; i < n; i += (uint)Vector256<int>.Count)
                {
                    var result = Vector256.LoadUnsafe(ref left, i) ^ Vector256.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }
            else if (Vector128.IsHardwareAccelerated && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var result = Vector128.LoadUnsafe(ref left, i) ^ Vector128.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }
#elif NET5_0_OR_GREATER
            if (Avx2.IsSupported && count >= (uint)Vector256<int>.Count)
            {
                var n = count - ((uint)Vector256<int>.Count - 1);
                for (; i < n; i += (uint)Vector256<int>.Count)
                {
                    var local1 = Avx.LoadVector256((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)));
                    var local2 = Avx.LoadVector256((int*)Unsafe.AsPointer(ref Unsafe.Add(ref right, (nint)i)));
                    Avx.Store((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)), Avx2.Xor(local1, local2));
                }
            }
            else if (Sse2.IsSupported && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var local1 = Sse2.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)));
                    var local2 = Sse2.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref right, (nint)i)));
                    Sse2.Store((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)), Sse2.Xor(local1, local2));
                }
            }
            else if (AdvSimd.IsSupported && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var local1 = AdvSimd.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)));
                    var local2 = AdvSimd.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref right, (nint)i)));
                    AdvSimd.Store((int*)Unsafe.AsPointer(ref Unsafe.Add(ref left, (nint)i)), AdvSimd.Xor(local1, local2));
                }
            }
#endif
            for (; i < count; ++i)
                Unsafe.Add(ref left, (nint)i) ^= Unsafe.Add(ref right, (nint)i);
        }

        /// <summary>
        ///     Not
        /// </summary>
        /// <param name="destination">Destination</param>
        /// <param name="count">Count</param>
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

            ref var value = ref MemoryMarshal.GetReference(destination);
            uint i = 0;
#if NET7_0_OR_GREATER
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated && count >= (uint)Vector512<int>.Count)
            {
                var n = count - ((uint)Vector512<int>.Count - 1);
                for (; i < n; i += (uint)Vector512<int>.Count)
                {
                    var result = ~Vector512.LoadUnsafe(ref value, i);
                    result.StoreUnsafe(ref value, i);
                }
            }
            else
#endif
            if (Vector256.IsHardwareAccelerated && count >= (uint)Vector256<int>.Count)
            {
                var n = count - ((uint)Vector256<int>.Count - 1);
                for (; i < n; i += (uint)Vector256<int>.Count)
                {
                    var result = ~Vector256.LoadUnsafe(ref value, i);
                    result.StoreUnsafe(ref value, i);
                }
            }
            else if (Vector128.IsHardwareAccelerated && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var result = ~Vector128.LoadUnsafe(ref value, i);
                    result.StoreUnsafe(ref value, i);
                }
            }
#elif NET5_0_OR_GREATER
            if (Avx2.IsSupported && count >= (uint)Vector256<int>.Count)
            {
                var local2 = Vector256.Create(-1);
                var n = count - ((uint)Vector256<int>.Count - 1);
                for (; i < n; i += (uint)Vector256<int>.Count)
                {
                    var local1 = Avx.LoadVector256((int*)Unsafe.AsPointer(ref Unsafe.Add(ref value, (nint)i)));
                    Avx.Store((int*)Unsafe.AsPointer(ref Unsafe.Add(ref value, (nint)i)), Avx2.Xor(local1, local2));
                }
            }
            else if (Sse2.IsSupported && count >= (uint)Vector128<int>.Count)
            {
                var local2 = Vector128.Create(-1);
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var local1 = Sse2.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref value, (nint)i)));
                    Sse2.Store((int*)Unsafe.AsPointer(ref Unsafe.Add(ref value, (nint)i)), Sse2.Xor(local1, local2));
                }
            }
            else if (AdvSimd.IsSupported && count >= (uint)Vector128<int>.Count)
            {
                var n = count - ((uint)Vector128<int>.Count - 1);
                for (; i < n; i += (uint)Vector128<int>.Count)
                {
                    var local = AdvSimd.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref value, (nint)i)));
                    AdvSimd.Store((int*)Unsafe.AsPointer(ref Unsafe.Add(ref value, (nint)i)), AdvSimd.Not(local));
                }
            }
#endif
            for (; i < count; ++i)
                Unsafe.Add(ref value, (nint)i) = ~ Unsafe.Add(ref value, (nint)i);
        }
    }
}