using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
        ///     Format
        /// </summary>
        public delegate bool Format4Delegate(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider);

        /// <summary>
        ///     Format
        /// </summary>
        public delegate bool ReadOnlySpanByteFormat4Delegate(ReadOnlySpan<byte> buffer, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider);

        /// <summary>
        ///     Parameter types
        /// </summary>
        public static readonly Type[] ParameterTypes4 = new Type[4]
        {
            typeof(Span<char>),
            typeof(int).MakeByRefType(),
            typeof(ReadOnlySpan<char>),
            typeof(IFormatProvider)
        };

        /// <summary>
        ///     Parameter types
        /// </summary>
        public static readonly Type[] ParameterTypes3 = new Type[3]
        {
            typeof(Span<char>),
            typeof(int).MakeByRefType(),
            typeof(ReadOnlySpan<char>)
        };

        /// <summary>
        ///     Parameter types
        /// </summary>
        public static readonly Type[] ParameterTypes2 = new Type[2]
        {
            typeof(Span<char>),
            typeof(int).MakeByRefType()
        };

        /// <summary>
        ///     Parameter types
        /// </summary>
        public static readonly Type[] ParameterTypes0 = new Type[2]
        {
            typeof(string),
            typeof(IFormatProvider)
        };

        /// <summary>
        ///     Register
        /// </summary>
        private static readonly MethodInfo RegisterFormatReadOnlySpanByteFallbackMethodInfo = typeof(FormatHelpers).GetMethod(nameof(RegisterFormatReadOnlySpanByteFallback), BindingFlags.Static | BindingFlags.NonPublic)!;

        /// <summary>
        ///     Unbox
        /// </summary>
        public static readonly MethodInfo UnboxMethodInfo = typeof(FormatHelpers).GetMethod(nameof(Unbox), BindingFlags.Static | BindingFlags.NonPublic)!;

        /// <summary>
        ///     Format
        /// </summary>
        public static readonly ConcurrentDictionary<Type, Format4Delegate> Format4Delegates = new();

        /// <summary>
        ///     Format
        /// </summary>
        public static readonly ConcurrentDictionary<Type, ReadOnlySpanByteFormat4Delegate> ReadOnlySpanByteFormat4Delegates = new();

        /// <summary>
        ///     Structure
        /// </summary>
        static FormatHelpers()
        {
            Format4Delegates[typeof(string)] = FormatString;
            Format4Delegates[typeof(ReadOnlyMemory<char>)] = FormatReadOnlyMemoryChar;
            Format4Delegates[typeof(Memory<char>)] = FormatMemoryChar;

            Format4Delegates[typeof(bool)] = FormatBoolean;
            Format4Delegates[typeof(decimal)] = FormatDecimal;
            Format4Delegates[typeof(DateTime)] = FormatDateTime;
            Format4Delegates[typeof(byte)] = FormatByte;
            Format4Delegates[typeof(DateTimeOffset)] = FormatDateTimeOffset;
            Format4Delegates[typeof(double)] = FormatDouble;
            Format4Delegates[typeof(Guid)] = FormatGuid;

#if NET5_0_OR_GREATER
            Format4Delegates[typeof(Half)] = FormatHalf;
#endif

            Format4Delegates[typeof(short)] = FormatInt16;
            Format4Delegates[typeof(int)] = FormatInt32;
            Format4Delegates[typeof(long)] = FormatInt64;
            Format4Delegates[typeof(sbyte)] = FormatSByte;
            Format4Delegates[typeof(float)] = FormatSingle;
            Format4Delegates[typeof(TimeSpan)] = FormatTimeSpan;
            Format4Delegates[typeof(ushort)] = FormatUInt16;
            Format4Delegates[typeof(uint)] = FormatUInt32;
            Format4Delegates[typeof(ulong)] = FormatUInt64;
            Format4Delegates[typeof(nint)] = FormatIntPtr;
            Format4Delegates[typeof(nuint)] = FormatUIntPtr;
            Format4Delegates[typeof(Version)] = FormatVersion;

            Format4Delegates[typeof(ReadOnlyMemory<char>)] = FormatNullableReadOnlyMemoryChar;
            Format4Delegates[typeof(Memory<char>)] = FormatNullableMemoryChar;

            Format4Delegates[typeof(bool?)] = FormatNullableBoolean;
            Format4Delegates[typeof(decimal?)] = FormatNullableDecimal;
            Format4Delegates[typeof(DateTime?)] = FormatNullableDateTime;
            Format4Delegates[typeof(byte?)] = FormatNullableByte;
            Format4Delegates[typeof(DateTimeOffset?)] = FormatNullableDateTimeOffset;
            Format4Delegates[typeof(double?)] = FormatNullableDouble;
            Format4Delegates[typeof(Guid?)] = FormatNullableGuid;

#if NET5_0_OR_GREATER
            Format4Delegates[typeof(Half?)] = FormatNullableHalf;
#endif

            Format4Delegates[typeof(short?)] = FormatNullableInt16;
            Format4Delegates[typeof(int?)] = FormatNullableInt32;
            Format4Delegates[typeof(long?)] = FormatNullableInt64;
            Format4Delegates[typeof(sbyte?)] = FormatNullableSByte;
            Format4Delegates[typeof(float?)] = FormatNullableSingle;
            Format4Delegates[typeof(TimeSpan?)] = FormatNullableTimeSpan;
            Format4Delegates[typeof(ushort?)] = FormatNullableUInt16;
            Format4Delegates[typeof(uint?)] = FormatNullableUInt32;
            Format4Delegates[typeof(ulong?)] = FormatNullableUInt64;
            Format4Delegates[typeof(nint?)] = FormatNullableIntPtr;
            Format4Delegates[typeof(nuint?)] = FormatNullableUIntPtr;
        }

        /// <summary>
        ///     Gets whether the provider provides a custom formatter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasCustomFormatter(IFormatProvider provider) => provider.GetType() != typeof(CultureInfo) && provider.GetFormat(typeof(ICustomFormatter)) != null;

        /// <summary>
        ///     Format
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

            if (typeof(T) == typeof(ReadOnlyMemory<char>))
            {
                var obj = Unsafe.As<T?, ReadOnlyMemory<char>>(ref value);
                return TryCopyTo(obj.Span, destination, out charsWritten);
            }

            if (typeof(T) == typeof(Memory<char>))
            {
                var obj = Unsafe.As<T?, Memory<char>>(ref value);
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
                return sizeof(nint) == 8 ? Unsafe.As<T, long>(ref value!).TryFormat(destination, out charsWritten, format, provider) : Unsafe.As<T, int>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(nuint))
                return sizeof(nint) == 8 ? Unsafe.As<T, ulong>(ref value!).TryFormat(destination, out charsWritten, format, provider) : Unsafe.As<T, uint>(ref value!).TryFormat(destination, out charsWritten, format, provider);

            if (typeof(T) == typeof(Version))
            {
                var obj = Unsafe.As<T?, Version?>(ref value);
                if (obj != null)
                    return obj.TryFormat(destination, out charsWritten);
                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(ReadOnlyMemory<char>?))
            {
                var nullable = Unsafe.As<T?, ReadOnlyMemory<char>?>(ref value);
                if (nullable == null)
                {
                    charsWritten = 0;
                    return true;
                }

                return TryCopyTo(nullable.Value.Span, destination, out charsWritten);
            }

            if (typeof(T) == typeof(Memory<char>?))
            {
                var nullable = Unsafe.As<T?, Memory<char>?>(ref value);
                if (nullable == null)
                {
                    charsWritten = 0;
                    return true;
                }

                return TryCopyTo(nullable.Value.Span, destination, out charsWritten);
            }

            if (typeof(T) == typeof(bool?))
            {
                var nullable = Unsafe.As<T?, bool?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(decimal?))
            {
                var nullable = Unsafe.As<T?, decimal?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(DateTime?))
            {
                var nullable = Unsafe.As<T?, DateTime?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(byte?))
            {
                var nullable = Unsafe.As<T?, byte?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(DateTimeOffset?))
            {
                var nullable = Unsafe.As<T?, DateTimeOffset?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(double?))
            {
                var nullable = Unsafe.As<T?, double?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(Guid?))
            {
                var nullable = Unsafe.As<T?, Guid?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format);

                charsWritten = 0;
                return true;
            }

#if NET5_0_OR_GREATER
            if (typeof(T) == typeof(Half?))
            {
                var nullable = Unsafe.As<T?, Half?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }
#endif

            if (typeof(T) == typeof(short?))
            {
                var nullable = Unsafe.As<T?, short?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(int?))
            {
                var nullable = Unsafe.As<T?, int?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(long?))
            {
                var nullable = Unsafe.As<T?, long?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(sbyte?))
            {
                var nullable = Unsafe.As<T?, sbyte?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(float?))
            {
                var nullable = Unsafe.As<T?, float?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(TimeSpan?))
            {
                var nullable = Unsafe.As<T?, TimeSpan?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(ushort?))
            {
                var nullable = Unsafe.As<T?, ushort?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(uint?))
            {
                var nullable = Unsafe.As<T?, uint?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(ulong?))
            {
                var nullable = Unsafe.As<T?, ulong?>(ref value);
                if (nullable != null)
                    return nullable.Value.TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(nint?))
            {
                var nullable = Unsafe.As<T?, nint?>(ref value);
                if (nullable != null)
                    return sizeof(nint) == 8 ? ((long)nullable.Value).TryFormat(destination, out charsWritten, format, provider) : ((int)nullable.Value).TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(nuint?))
            {
                var nullable = Unsafe.As<T?, nuint?>(ref value);
                if (nullable != null)
                    return sizeof(nint) == 8 ? ((ulong)nullable.Value).TryFormat(destination, out charsWritten, format, provider) : ((uint)nullable.Value).TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T).IsEnum)
            {
                var obj = value!.ToString()!;
                return TryCopyTo(obj.AsSpan(), destination, out charsWritten);
            }

            if (value == null)
            {
                charsWritten = 0;
                return true;
            }

            if (typeof(T).IsArray)
            {
                var obj = typeof(T).ToString();
                return TryCopyTo(obj.AsSpan(), destination, out charsWritten);
            }

            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(typeof(T))!;
                var buffer = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref value!), Unsafe.SizeOf<T>());
                return TryFormatReadOnlySpanByteFallback(underlyingType, buffer, destination, out charsWritten, format, provider);
            }

            if (!typeof(T).IsValueType)
            {
                var type = value.GetType();

                if (typeof(T) == typeof(object) && type.IsEnum)
                {
                    var obj = value.ToString()!;
                    return TryCopyTo(obj.AsSpan(), destination, out charsWritten);
                }

                if (type == typeof(object))
                    return FormatObject(destination, out charsWritten, format, provider);

                if (type != typeof(T))
                    return TryFormatFallback(type, value, destination, out charsWritten, format, provider);
            }

            return FormatHelpers<T>.TryFormat(value, destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static void RegisterFormatReadOnlySpanByteFallback<T>() where T : struct
        {
            ReadOnlySpanByteFormat4Delegates[typeof(T)] = ReadOnlySpanByteFormat4Delegate;

            static bool ReadOnlySpanByteFormat4Delegate(ReadOnlySpan<byte> buffer, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => TryFormatReadOnlySpanByteFallback<T>(buffer, destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool TryFormatReadOnlySpanByteFallback<T>(ReadOnlySpan<byte> buffer, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) where T : struct
        {
            var obj = Unsafe.ReadUnaligned<T?>(ref MemoryMarshal.GetReference(buffer));
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return TryFormat(obj.Value, destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool TryFormatReadOnlySpanByteFallback(Type type, ReadOnlySpan<byte> buffer, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            if (ReadOnlySpanByteFormat4Delegates.TryGetValue(type, out var format4))
                return format4(buffer, destination, out charsWritten, format, provider);

            RegisterFormatReadOnlySpanByteFallbackMethodInfo.MakeGenericMethod(type).Invoke(null, null);
            return ReadOnlySpanByteFormat4Delegates[type](buffer, destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool TryFormatFallback<T>(Type type, T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            if (Format4Delegates.TryGetValue(type, out var format4))
                return format4(value!, destination, out charsWritten, format, provider);

            var method = typeof(FormatHelpers<>).MakeGenericType(type).GetMethod(nameof(FormatHelpers<T>.Initialize), BindingFlags.Public | BindingFlags.Static);
            method!.Invoke(null, null);
            return Format4Delegates[type](value!, destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Unbox
        /// </summary>
        private static T Unbox<T>(object obj) where T : struct => Unsafe.Unbox<T>(obj);

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatObject(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => TryCopyTo("System.Object".AsSpan(), destination, out charsWritten);

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatString(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.As<object, string>(ref value);
            return TryCopyTo(obj.AsSpan(), destination, out charsWritten);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatReadOnlyMemoryChar(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.As<object, ReadOnlyMemory<char>>(ref value);
            return TryCopyTo(obj.Span, destination, out charsWritten);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatMemoryChar(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.As<object, Memory<char>>(ref value);
            return TryCopyTo(obj.Span, destination, out charsWritten);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatBoolean(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<bool>(value);
            return obj.TryFormat(destination, out charsWritten);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatDecimal(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<decimal>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatDateTime(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<DateTime>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatByte(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<byte>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatDateTimeOffset(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<DateTimeOffset>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatDouble(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<double>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatGuid(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<Guid>(value);
            return obj.TryFormat(destination, out charsWritten, format);
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatHalf(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<Half>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }
#endif

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatInt16(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<short>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatInt32(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<int>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatInt64(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<long>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatSByte(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<sbyte>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatSingle(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<float>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatTimeSpan(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<TimeSpan>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatUInt16(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<ushort>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatUInt32(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<uint>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatUInt64(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<ulong>(value);
            return obj.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatIntPtr(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<nint>(value);
            return sizeof(nint) == 8 ? ((long)obj).TryFormat(destination, out charsWritten, format, provider) : ((int)obj).TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatUIntPtr(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<nuint>(value);
            return sizeof(nint) == 8 ? ((ulong)obj).TryFormat(destination, out charsWritten, format, provider) : ((uint)obj).TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatVersion(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (Version)value;
            return obj.TryFormat(destination, out charsWritten);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableReadOnlyMemoryChar(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (ReadOnlyMemory<char>?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return TryCopyTo(obj.Value.Span, destination, out charsWritten);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableMemoryChar(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (Memory<char>?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return TryCopyTo(obj.Value.Span, destination, out charsWritten);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableBoolean(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (bool?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableDecimal(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (decimal?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableDateTime(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (DateTime?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableByte(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (byte?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableDateTimeOffset(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (DateTimeOffset?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableDouble(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (double?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableGuid(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (Guid?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format);
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableHalf(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (Half?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }
#endif

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableInt16(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (short?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableInt32(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (int?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableInt64(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (long?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableSByte(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (sbyte?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableSingle(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (float?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableTimeSpan(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (TimeSpan?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableUInt16(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (ushort?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableUInt32(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (uint?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableUInt64(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (ulong?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return obj.Value.TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableIntPtr(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (nint?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return sizeof(nint) == 8 ? ((long)obj.Value).TryFormat(destination, out charsWritten, format, provider) : ((int)obj.Value).TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatNullableUIntPtr(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (nuint?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return sizeof(nint) == 8 ? ((ulong)obj.Value).TryFormat(destination, out charsWritten, format, provider) : ((uint)obj.Value).TryFormat(destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Try copy to
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryCopyTo(ReadOnlySpan<char> source, Span<char> destination, out int charsWritten)
        {
            if (source.TryCopyTo(destination))
            {
                charsWritten = source.Length;
                return true;
            }

            charsWritten = 0;
            return false;
        }
    }
}