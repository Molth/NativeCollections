using System.Runtime.CompilerServices;
using rust;

// ReSharper disable All

namespace xxhash_rust
{
    internal static partial class XxHash64
    {
        private const int CHUNK_SIZE = sizeof(ulong) * 4;
        private const ulong PRIME_1 = 0x9E3779B185EBCA87;
        private const ulong PRIME_2 = 0xC2B2AE3D27D4EB4F;
        private const ulong PRIME_3 = 0x165667B19E3779F9;
        private const ulong PRIME_4 = 0x85EBCA77C2B2AE63;
        private const ulong PRIME_5 = 0x27D4EB2F165667C5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong round(ulong acc, ulong input) =>
            acc.wrapping_add(input.wrapping_mul(PRIME_2))
                .rotate_left(31)
                .wrapping_mul(PRIME_1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong merge_round(ulong acc, ulong val)
        {
            acc ^= round(0, val);
            return acc.wrapping_mul(PRIME_1).wrapping_add(PRIME_4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong avalanche(ulong input)
        {
            input ^= input >> 33;
            input = input.wrapping_mul(PRIME_2);
            input ^= input >> 29;
            input = input.wrapping_mul(PRIME_3);
            input ^= input >> 32;
            return input;
        }
    }
}