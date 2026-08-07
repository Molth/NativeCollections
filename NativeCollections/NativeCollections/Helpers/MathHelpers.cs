using System.Runtime.CompilerServices;
#if NET5_0_OR_GREATER
using System;
#endif

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides constants and static methods for trigonometric, logarithmic,
    ///     and other common mathematical functions.
    /// </summary>
    internal static class MathHelpers
    {
        /// <summary>
        ///     Returns the larger of two native signed integers.
        /// </summary>
        /// <param name="val1">The first of two native signed integers to compare.</param>
        /// <param name="val2">The second of two native signed integers to compare.</param>
        /// <returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Max(nint val1, nint val2)
        {
#if NET6_0_OR_GREATER
            return Math.Max(val1, val2);
#else
            return val1 >= val2 ? val1 : val2;
#endif
        }

        /// <summary>
        ///     Returns the smaller of two native signed integers.
        /// </summary>
        /// <param name="val1">The first of two native signed integers to compare.</param>
        /// <param name="val2">The second of two native signed integers to compare.</param>
        /// <returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Min(nint val1, nint val2)
        {
#if NET6_0_OR_GREATER
            return Math.Min(val1, val2);
#else
            return val1 <= val2 ? val1 : val2;
#endif
        }

        /// <summary>
        ///     Returns the larger of two native unsigned integers.
        /// </summary>
        /// <param name="val1">The first of two native unsigned integers to compare.</param>
        /// <param name="val2">The second of two native unsigned integers to compare.</param>
        /// <returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Max(nuint val1, nuint val2)
        {
#if NET6_0_OR_GREATER
            return Math.Max(val1, val2);
#else
            return val1 >= val2 ? val1 : val2;
#endif
        }

        /// <summary>
        ///     Returns the smaller of two native unsigned integers.
        /// </summary>
        /// <param name="val1">The first of two native unsigned integers to compare.</param>
        /// <param name="val2">The second of two native unsigned integers to compare.</param>
        /// <returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Min(nuint val1, nuint val2)
        {
#if NET6_0_OR_GREATER
            return Math.Min(val1, val2);
#else
            return val1 <= val2 ? val1 : val2;
#endif
        }

        /// <summary>
        ///     Produces the full product of two unsigned 64-bit numbers.
        /// </summary>
        /// <param name="a">The first number to multiply.</param>
        /// <param name="b">The second number to multiply.</param>
        /// <param name="low">The low 64-bit of the product of the specified numbers.</param>
        /// <returns>The high 64-bit of the product of the specified numbers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong BigMul(ulong a, ulong b, out ulong low)
        {
#if NET5_0_OR_GREATER
            return Math.BigMul(a, b, out low);
#else
            var al = (uint)a;
            var ah = (uint)(a >> 32);
            var bl = (uint)b;
            var bh = (uint)(b >> 32);
            var mull = (ulong)al * bl;
            var t = (ulong)ah * bl + (mull >> 32);
            var tl = (ulong)al * bh + (uint)t;
            low = (tl << 32) | (uint)mull;
            return (ulong)ah * bh + (t >> 32) + (tl >> 32);
#endif
        }

        /// <summary>
        ///     Produces the quotient and the remainder of two signed native-size numbers.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The quotient and the remainder of the specified numbers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (nint Quotient, nint Remainder) DivRem(nint left, nint right)
        {
#if NET6_0_OR_GREATER
            return Math.DivRem(left, right);
#else
            var quotient = left / right;
            return (quotient, left - quotient * right);
#endif
        }

        /// <summary>
        ///     Produces the quotient and the remainder of two signed native-size numbers.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The quotient and the remainder of the specified numbers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int Quotient, int Remainder) DivRem(int left, int right)
        {
#if NET6_0_OR_GREATER
            return Math.DivRem(left, right);
#else
            var quotient = left / right;
            return (quotient, left - quotient * right);
#endif
        }
    }
}