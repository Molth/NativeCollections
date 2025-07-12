using System;
using System.Runtime.CompilerServices;

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
                TryFormat = &FormatNotValueTypeByISpanFormattable;
                if (typeof(T).IsValueType)
                    RegisterValueType();
                else
                    RegisterNotValueType();

                return;

                static bool FormatNotValueTypeByISpanFormattable(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var utf8SpanFormattable = (IUtf8SpanFormattable)value!;
                    return utf8SpanFormattable.TryFormat(destination, out bytesWritten, format, provider);
                }
            }
#endif

            if (typeof(T).IsValueType)
            {
                TryFormat = &FormatValueTypeFallback;
                RegisterValueType();

                static bool FormatValueTypeFallback(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
                    temp.AppendFormat(value, format, provider);
                    return Utf8FormatHelpers.TryGetBytes(temp.Text, destination, out bytesWritten);
                }
            }
            else
            {
                TryFormat = &FormatNotValueTypeFallback;
                RegisterNotValueType();

                static bool FormatNotValueTypeFallback(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
                    temp.AppendFormat(value, format, provider);
                    return Utf8FormatHelpers.TryGetBytes(temp.Text, destination, out bytesWritten);
                }
            }
        }

        /// <summary>
        ///     Register
        /// </summary>
        private static void RegisterValueType()
        {
#if NET8_0_OR_GREATER
            if (typeof(IUtf8SpanFormattable).IsAssignableFrom(typeof(T)))
            {
                Utf8FormatHelpers.Format4Delegates[typeof(T)] = new Utf8FormatHelpers.Handler(&FormatNotValueTypeByISpanFormattable);
                return;

                static bool FormatNotValueTypeByISpanFormattable(object value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var utf8SpanFormattable = (IUtf8SpanFormattable)value;
                    return utf8SpanFormattable.TryFormat(destination, out bytesWritten, format, provider);
                }
            }
#endif

            Utf8FormatHelpers.Format4Delegates[typeof(T)] = new Utf8FormatHelpers.Handler(&Format4ValueType);
            return;

            static bool Format4ValueType(object value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            {
                var obj = (T)value;
                return TryFormat(obj, destination, out bytesWritten, format, provider);
            }
        }

        /// <summary>
        ///     Register
        /// </summary>
        private static void RegisterNotValueType()
        {
            Utf8FormatHelpers.Format4Delegates[typeof(T)] = new Utf8FormatHelpers.Handler(&Format4NotValueType);
            return;

            static bool Format4NotValueType(object value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            {
                var obj = Unsafe.As<object, T>(ref value);
                return TryFormat(obj, destination, out bytesWritten, format, provider);
            }
        }
    }
}