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
    internal static unsafe class Utf8FormatHelpers
    {
        /// <summary>
        ///     Gets whether the provider provides a custom formatter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasCustomFormatter(IFormatProvider provider) => provider.GetType() != typeof(CultureInfo) && provider.GetFormat(typeof(ICustomFormatter)) != null;

        /// <summary>
        ///     Try format
        /// </summary>
        public static bool TryFormat<T>(T? value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (typeof(T) == typeof(string))
            {
                var obj = Unsafe.As<T?, string?>(ref value);
                if (obj == null)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryGetBytes(obj.AsSpan(), destination, out bytesWritten);
            }

            if (typeof(T) == typeof(ArraySegment<char>))
            {
                var obj = Unsafe.As<T, ArraySegment<char>>(ref value!);
                return TryGetBytes(obj.AsSpan(), destination, out bytesWritten);
            }

            if (typeof(T) == typeof(ReadOnlyMemory<char>))
            {
                var obj = Unsafe.As<T, ReadOnlyMemory<char>>(ref value!);
                return TryGetBytes(obj.Span, destination, out bytesWritten);
            }

            if (typeof(T) == typeof(Memory<char>))
            {
                var obj = Unsafe.As<T, Memory<char>>(ref value!);
                return TryGetBytes(obj.Span, destination, out bytesWritten);
            }

            if (typeof(T) == typeof(bool))
                return TryGetBytes(Unsafe.As<T, bool>(ref value!).ToString().AsSpan(), destination, out bytesWritten);

            if (typeof(T) == typeof(decimal))
                return TryFormatUtf8(Unsafe.As<T, decimal>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(DateTime))
                return TryFormatUtf8(Unsafe.As<T, DateTime>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(byte))
                return TryFormatUtf8(Unsafe.As<T, byte>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(DateTimeOffset))
                return TryFormatUtf8(Unsafe.As<T, DateTimeOffset>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(double))
                return TryFormatUtf8(Unsafe.As<T, double>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(Guid))
                return TryFormatUtf8(Unsafe.As<T, Guid>(ref value!), destination, out bytesWritten, format, provider);

#if NET5_0_OR_GREATER
            if (typeof(T) == typeof(Half))
                return TryFormatUtf8(Unsafe.As<T, Half>(ref value!), destination, out bytesWritten, format, provider);
#endif

            if (typeof(T) == typeof(short))
                return TryFormatUtf8(Unsafe.As<T, short>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(int))
                return TryFormatUtf8(Unsafe.As<T, int>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(long))
                return TryFormatUtf8(Unsafe.As<T, long>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(sbyte))
                return TryFormatUtf8(Unsafe.As<T, sbyte>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(float))
                return TryFormatUtf8(Unsafe.As<T, float>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(TimeSpan))
                return TryFormatUtf8(Unsafe.As<T, TimeSpan>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(ushort))
                return TryFormatUtf8(Unsafe.As<T, ushort>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(uint))
                return TryFormatUtf8(Unsafe.As<T, uint>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(ulong))
                return TryFormatUtf8(Unsafe.As<T, ulong>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(nint))
                return sizeof(nint) == 8 ? TryFormatUtf8(Unsafe.As<T, long>(ref value!), destination, out bytesWritten, format, provider) : TryFormatUtf8(Unsafe.As<T, int>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(nuint))
                return sizeof(nint) == 8 ? TryFormatUtf8(Unsafe.As<T, ulong>(ref value!), destination, out bytesWritten, format, provider) : TryFormatUtf8(Unsafe.As<T, uint>(ref value!), destination, out bytesWritten, format, provider);

            if (typeof(T) == typeof(Version))
            {
                var obj = Unsafe.As<T?, Version?>(ref value);
                if (obj == null)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(obj, destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(ArraySegment<char>?))
            {
                var nullable = Unsafe.As<T?, ArraySegment<char>?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryGetBytes(nullable.GetValueOrDefault().AsSpan(), destination, out bytesWritten);
            }

            if (typeof(T) == typeof(ReadOnlyMemory<char>?))
            {
                var nullable = Unsafe.As<T?, ReadOnlyMemory<char>?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryGetBytes(nullable.GetValueOrDefault().Span, destination, out bytesWritten);
            }

            if (typeof(T) == typeof(Memory<char>?))
            {
                var nullable = Unsafe.As<T?, Memory<char>?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryGetBytes(nullable.GetValueOrDefault().Span, destination, out bytesWritten);
            }

            if (typeof(T) == typeof(bool?))
            {
                var nullable = Unsafe.As<T?, bool?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryGetBytes(nullable.GetValueOrDefault().ToString().AsSpan(), destination, out bytesWritten);
            }

            if (typeof(T) == typeof(decimal?))
            {
                var nullable = Unsafe.As<T?, decimal?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(DateTime?))
            {
                var nullable = Unsafe.As<T?, DateTime?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(byte?))
            {
                var nullable = Unsafe.As<T?, byte?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(DateTimeOffset?))
            {
                var nullable = Unsafe.As<T?, DateTimeOffset?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(double?))
            {
                var nullable = Unsafe.As<T?, double?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(Guid?))
            {
                var nullable = Unsafe.As<T?, Guid?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

#if NET5_0_OR_GREATER
            if (typeof(T) == typeof(Half?))
            {
                var nullable = Unsafe.As<T?, Half?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }
#endif

            if (typeof(T) == typeof(short?))
            {
                var nullable = Unsafe.As<T?, short?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(int?))
            {
                var nullable = Unsafe.As<T?, int?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(long?))
            {
                var nullable = Unsafe.As<T?, long?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(sbyte?))
            {
                var nullable = Unsafe.As<T?, sbyte?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(float?))
            {
                var nullable = Unsafe.As<T?, float?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(TimeSpan?))
            {
                var nullable = Unsafe.As<T?, TimeSpan?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(ushort?))
            {
                var nullable = Unsafe.As<T?, ushort?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(uint?))
            {
                var nullable = Unsafe.As<T?, uint?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(ulong?))
            {
                var nullable = Unsafe.As<T?, ulong?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return TryFormatUtf8(nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(nint?))
            {
                var nullable = Unsafe.As<T?, nint?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return sizeof(nint) == 8 ? TryFormatUtf8((long)nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider) : TryFormatUtf8((int)nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            if (typeof(T) == typeof(nuint?))
            {
                var nullable = Unsafe.As<T?, nuint?>(ref value);
                if (!nullable.HasValue)
                {
                    bytesWritten = 0;
                    return true;
                }

                return sizeof(nint) == 8 ? TryFormatUtf8((ulong)nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider) : TryFormatUtf8((uint)nullable.GetValueOrDefault(), destination, out bytesWritten, format, provider);
            }

            return TryFormatFallback(value, destination, out bytesWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool TryFormatFallback<T>(T? value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
#if NET8_0_OR_GREATER
            if (value is IUtf8SpanFormattable utf8SpanFormattable)
                return utf8SpanFormattable.TryFormat(destination, out bytesWritten, format, provider);
#endif

#if NET6_0_OR_GREATER
            if (value is ISpanFormattable spanFormattable)
            {
                using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
                temp.AppendFormattable(spanFormattable, format, provider);
                return TryGetBytes(temp.Text, destination, out bytesWritten);
            }
#endif

            var result = value is IFormattable formattable ? formattable.ToString(format.ToString(), provider) : value?.ToString();
            var obj = (result ?? "").AsSpan();
            return TryGetBytes(obj, destination, out bytesWritten);
        }

        /// <summary>
        ///     Try format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatUtf8<T>(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
#if NET8_0_OR_GREATER
            where T : IUtf8SpanFormattable
#elif NET6_0_OR_GREATER
            where T : ISpanFormattable
#endif
        {
#if NET8_0_OR_GREATER
            return value.TryFormat(destination, out bytesWritten, format, provider);
#elif NET6_0_OR_GREATER
            using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
            temp.AppendFormattable(value, format, provider);
            return TryGetBytes(temp.Text, destination, out bytesWritten);
#else
            using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
            temp.AppendFormat(value, format, provider);
            return TryGetBytes(temp.Text, destination, out bytesWritten);
#endif
        }

        /// <summary>
        ///     Encodes into a span of bytes a set of characters from the specified read-only span if the destination is large
        ///     enough.
        /// </summary>
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
        public static bool TryGetBytes(ReadOnlySpan<char> chars, Span<byte> bytes, out int bytesWritten) => EncodingHelpers.TryGetBytes(Encoding.UTF8, chars, bytes, out bytesWritten);

        /// <summary>
        ///     Handler
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct Handler
        {
            /// <summary>
            ///     Try format
            /// </summary>
            public readonly delegate* managed<object, Span<byte>, out int, ReadOnlySpan<char>, IFormatProvider?, bool> TryFormat;

            /// <summary>
            ///     Structure
            /// </summary>
            public Handler(delegate* managed<object, Span<byte>, out int, ReadOnlySpan<char>, IFormatProvider?, bool> tryFormat) => TryFormat = tryFormat;
        }
    }
}