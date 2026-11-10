using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    internal static partial class XxHash32
    {
        /// <summary>
        ///     Reads a little-endian 32-bit unsigned integer from the specified position.
        /// </summary>
        /// <param name="input">The input bytes to read.</param>
        /// <param name="cursor">The zero-based position at which to read.</param>
        /// <returns>The value read from the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ReadU32(ReadOnlySpan<byte> input, int cursor) => BinaryPrimitivesHelpers.ReadUInt32LittleEndian(input.Slice(cursor));

        /// <summary>
        ///     Processes the remaining bytes and applies the final xxHash32 avalanche mix.
        /// </summary>
        /// <param name="input">The current hash accumulator.</param>
        /// <param name="data">The input bytes being hashed.</param>
        /// <param name="cursor">The zero-based position of the first remaining byte.</param>
        /// <returns>The finalized hash value.</returns>
        private static uint Finalize(uint input, ReadOnlySpan<byte> data, int cursor)
        {
            var len = data.Length.WrappingSub(cursor);

            while (len >= 4)
            {
                input = input.WrappingAdd(ReadU32(data, cursor).WrappingMul(PRIME_3));
                cursor = cursor.WrappingAdd(Unsafe.SizeOf<uint>());
                len = len.WrappingSub(Unsafe.SizeOf<uint>());
                input = input.RotateLeft(17).WrappingMul(PRIME_4);
            }

            while (len > 0)
            {
                input = input.WrappingAdd(U32.FromU8Unchecked(data[cursor]).WrappingMul(PRIME_5));
                cursor = cursor.WrappingAdd(Unsafe.SizeOf<byte>());
                len = len.WrappingSub(Unsafe.SizeOf<byte>());
                input = input.RotateLeft(11).WrappingMul(PRIME_1);
            }

            return Avalanche(input);
        }

        /// <summary>
        ///     Computes the xxHash32 value for the specified input and seed.
        /// </summary>
        /// <param name="input">The input bytes to hash.</param>
        /// <param name="seed">The seed used to initialize the hash.</param>
        /// <returns>The computed 32-bit hash value.</returns>
        public static uint HashToU32(ReadOnlySpan<byte> input, uint seed)
        {
            var result = U32.FromI32Unchecked(input.Length);
            var cursor = 0;

            if (input.Length >= CHUNK_SIZE)
            {
                var v1 = seed.WrappingAdd(PRIME_1).WrappingAdd(PRIME_2);
                var v2 = seed.WrappingAdd(PRIME_2);
                var v3 = seed;
                var v4 = seed.WrappingSub(PRIME_1);

                while (true)
                {
                    v1 = Round(v1, ReadU32(input, cursor));
                    cursor = cursor.WrappingAdd(Unsafe.SizeOf<uint>());
                    v2 = Round(v2, ReadU32(input, cursor));
                    cursor = cursor.WrappingAdd(Unsafe.SizeOf<uint>());
                    v3 = Round(v3, ReadU32(input, cursor));
                    cursor = cursor.WrappingAdd(Unsafe.SizeOf<uint>());
                    v4 = Round(v4, ReadU32(input, cursor));
                    cursor = cursor.WrappingAdd(Unsafe.SizeOf<uint>());

                    if (input.Length.WrappingSub(cursor) < CHUNK_SIZE)
                        break;
                }

                result = result.WrappingAdd(v1.RotateLeft(1).WrappingAdd(v2.RotateLeft(7).WrappingAdd(v3.RotateLeft(12).WrappingAdd(v4.RotateLeft(18)))));
            }
            else
            {
                result = result.WrappingAdd(seed.WrappingAdd(PRIME_5));
            }

            return Finalize(result, input, cursor);
        }
    }
}