using System;
using System.Reflection;
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
    internal static class FormatHelpers<T>
    {
        /// <summary>
        ///     Delegate
        /// </summary>
        private static object _delegate;

        /// <summary>
        ///     Structure
        /// </summary>
        static FormatHelpers()
        {
            var method = typeof(T).GetMethod("TryFormat", BindingFlags.Public | BindingFlags.Instance, null, FormatHelpers.ParameterTypes4, null);
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

            method = typeof(T).GetMethod("TryFormat", BindingFlags.Public | BindingFlags.Instance, null, FormatHelpers.ParameterTypes3, null);
            if (method != null)
            {
                if (typeof(T).IsValueType)
                {
                    var format3ValueType = (Format3ValueTypeDelegate)method.CreateDelegate(typeof(Format3ValueTypeDelegate), null);
                    FormatValueType = Format3ValueType;
                    RegisterValueType();
                    bool Format3ValueType(ref T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => format3ValueType(ref value, destination, out charsWritten, format);
                }
                else
                {
                    var format3NotValueType = (Format3NotValueTypeDelegate)method.CreateDelegate(typeof(Format3NotValueTypeDelegate), null);
                    FormatNotValueType = Format3NotValueType;
                    RegisterNotValueType();
                    bool Format3NotValueType(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => format3NotValueType(value, destination, out charsWritten, format);
                }

                return;
            }

            method = typeof(T).GetMethod("TryFormat", BindingFlags.Public | BindingFlags.Instance, null, FormatHelpers.ParameterTypes2, null);
            if (method != null)
            {
                if (typeof(T).IsValueType)
                {
                    var format2ValueType = (Format2ValueTypeDelegate)method.CreateDelegate(typeof(Format2ValueTypeDelegate), null);
                    FormatValueType = Format2ValueType;
                    RegisterValueType();
                    bool Format2ValueType(ref T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => format2ValueType(ref value, destination, out charsWritten);
                }
                else
                {
                    var format2NotValueType = (Format2NotValueTypeDelegate)method.CreateDelegate(typeof(Format2NotValueTypeDelegate), null);
                    FormatNotValueType = Format2NotValueType;
                    RegisterNotValueType();
                    bool Format2NotValueType(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => format2NotValueType(value, destination, out charsWritten);
                }

                return;
            }

            if (typeof(IFormattable).IsAssignableFrom(typeof(T)))
            {
                method = typeof(T).GetMethod("TryFormat", BindingFlags.Public | BindingFlags.Instance, null, FormatHelpers.ParameterTypes0, null);
                if (method != null)
                {
                    if (typeof(T).IsValueType)
                    {
                        var format0ValueType = (Format0ValueTypeDelegate)method.CreateDelegate(typeof(Format0ValueTypeDelegate), null);
                        FormatValueType = Format0ValueType;
                        RegisterValueType();

                        bool Format0ValueType(ref T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                        {
                            var result = format0ValueType(ref value, format.ToString(), provider);
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
                        var format0NotValueType = (Format0NotValueTypeDelegate)method.CreateDelegate(typeof(Format0NotValueTypeDelegate), null);
                        FormatNotValueType = Format0NotValueType;
                        RegisterNotValueType();

                        bool Format0NotValueType(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                        {
                            var result = format0NotValueType(value, format.ToString(), provider);
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
        public static bool TryFormat(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) => typeof(T).IsValueType ? FormatValueType(ref value, destination, out charsWritten, format, provider) : FormatNotValueType(value, destination, out charsWritten, format, provider);

        /// <summary>
        ///     Initialize
        /// </summary>
        public static void Initialize()
        {
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatValueTypeFallback(ref T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
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

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNotValueTypeFallback(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
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

        /// <summary>
        ///     Register
        /// </summary>
        private static void RegisterValueType()
        {
            var method = FormatHelpers.UnboxMethodInfo.MakeGenericMethod(typeof(T));
            var unbox = (UnboxDelegate)Delegate.CreateDelegate(typeof(UnboxDelegate), method);
            FormatHelpers.Format4Delegates[typeof(T)] = Format4ValueTypeUnbox;
            return;

            bool Format4ValueTypeUnbox(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            {
                var obj = unbox(value);
                return FormatValueType(ref obj, destination, out charsWritten, format, provider);
            }
        }

        /// <summary>
        ///     Register
        /// </summary>
        private static void RegisterNotValueType() => FormatHelpers.Format4Delegates[typeof(T)] = Format4NotValueType;

        /// <summary>
        ///     Format
        /// </summary>
        private static bool Format4NotValueType(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.As<object, T>(ref value);
            return FormatNotValueType(obj, destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private delegate bool Format4ValueTypeDelegate(ref T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider);

        /// <summary>
        ///     Format
        /// </summary>
        private delegate bool Format4NotValueTypeDelegate(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider);

        /// <summary>
        ///     Format
        /// </summary>
        private delegate bool Format3ValueTypeDelegate(ref T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format);

        /// <summary>
        ///     Format
        /// </summary>
        private delegate bool Format3NotValueTypeDelegate(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format);

        /// <summary>
        ///     Format
        /// </summary>
        private delegate bool Format2ValueTypeDelegate(ref T value, Span<char> destination, out int charsWritten);

        /// <summary>
        ///     Format
        /// </summary>
        private delegate bool Format2NotValueTypeDelegate(T value, Span<char> destination, out int charsWritten);

        /// <summary>
        ///     Format
        /// </summary>
        private delegate string Format0ValueTypeDelegate(ref T value, string? format, IFormatProvider? formatProvider);

        /// <summary>
        ///     Format
        /// </summary>
        private delegate string Format0NotValueTypeDelegate(T value, string? format, IFormatProvider? formatProvider);

        /// <summary>
        ///     Unbox
        /// </summary>
        private delegate T UnboxDelegate(object obj);
    }
}