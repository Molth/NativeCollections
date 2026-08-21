using System;
using System.Runtime.CompilerServices;
using NativeCollections;
using rust;

// ReSharper disable All

namespace xxhash_rust
{
    internal static partial class XxHash32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint read_u32(ReadOnlySpan<byte> input, int cursor) => BinaryPrimitivesHelpers.ReadUInt32LittleEndian(input.Slice(cursor));

        private static uint finalize(uint input, ReadOnlySpan<byte> data, int cursor)
        {
            var len = data.Length - cursor;

            while (len >= 4)
            {
                input = input.wrapping_add(
                    read_u32(data, cursor).wrapping_mul(PRIME_3)
                );
                cursor += sizeof(uint);
                len -= sizeof(uint);
                input = input.rotate_left(17).wrapping_mul(PRIME_4);
            }

            while (len > 0)
            {
                input = input.wrapping_add(((uint)data[cursor]).wrapping_mul(PRIME_5));
                cursor += sizeof(byte);
                len -= sizeof(byte);
                input = input.rotate_left(11).wrapping_mul(PRIME_1);
            }

            return avalanche(input);
        }

        /// Const variant of xxh32 hashing.
        public static uint xxh32(ReadOnlySpan<byte> input, uint seed)
        {
            var result = (uint)input.Length;
            var cursor = 0;

            if (input.Length >= CHUNK_SIZE)
            {
                var v1 = seed.wrapping_add(PRIME_1).wrapping_add(PRIME_2);
                var v2 = seed.wrapping_add(PRIME_2);
                var v3 = seed;
                var v4 = seed.wrapping_sub(PRIME_1);

                while (true)
                {
                    v1 = round(v1, read_u32(input, cursor));
                    cursor += sizeof(uint);
                    v2 = round(v2, read_u32(input, cursor));
                    cursor += sizeof(uint);
                    v3 = round(v3, read_u32(input, cursor));
                    cursor += sizeof(uint);
                    v4 = round(v4, read_u32(input, cursor));
                    cursor += sizeof(uint);

                    if (input.Length - cursor < CHUNK_SIZE)
                    {
                        break;
                    }
                }

                result = result.wrapping_add(
                    v1.rotate_left(1).wrapping_add(
                        v2.rotate_left(7).wrapping_add(
                            v3.rotate_left(12).wrapping_add(
                                v4.rotate_left(18)
                            )
                        )
                    )
                );
            }
            else
            {
                result = result.wrapping_add(seed.wrapping_add(PRIME_5));
            }

            return finalize(result, input, cursor);
        }
    }
}