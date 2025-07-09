using System;
using System.Reflection;
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
    ///     Format helpers
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    internal static class Utf8FormatHelpers<T>
    {
        /// <summary>
        ///     Delegate
        /// </summary>
        private static object _delegate;

        /// <summary>
        ///     Structure
        /// </summary>
        static Utf8FormatHelpers()
        {
            var method = typeof(T).GetMethod("TryFormat", BindingFlags.Public | BindingFlags.Instance, null, Utf8FormatHelpers.ParameterTypes4, null);
            if (method != null)
            {
                if (typeof(T).IsValueType)
                {
                    FormatValueType = (Format4ValueTypeDelegate)method.CreateDelegate(typeof(Format4ValueTypeDelegate), null);
                    RegisterValueType();
                }
                else
                {
                    FormatNotValueType = (Format4NotValueTypeDelegate)method.CreateDelegate(typeof(Format4NotValueTypeDelegate), null);
                    RegisterNotValueType();
                }

                return;
            }

            method = typeof(T).GetMethod("TryFormat", BindingFlags.Public | BindingFlags.Instance, null, Utf8FormatHelpers.ParameterTypes3, null);
            if (method != null)
            {
                if (typeof(T).IsValueType)
                {
                    var format3ValueType = (Format3ValueTypeDelegate)method.CreateDelegate(typeof(Format3ValueTypeDelegate), null);
                    FormatValueType = Format3ValueType;
                    RegisterValueType();
                    bool Format3ValueType(ref T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => format3ValueType(ref value, destination, out bytesWritten, format);
                }
                else
                {
                    var format3NotValueType = (Format3NotValueTypeDelegate)method.CreateDelegate(typeof(Format3NotValueTypeDelegate), null);
                    FormatNotValueType = Format3NotValueType;
                    RegisterNotValueType();
                    bool Format3NotValueType(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => format3NotValueType(value, destination, out bytesWritten, format);
                }

                return;
            }

            method = typeof(T).GetMethod("TryFormat", BindingFlags.Public | BindingFlags.Instance, null, Utf8FormatHelpers.ParameterTypes2, null);
            if (method != null)
            {
                if (typeof(T).IsValueType)
                {
                    var format2ValueType = (Format2ValueTypeDelegate)method.CreateDelegate(typeof(Format2ValueTypeDelegate), null);
                    FormatValueType = Format2ValueType;
                    RegisterValueType();
                    bool Format2ValueType(ref T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => format2ValueType(ref value, destination, out bytesWritten);
                }
                else
                {
                    var format2NotValueType = (Format2NotValueTypeDelegate)method.CreateDelegate(typeof(Format2NotValueTypeDelegate), null);
                    FormatNotValueType = Format2NotValueType;
                    RegisterNotValueType();
                    bool Format2NotValueType(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => format2NotValueType(value, destination, out bytesWritten);
                }

                return;
            }

            if (typeof(T).IsValueType)
            {
                FormatValueType = FormatValueTypeFallback;
                RegisterValueType();
            }
            else
            {
                FormatNotValueType = FormatNotValueTypeFallback;
                RegisterNotValueType();
            }
        }

        /// <summary>
        ///     Value type
        /// </summary>
        private static Format4ValueTypeDelegate FormatValueType
        {
            get => Unsafe.As<object, Format4ValueTypeDelegate>(ref _delegate);
            set => _delegate = value;
        }

        /// <summary>
        ///     Not value type
        /// </summary>
        private static Format4NotValueTypeDelegate FormatNotValueType
        {
            get => Unsafe.As<object, Format4NotValueTypeDelegate>(ref _delegate);
            set => _delegate = value;
        }

        /// <summary>
        ///     Try format
        /// </summary>
        public static bool TryFormat(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) => typeof(T).IsValueType ? FormatValueType(ref value, destination, out bytesWritten, format, provider) : FormatNotValueType(value, destination, out bytesWritten, format, provider);

        /// <summary>
        ///     Initialize
        /// </summary>
        public static void Initialize()
        {
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatValueTypeFallback(ref T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
            temp.AppendFormat(value, format, provider);
            return EncodingHelpers.TryGetBytes(Encoding.UTF8, temp.Text, destination, out bytesWritten);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNotValueTypeFallback(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
            temp.AppendFormat(value, format, provider);
            return EncodingHelpers.TryGetBytes(Encoding.UTF8, temp.Text, destination, out bytesWritten);
        }

        /// <summary>
        ///     Register
        /// </summary>
        private static void RegisterValueType()
        {
            var method = Utf8FormatHelpers.UnboxMethodInfo.MakeGenericMethod(typeof(T));
            var unbox = (UnboxDelegate)Delegate.CreateDelegate(typeof(UnboxDelegate), method);
            Utf8FormatHelpers.Format4Delegates[typeof(T)] = Format4ValueTypeUnbox;
            return;

            bool Format4ValueTypeUnbox(object value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            {
                var obj = unbox(value);
                return FormatValueType(ref obj, destination, out bytesWritten, format, provider);
            }
        }

        /// <summary>
        ///     Register
        /// </summary>
        private static void RegisterNotValueType() => Utf8FormatHelpers.Format4Delegates[typeof(T)] = Format4NotValueType;

        /// <summary>
        ///     Format
        /// </summary>
        private static bool Format4NotValueType(object value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.As<object, T>(ref value);
            return FormatNotValueType(obj, destination, out bytesWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private delegate bool Format4ValueTypeDelegate(ref T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider);

        /// <summary>
        ///     Format
        /// </summary>
        private delegate bool Format4NotValueTypeDelegate(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider);

        /// <summary>
        ///     Format
        /// </summary>
        private delegate bool Format3ValueTypeDelegate(ref T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format);

        /// <summary>
        ///     Format
        /// </summary>
        private delegate bool Format3NotValueTypeDelegate(T value, Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format);

        /// <summary>
        ///     Format
        /// </summary>
        private delegate bool Format2ValueTypeDelegate(ref T value, Span<byte> destination, out int bytesWritten);

        /// <summary>
        ///     Format
        /// </summary>
        private delegate bool Format2NotValueTypeDelegate(T value, Span<byte> destination, out int bytesWritten);

        /// <summary>
        ///     Unbox
        /// </summary>
        private delegate T UnboxDelegate(object obj);
    }
}