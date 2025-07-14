using System;
using System.Runtime.CompilerServices;
using System.Text;

#pragma warning disable CA2208
#pragma warning disable CS8618
#pragma warning disable CS8632
#pragma warning disable CS8500

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Encoding helpers
    /// </summary>
    internal static class EncodingHelpers
    {
        /// <summary>
        ///     Encodes into a span of bytes a set of characters from the specified read-only span if the destination is large
        ///     enough.
        /// </summary>
        /// <param name="encoding">Encoding</param>
        /// <param name="chars">The span containing the set of characters to encode.</param>
        /// <param name="bytes">The byte span to hold the encoded bytes.</param>
        /// <param name="bytesWritten">
        ///     Upon successful completion of the operation, the number of bytes encoded into
        ///     <paramref name="bytes" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if all of the characters were encoded into the destination; <see langword="false" />
        ///     if the destination was too small to contain all the encoded bytes.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetBytes(Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes, out int bytesWritten)
        {
#if NET8_0_OR_GREATER
            return encoding.TryGetBytes(chars, bytes, out bytesWritten);
#else
            var required = encoding.GetByteCount(chars);
            if (required <= bytes.Length)
            {
                bytesWritten = encoding.GetBytes(chars, bytes);
                return true;
            }

            bytesWritten = 0;
            return false;
#endif
        }

        /// <summary>
        ///     Decodes into a span of chars a set of bytes from the specified read-only span if the destination is large
        ///     enough.
        /// </summary>
        /// <param name="encoding">Encoding</param>
        /// <param name="bytes">A read-only span containing the sequence of bytes to decode.</param>
        /// <param name="chars">The character span receiving the decoded bytes.</param>
        /// <param name="charsWritten">
        ///     Upon successful completion of the operation, the number of chars decoded into
        ///     <paramref name="chars" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if all of the characters were decoded into the destination; <see langword="false" /> if the
        ///     destination was too small to contain all the decoded chars.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetChars(Encoding encoding, ReadOnlySpan<byte> bytes, Span<char> chars, out int charsWritten)
        {
#if NET8_0_OR_GREATER
            return encoding.TryGetChars(bytes, chars, out charsWritten);
#else
            if (encoding.GetCharCount(bytes) <= chars.Length)
            {
                charsWritten = encoding.GetChars(bytes, chars);
                return true;
            }

            charsWritten = 0;
            return false;
#endif
        }
    }
}