using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Reads bytes as primitives with specific endianness.
    /// </summary>
    internal static class BinaryPrimitivesHelpers
    {
        /// <summary>
        ///     Reads a <see cref="UInt32" /> from the given location, as little endian.
        /// </summary>
        /// <param name="source">
        ///     The read-only span to read. Must contain at least 4 bytes;
        ///     this method does not validate the length.
        /// </param>
        /// <returns>The little endian value.</returns>
        /// <remarks>
        ///     A span shorter than 4 bytes, including an empty span, causes an out-of-bounds read.
        ///     Callers must validate the length before calling this method.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> source)
        {
            var result = Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(source));
            if (!BitConverter.IsLittleEndian)
                result = BinaryPrimitives.ReverseEndianness(result);

            return result;
        }

        /// <summary>
        ///     Reads a <see cref="UInt64" /> from the given location, as little endian.
        /// </summary>
        /// <param name="source">
        ///     The read-only span to read. Must contain at least 8 bytes;
        ///     this method does not validate the length.
        /// </param>
        /// <returns>The little endian value.</returns>
        /// <remarks>
        ///     A span shorter than 8 bytes, including an empty span, causes an out-of-bounds read.
        ///     Callers must validate the length before calling this method.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64LittleEndian(ReadOnlySpan<byte> source)
        {
            var result = Unsafe.ReadUnaligned<ulong>(ref MemoryMarshal.GetReference(source));
            if (!BitConverter.IsLittleEndian)
                result = BinaryPrimitives.ReverseEndianness(result);

            return result;
        }
    }
}