using System;

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
    /// <typeparam name="T">Type</typeparam>
    internal static unsafe class Utf8FormatHelpers<T>
    {
        /// <summary>
        ///     Try format
        /// </summary>
        public static readonly delegate* managed<T, Span<byte>, out int, ReadOnlySpan<char>, IFormatProvider?, bool> TryFormat;

        /// <summary>
        ///     Structure
        /// </summary>
        static Utf8FormatHelpers()
        {
#if NET8_0_OR_GREATER
            if (typeof(IUtf8SpanFormattable).IsAssignableFrom(typeof(T)))
            {
                TryFormat = &FormatTypeByIUtf8SpanFormattable;
                Utf8FormatHelpers.Format4Delegates[typeof(T)] = new Utf8FormatHelpers.Handler(&FormatTypeByIUtf8SpanFormattableObject);
                return;

                static bool FormatTypeByIUtf8SpanFormattable(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var utf8SpanFormattable = (IUtf8SpanFormattable)value!;
                    return utf8SpanFormattable.TryFormat(destination, out bytesWritten, format, provider);
                }

                static bool FormatTypeByIUtf8SpanFormattableObject(object value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var utf8SpanFormattable = (IUtf8SpanFormattable)value;
                    return utf8SpanFormattable.TryFormat(destination, out bytesWritten, format, provider);
                }
            }
#endif

#if NET6_0_OR_GREATER
            if (typeof(ISpanFormattable).IsAssignableFrom(typeof(T)))
            {
                TryFormat = &FormatByISpanFormattable;
                Utf8FormatHelpers.Format4Delegates[typeof(T)] = new Utf8FormatHelpers.Handler(&FormatByISpanFormattableObject);
                return;

                static bool FormatByISpanFormattable(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var spanFormattable = (ISpanFormattable)value!;
                    using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
                    temp.AppendFormattable(spanFormattable);
                    return Utf8FormatHelpers.TryGetBytes(temp.Text, destination, out bytesWritten);
                }

                static bool FormatByISpanFormattableObject(object value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var spanFormattable = (ISpanFormattable)value;
                    using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
                    temp.AppendFormattable(spanFormattable);
                    return Utf8FormatHelpers.TryGetBytes(temp.Text, destination, out bytesWritten);
                }
            }
#endif

            if (typeof(IFormattable).IsAssignableFrom(typeof(T)))
            {
                TryFormat = &FormatByIFormattable;
                Utf8FormatHelpers.Format4Delegates[typeof(T)] = new Utf8FormatHelpers.Handler(&FormatByIFormattableObject);
                return;

                static bool FormatByIFormattable(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var formattable = (IFormattable)value!;
                    var result = formattable.ToString(format.ToString(), provider);
                    var obj = (result != null ? result : "").AsSpan();
                    return Utf8FormatHelpers.TryGetBytes(obj, destination, out bytesWritten);
                }

                static bool FormatByIFormattableObject(object value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var formattable = (IFormattable)value;
                    var result = formattable.ToString(format.ToString(), provider);
                    var obj = (result != null ? result : "").AsSpan();
                    return Utf8FormatHelpers.TryGetBytes(obj, destination, out bytesWritten);
                }
            }

            TryFormat = &FormatFallback;
            Utf8FormatHelpers.Format4Delegates[typeof(T)] = new Utf8FormatHelpers.Handler(&FormatFallbackObject);
            return;

            static bool FormatFallback(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            {
                var result = value!.ToString() ?? "";
                return Utf8FormatHelpers.TryGetBytes(result.AsSpan(), destination, out bytesWritten);
            }

            static bool FormatFallbackObject(object value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            {
                var result = value.ToString() ?? "";
                return Utf8FormatHelpers.TryGetBytes(result.AsSpan(), destination, out bytesWritten);
            }
        }
    }
}