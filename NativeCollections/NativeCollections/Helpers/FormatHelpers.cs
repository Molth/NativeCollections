using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable CA2208
#pragma warning disable CS8618
#pragma warning disable CS8632
#pragma warning disable CS8500

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Format helpers
    /// </summary>
    internal static unsafe class FormatHelpers
    {
        /// <summary>
        ///     Gets whether the provider provides a custom formatter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasCustomFormatter(IFormatProvider provider) => provider.GetType() != typeof(CultureInfo) && provider.GetFormat(typeof(ICustomFormatter)) != null;

        /// <summary>
        ///     Try format
        /// </summary>
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

                return Environment.Is64BitProcess ? ((long)nullable.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider) : ((int)nullable.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider);
            }

            if (typeof(T) == typeof(nuint?))
            {
                var nullable = Unsafe.As<T?, nuint?>(ref value);
                if (!nullable.HasValue)
                {
                    charsWritten = 0;
                    return true;
                }

                return Environment.Is64BitProcess ? ((ulong)nullable.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider) : ((uint)nullable.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider);
            }

            return TryFormatFallback(value, destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool TryFormatFallback<T>(T? value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
#if NET6_0_OR_GREATER
            if (value is ISpanFormattable spanFormattable)
                return spanFormattable.TryFormat(destination, out charsWritten, format, provider);
#endif

#if NET8_0_OR_GREATER
            if (value is IUtf8SpanFormattable utf8SpanFormattable)
            {
                using var temp = new NativeStringBuilder<byte>(stackalloc byte[1024], 0);
                temp.AppendFormattable(utf8SpanFormattable, format, provider);
                return TryGetChars(temp.Text, destination, out charsWritten);
            }
#endif

            var result = value is IFormattable formattable ? formattable.ToString(format.ToString(), provider) : value?.ToString();
            var obj = (result ?? "").AsSpan();
            return TryCopyTo(obj, destination, out charsWritten);
        }

        /// <summary>
        ///     Try copy to
        /// </summary>
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

        /// <summary>
        ///     Decodes into a span of chars a set of bytes from the specified read-only span if the destination is large
        ///     enough.
        /// </summary>
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
        public static bool TryGetChars(ReadOnlySpan<byte> bytes, Span<char> chars, out int charsWritten) => EncodingHelpers.TryGetChars(Encoding.UTF8, bytes, chars, out charsWritten);

        /// <summary>
        ///     Handler
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct Handler
        {
            /// <summary>
            ///     Try format
            /// </summary>
            public readonly delegate* managed<object, Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?, bool> TryFormat;

            /// <summary>
            ///     Structure
            /// </summary>
            public Handler(delegate* managed<object, Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?, bool> tryFormat) => TryFormat = tryFormat;
        }
    }
}