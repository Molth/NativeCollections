using System.Runtime.CompilerServices;

namespace Examples
{
    internal static class HashHelpers
    {
        /// <summary>
        ///     Returns approximate reciprocal of the divisor: ceil(2**64 / divisor).
        /// </summary>
        /// <remarks>This should only be used on 64-bit.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetFastModMultiplier(uint divisor) => ulong.MaxValue / divisor + 1;

        /// <summary>
        ///     Performs a mod operation using the multiplier pre-computed with <see cref="GetFastModMultiplier" />.
        /// </summary>
        /// <remarks>This should only be used on 64-bit.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FastMod(uint value, uint divisor, ulong multiplier) => (uint)(((((multiplier * value) >> 32) + 1) * divisor) >> 32);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Mod3(uint value)
        {
            var quotient = (value * 0xAAAAAAABul) >> 33;
            return value - (uint)quotient * 3;
        }
    }
}