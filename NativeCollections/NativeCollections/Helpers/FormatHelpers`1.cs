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
                if (typeof(T).IsValueType)
                    RegisterValueType();
                else
                    RegisterNotValueType();

                return;

                static bool FormatByISpanFormattable(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var spanFormattable = (ISpanFormattable)value!;
                    return spanFormattable.TryFormat(destination, out charsWritten, format, provider);
                }
            }
#endif

            if (typeof(IFormattable).IsAssignableFrom(typeof(T)))
            {
                TryFormat = &FormatByIFormattable;
                if (typeof(T).IsValueType)
                    RegisterValueType();
                else
                    RegisterNotValueType();

                return;

                static bool FormatByIFormattable(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var formattable = (IFormattable)value!;
                    var result = formattable.ToString(format.ToString(), provider);
                    var obj = (result != null ? result : "").AsSpan();
                    return FormatHelpers.TryCopyTo(obj, destination, out charsWritten);
                }
            }

            if (typeof(T).IsValueType)
            {
                TryFormat = &FormatValueTypeFallback;
                RegisterValueType();

                static bool FormatValueTypeFallback(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
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
            }
            else
            {
                TryFormat = &FormatNotValueTypeFallback;
                RegisterNotValueType();

                static bool FormatNotValueTypeFallback(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var result = value?.ToString() ?? "";
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

        /// <summary>
        ///     Register
        /// </summary>
        private static void RegisterValueType()
        {
#if NET6_0_OR_GREATER
            if (typeof(ISpanFormattable).IsAssignableFrom(typeof(T)))
            {
                FormatHelpers.Format4Delegates[typeof(T)] = new FormatHelpers.Handler(&FormatByISpanFormattable);
                return;

                static bool FormatByISpanFormattable(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var spanFormattable = (ISpanFormattable)value;
                    return spanFormattable.TryFormat(destination, out charsWritten, format, provider);
                }
            }
#endif

            if (typeof(IFormattable).IsAssignableFrom(typeof(T)))
            {
                FormatHelpers.Format4Delegates[typeof(T)] = new FormatHelpers.Handler(&FormatByIFormattable);
                return;

                static bool FormatByIFormattable(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var formattable = (IFormattable)value;
                    var result = formattable.ToString(format.ToString(), provider);
                    var obj = (result != null ? result : "").AsSpan();
                    return FormatHelpers.TryCopyTo(obj, destination, out charsWritten);
                }
            }

            FormatHelpers.Format4Delegates[typeof(T)] = new FormatHelpers.Handler(&Format4ValueType);
            return;

            static bool Format4ValueType(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            {
                var obj = (T)value;
                return TryFormat(obj, destination, out charsWritten, format, provider);
            }
        }

        /// <summary>
        ///     Register
        /// </summary>
        private static void RegisterNotValueType()
        {
            FormatHelpers.Format4Delegates[typeof(T)] = new FormatHelpers.Handler(&Format4NotValueType);
            return;

            static bool Format4NotValueType(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            {
                var obj = Unsafe.As<object, T>(ref value);
                return TryFormat(obj, destination, out charsWritten, format, provider);
            }
        }
    }
}