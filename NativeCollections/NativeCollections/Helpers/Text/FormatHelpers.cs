using System;
using System.Globalization;
using System.Runtime.CompilerServices;
#if NET8_0_OR_GREATER
using System.Text;
#endif

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a mechanism for retrieving an object to control formatting.
    /// </summary>
    internal static class FormatHelpers
    {
        /// <summary>
        ///     Gets whether the provider provides a custom formatter.
        /// </summary>
        /// <param name="provider">The format provider to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="provider" /> supplies
        ///     a custom formatter; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasCustomFormatter(IFormatProvider provider) => provider.GetType() != typeof(CultureInfo) && provider.GetFormat(typeof(ICustomFormatter)) != null;

        /// <summary>
        ///     Formats the specified value into the destination span. If the destination
        ///     is too small to contain the formatted text, no text is written to it. The
        ///     value is formatted using the specified format string and format provider.
        /// </summary>
        /// <typeparam name="T">The type of the value to format.</typeparam>
        /// <param name="value">The value to format.</param>
        /// <param name="destination">The buffer to receive the formatted characters.</param>
        /// <param name="charsWritten">
        ///     When this method returns, the number of characters written to <paramref name="destination" />.
        /// </param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was formatted into the destination;
        ///     otherwise, <see langword="false" /> if the destination was too small.
        /// </returns>
        /// <remarks>
        ///     Character-sequence values (<see cref="string" />, <see cref="ArraySegment{T}" /> of
        ///     <see cref="char" />, <see cref="ReadOnlyMemory{T}" /> and <see cref="Memory{T}" /> of
        ///     <see cref="char" />, and their nullable forms) are written as their content;
        ///     <paramref name="format" /> is ignored for them.
        /// </remarks>
        public static bool TryFormat<T>(T? value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (typeof(T) == typeof(string))
            {
                var obj = Unsafe.As<T?, string?>(ref value);
                if (obj == null)
                {
                    charsWritten = 0;
                    return true;
                }

                return TryCopyTo(obj.AsSpan(), destination, out charsWritten);
            }

            if (typeof(T) == typeof(ArraySegment<char>))
            {
                var obj = Unsafe.As<T, ArraySegment<char>>(ref value!);
                return TryCopyTo(obj.AsSpan(), destination, out charsWritten);
            }

            if (typeof(T) == typeof(ReadOnlyMemory<char>))
            {
                var obj = Unsafe.As<T, ReadOnlyMemory<char>>(ref value!);
                return TryCopyTo(obj.Span, destination, out charsWritten);
            }

            if (typeof(T) == typeof(Memory<char>))
            {
                var obj = Unsafe.As<T, Memory<char>>(ref value!);
                return TryCopyTo(obj.Span, destination, out charsWritten);
            }

            if (typeof(T) == typeof(bool))
                return Unsafe.As<T, bool>(ref value!).TryFormat(destination, out charsWritten);

            if (typeof(T) == typeof(decimal))
                return Unsafe.As<T, decimal>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(DateTime))
                return Unsafe.As<T, DateTime>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(byte))
                return Unsafe.As<T, byte>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(DateTimeOffset))
                return Unsafe.As<T, DateTimeOffset>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(double))
                return Unsafe.As<T, double>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(Guid))
                return Unsafe.As<T, Guid>(ref value!).TryFormat(destination, out charsWritten, format);

#if NET5_0_OR_GREATER
            if (typeof(T) == typeof(Half))
                return Unsafe.As<T, Half>(ref value!).TryFormat(destination, out charsWritten, format, provider);
#endif

            if (typeof(T) == typeof(short))
                return Unsafe.As<T, short>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(int))
                return Unsafe.As<T, int>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(long))
                return Unsafe.As<T, long>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(sbyte))
                return Unsafe.As<T, sbyte>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(float))
                return Unsafe.As<T, float>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(TimeSpan))
                return Unsafe.As<T, TimeSpan>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(ushort))
                return Unsafe.As<T, ushort>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(uint))
                return Unsafe.As<T, uint>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(ulong))
                return Unsafe.As<T, ulong>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(nint))
                return Environment.Is64BitProcess ? Unsafe.As<T, long>(ref value!).TryFormat(destination, out charsWritten, format, provider) : Unsafe.As<T, int>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(nuint))
                return Environment.Is64BitProcess ? Unsafe.As<T, ulong>(ref value!).TryFormat(destination, out charsWritten, format, provider) : Unsafe.As<T, uint>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(Version))
            {
                var obj = Unsafe.As<T?, Version?>(ref value);
                if (obj != null)
                    return obj.TryFormat(destination, out charsWritten);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(ArraySegment<char>?))
            {
                var nullable = Unsafe.As<T?, ArraySegment<char>?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return TryCopyTo(nullable.GetValueOrDefault().AsSpan(), destination, out charsWritten);
            }

            if (typeof(T) == typeof(ReadOnlyMemory<char>?))
            {
                var nullable = Unsafe.As<T?, ReadOnlyMemory<char>?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return TryCopyTo(nullable.GetValueOrDefault().Span, destination, out charsWritten);
            }

            if (typeof(T) == typeof(Memory<char>?))
            {
                var nullable = Unsafe.As<T?, Memory<char>?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return TryCopyTo(nullable.GetValueOrDefault().Span, destination, out charsWritten);
            }

            if (typeof(T) == typeof(bool?))
            {
                var nullable = Unsafe.As<T?, bool?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten);
            }

            if (typeof(T) == typeof(decimal?))
            {
                var nullable = Unsafe.As<T?, decimal?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(DateTime?))
            {
                var nullable = Unsafe.As<T?, DateTime?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(byte?))
            {
                var nullable = Unsafe.As<T?, byte?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(DateTimeOffset?))
            {
                var nullable = Unsafe.As<T?, DateTimeOffset?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(double?))
            {
                var nullable = Unsafe.As<T?, double?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(Guid?))
            {
                var nullable = Unsafe.As<T?, Guid?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format);
            }

#if NET5_0_OR_GREATER
            if (typeof(T) == typeof(Half?))
            {
                var nullable = Unsafe.As<T?, Half?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }
#endif

            if (typeof(T) == typeof(short?))
            {
                var nullable = Unsafe.As<T?, short?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(int?))
            {
                var nullable = Unsafe.As<T?, int?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(long?))
            {
                var nullable = Unsafe.As<T?, long?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(sbyte?))
            {
                var nullable = Unsafe.As<T?, sbyte?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(float?))
            {
                var nullable = Unsafe.As<T?, float?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(TimeSpan?))
            {
                var nullable = Unsafe.As<T?, TimeSpan?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(ushort?))
            {
                var nullable = Unsafe.As<T?, ushort?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(uint?))
            {
                var nullable = Unsafe.As<T?, uint?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(ulong?))
            {
                var nullable = Unsafe.As<T?, ulong?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(nint?))
            {
                var nullable = Unsafe.As<T?, nint?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return Environment.Is64BitProcess ? I64.FromIsizeUnchecked(nullable.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider) : I32.FromIsizeUnchecked(nullable.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(nuint?))
            {
                var nullable = Unsafe.As<T?, nuint?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return Environment.Is64BitProcess ? U64.FromUsizeUnchecked(nullable.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider) : U32.FromUsizeUnchecked(nullable.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider);
            }

            return TryFormatFallback(value, destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Formats the specified value using the fallback formatting paths. Formatting is
        ///     attempted with the span-formattable and UTF-8 span-formattable interfaces, then with
        ///     <see cref="IFormattable" />, and finally with the string representation of the value.
        /// </summary>
        /// <typeparam name="T">The type of the value to format.</typeparam>
        /// <param name="value">The value to format.</param>
        /// <param name="destination">The buffer to receive the formatted characters.</param>
        /// <param name="charsWritten">
        ///     When this method returns, the number of characters written to <paramref name="destination" />.
        /// </param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was formatted into the destination;
        ///     otherwise, <see langword="false" /> if the destination was too small.
        /// </returns>
        /// <remarks>
        ///     When <paramref name="format" /> is empty, <see langword="null" /> is passed as the
        ///     format string to <see cref="IFormattable" /> so that the value uses its default formatting.
        /// </remarks>
        [SkipLocalsInit]
        private static bool TryFormatFallback<T>(T? value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
#if NET6_0_OR_GREATER
            if (value is ISpanFormattable spanFormattable)
                return spanFormattable.TryFormat(destination, out charsWritten, format, provider);
#endif

#if NET8_0_OR_GREATER
            if (value is IUtf8SpanFormattable utf8SpanFormattable)
            {
                using (var temp = UnsafeStringBuilder<byte>.From(stackalloc byte[1024], 0))
                {
                    temp.AppendFormattable(utf8SpanFormattable, format, provider);
                    return TryGetChars(temp.Text, destination, out charsWritten);
                }
            }
#endif

            var result = value is IFormattable formattable ? formattable.ToString(format.IsEmpty ? null : format.ToString(), provider) : value?.ToString();
            var obj = (result ?? "").AsSpan();
            return TryCopyTo(obj, destination, out charsWritten);
        }

        /// <summary>
        ///     Copies the contents of this read-only span into destination span. If the
        ///     source and destinations overlap, this method behaves as if the original
        ///     values in a temporary location before the destination is overwritten.
        /// </summary>
        /// <param name="source">The read-only span of characters to copy.</param>
        /// <param name="destination">The destination span to receive the characters.</param>
        /// <param name="charsWritten">
        ///     When this method returns, the number of characters copied to
        ///     <paramref name="destination" />, or <c>0</c> if <paramref name="destination" /> is too small.
        /// </param>
        /// <returns>
        ///     If the destination span is shorter than the source span, this method
        ///     return <see langword="false" /> and no data is written to the destination.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryCopyTo(ReadOnlySpan<char> source, Span<char> destination, out int charsWritten)
        {
            if (source.TryCopyTo(destination))
            {
                charsWritten = source.Length;
                return true;
            }

            charsWritten = 0;
            return false;
        }

#if NET8_0_OR_GREATER
        /// <summary>
        ///     Decodes into a span of chars a set of bytes from the
        ///     specified read-only span if the destination is large enough.
        /// </summary>
        /// <param name="bytes">A read-only span containing the sequence of bytes to decode.</param>
        /// <param name="chars">The character span receiving the decoded bytes.</param>
        /// <param name="charsWritten">
        ///     Upon successful completion of the operation, the number of chars decoded into <paramref name="chars" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if all of the characters were decoded into the destination;
        ///     <see langword="false" /> if the destination was too small to contain all the decoded chars.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetChars(ReadOnlySpan<byte> bytes, Span<char> chars, out int charsWritten) => Encoding.UTF8.TryGetChars(bytes, chars, out charsWritten);
#endif
    }
}