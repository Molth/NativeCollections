using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Herta
{
    internal static class FormatHelpers<T>
    {
        private static readonly Format4ValueTypeDelegate? FormatValueType;
        private static readonly Format4NotValueTypeDelegate? FormatNotValueType;

        static FormatHelpers()
        {
            if (typeof(T).IsEnum)
            {
#if NET7_0_OR_GREATER
                var method1 = typeof(Enum).GetMethod(nameof(Enum.TryFormat), BindingFlags.Public | BindingFlags.Static);
                if (method1 != null)
                {
                    var method2 = method1.MakeGenericMethod(typeof(T));
                    if (method2 != null)
                    {
                        var format3ValueType = (Format3NotValueTypeDelegate)Delegate.CreateDelegate(typeof(Format3NotValueTypeDelegate), method2);
                        FormatValueType = Format3ValueType;
                        RegisterValueType();

                        return;

                        bool Format3ValueType(ref T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => format3ValueType(value, destination, out charsWritten, format);
                    }
                }
#endif

                FormatValueType = FormatValueTypeFallback;
                RegisterValueType();

                return;
            }

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

            if (typeof(T).IsAssignableTo(typeof(IFormattable)))
            {
                method = typeof(T).GetMethod("TryFormat", BindingFlags.Public | BindingFlags.Instance, null, FormatHelpers.ParameterTypes0, null)!;

                if (typeof(T).IsValueType)
                {
                    var format0ValueType = (Format0ValueTypeDelegate)method.CreateDelegate(typeof(Format0ValueTypeDelegate), null);
                    FormatValueType = Format0ValueType;
                    RegisterValueType();

                    bool Format0ValueType(ref T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                    {
                        var result = format0ValueType(ref value, format.ToString(), provider);
                        if (result.TryCopyTo(destination))
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
                        if (result.TryCopyTo(destination))
                        {
                            charsWritten = result.Length;
                            return true;
                        }

                        charsWritten = 0;
                        return false;
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

            return;

            static bool FormatValueTypeFallback(ref T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            {
                var result = value!.ToString() ?? "";
                if (result.TryCopyTo(destination))
                {
                    charsWritten = result.Length;
                    return true;
                }

                charsWritten = 0;
                return false;
            }

            static bool FormatNotValueTypeFallback(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            {
                var result = value?.ToString() ?? "";
                if (result.TryCopyTo(destination))
                {
                    charsWritten = result.Length;
                    return true;
                }

                charsWritten = 0;
                return false;
            }

            static void RegisterValueType()
            {
                var method1 = typeof(FormatHelpers).GetMethod(nameof(FormatHelpers.Unbox), BindingFlags.Public | BindingFlags.Static);
                if (method1 != null)
                {
                    var method2 = method1.MakeGenericMethod(typeof(T));
                    if (method2 != null)
                    {
                        var unbox = (UnboxDelegate)Delegate.CreateDelegate(typeof(UnboxDelegate), method2);
                        FormatHelpers.Format4Delegates[typeof(T)] = Format4ValueTypeUnbox;
                        return;

                        bool Format4ValueTypeUnbox(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                        {
                            var obj = unbox(value);
                            return FormatValueType!(ref obj, destination, out charsWritten, format, provider);
                        }
                    }
                }

                FormatHelpers.Format4Delegates[typeof(T)] = Format4ValueType;
                return;

                static bool Format4ValueType(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var obj = (T)value;
                    return FormatValueType!(ref obj, destination, out charsWritten, format, provider);
                }
            }

            static void RegisterNotValueType()
            {
                FormatHelpers.Format4Delegates[typeof(T)] = Format4NotValueType;
                return;

                static bool Format4NotValueType(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var obj = Unsafe.As<object, T>(ref value);
                    return FormatNotValueType!(obj, destination, out charsWritten, format, provider);
                }
            }
        }

        public static bool TryFormat(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) => typeof(T).IsValueType ? FormatValueType!(ref value, destination, out charsWritten, format, provider) : FormatNotValueType!(value, destination, out charsWritten, format, provider);

        public static void Initialize()
        {
        }

        private delegate bool Format4ValueTypeDelegate(ref T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider);

        private delegate bool Format4NotValueTypeDelegate(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider);

        private delegate bool Format3ValueTypeDelegate(ref T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format);

        private delegate bool Format3NotValueTypeDelegate(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format);

        private delegate bool Format2ValueTypeDelegate(ref T value, Span<char> destination, out int charsWritten);

        private delegate bool Format2NotValueTypeDelegate(T value, Span<char> destination, out int charsWritten);

        private delegate string Format0ValueTypeDelegate(ref T value, string? format, IFormatProvider? formatProvider);

        private delegate string Format0NotValueTypeDelegate(T value, string? format, IFormatProvider? formatProvider);

        private delegate T UnboxDelegate(object obj);
    }
}