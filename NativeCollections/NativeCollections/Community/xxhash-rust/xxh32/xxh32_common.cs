using System.Runtime.CompilerServices;
using rust;

// ReSharper disable All

namespace xxhash_rust
{
    internal static partial class XxHash32
    {
        private const int CHUNK_SIZE = sizeof(uint) * 4;
        private const uint PRIME_1 = 0x9E3779B1;
        private const uint PRIME_2 = 0x85EBCA77;
        private const uint PRIME_3 = 0xC2B2AE3D;
        private const uint PRIME_4 = 0x27D4EB2F;
        private const uint PRIME_5 = 0x165667B1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint round(uint acc, uint input) =>
            acc.wrapping_add(input.wrapping_mul(PRIME_2))
                .rotate_left(13)
                .wrapping_mul(PRIME_1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint avalanche(uint input)
        {
            input ^= input >> 15;
            input = input.wrapping_mul(PRIME_2);
            input ^= input >> 13;
            input = input.wrapping_mul(PRIME_3);
            input ^= input >> 16;
            return input;
        }
    }
}