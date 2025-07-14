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
    internal static unsafe class FormatHelpers<T>
    {
        /// <summary>
        ///     Try format
        /// </summary>
        public static readonly delegate* managed<T, Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?, bool> TryFormat;

        /// <summary>
        ///     Structure
        /// </summary>
        static FormatHelpers()
        {
#if NET6_0_OR_GREATER
            if (typeof(ISpanFormattable).IsAssignableFrom(typeof(T)))
            {
                TryFormat = &FormatByISpanFormattable;
                FormatHelpers.Format4Delegates[typeof(T)] = new FormatHelpers.Handler(&FormatByISpanFormattableObject);
                return;

                static bool FormatByISpanFormattable(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var spanFormattable = (ISpanFormattable)value!;
                    return spanFormattable.TryFormat(destination, out charsWritten, format, provider);
                }

                static bool FormatByISpanFormattableObject(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var spanFormattable = (ISpanFormattable)value;
                    return spanFormattable.TryFormat(destination, out charsWritten, format, provider);
                }
            }
#endif

#if NET8_0_OR_GREATER
            if (typeof(IUtf8SpanFormattable).IsAssignableFrom(typeof(T)))
            {
                TryFormat = &FormatByIUtf8SpanFormattable;
                FormatHelpers.Format4Delegates[typeof(T)] = new FormatHelpers.Handler(&FormatByIUtf8SpanFormattableObject);
                return;

                static bool FormatByIUtf8SpanFormattable(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var utf8SpanFormattable = (IUtf8SpanFormattable)value!;
                    using var temp = new NativeStringBuilder<byte>(stackalloc byte[1024], 0);
                    temp.AppendFormattable(utf8SpanFormattable);
                    return FormatHelpers.TryGetChars(temp.Text, destination, out charsWritten);
                }

                static bool FormatByIUtf8SpanFormattableObject(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var utf8SpanFormattable = (IUtf8SpanFormattable)value;
                    using var temp = new NativeStringBuilder<byte>(stackalloc byte[1024], 0);
                    temp.AppendFormattable(utf8SpanFormattable);
                    return FormatHelpers.TryGetChars(temp.Text, destination, out charsWritten);
                }
            }
#endif

            if (typeof(IFormattable).IsAssignableFrom(typeof(T)))
            {
                TryFormat = &FormatByIFormattable;
                FormatHelpers.Format4Delegates[typeof(T)] = new FormatHelpers.Handler(&FormatByIFormattableObject);
                return;

                static bool FormatByIFormattable(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var formattable = (IFormattable)value!;
                    var result = formattable.ToString(format.ToString(), provider);
                    var obj = (result != null ? result : "").AsSpan();
                    return FormatHelpers.TryCopyTo(obj, destination, out charsWritten);
                }

                static bool FormatByIFormattableObject(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var formattable = (IFormattable)value;
                    var result = formattable.ToString(format.ToString(), provider);
                    var obj = (result != null ? result : "").AsSpan();
                    return FormatHelpers.TryCopyTo(obj, destination, out charsWritten);
                }
            }

            TryFormat = &FormatFallback;
            FormatHelpers.Format4Delegates[typeof(T)] = new FormatHelpers.Handler(&FormatFallbackObject);
            return;

            static bool FormatFallback(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            {
                var result = value!.ToString() ?? "";
                if (result.AsSpan().TryCopyTo(destination))
                {
                    charsWritten = result.Length;
                    return true;
                }

                charsWritten = 0;
                return false;
            }

            static bool FormatFallbackObject(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            {
                var result = value.ToString() ?? "";
                if (result.AsSpan().TryCopyTo(destination))
                {
                    charsWritten = result.Length;
                    return true;
                }

                charsWritten = 0;
                return false;
            }
        }
    }
}