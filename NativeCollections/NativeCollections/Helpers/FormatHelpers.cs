using System;
using System.Collections.Concurrent;
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
        ///     Format
        /// </summary>
        public static readonly ConcurrentDictionary<Type, Handler> Format4Delegates = new();

        /// <summary>
        ///     Structure
        /// </summary>
        static FormatHelpers()
        {
            Format4Delegates[typeof(string)] = new Handler(&FormatString);
            Format4Delegates[typeof(ArraySegment<char>)] = new Handler(&FormatArraySegmentChar);
            Format4Delegates[typeof(ReadOnlyMemory<char>)] = new Handler(&FormatReadOnlyMemoryChar);
            Format4Delegates[typeof(Memory<char>)] = new Handler(&FormatMemoryChar);

            Format4Delegates[typeof(bool)] = new Handler(&FormatBoolean);
            Format4Delegates[typeof(decimal)] = new Handler(&FormatDecimal);
            Format4Delegates[typeof(DateTime)] = new Handler(&FormatDateTime);
            Format4Delegates[typeof(byte)] = new Handler(&FormatByte);
            Format4Delegates[typeof(DateTimeOffset)] = new Handler(&FormatDateTimeOffset);
            Format4Delegates[typeof(double)] = new Handler(&FormatDouble);
            Format4Delegates[typeof(Guid)] = new Handler(&FormatGuid);

#if NET5_0_OR_GREATER
            Format4Delegates[typeof(Half)] = new Handler(&FormatHalf);
#endif

            Format4Delegates[typeof(short)] = new Handler(&FormatInt16);
            Format4Delegates[typeof(int)] = new Handler(&FormatInt32);
            Format4Delegates[typeof(long)] = new Handler(&FormatInt64);
            Format4Delegates[typeof(sbyte)] = new Handler(&FormatSByte);
            Format4Delegates[typeof(float)] = new Handler(&FormatSingle);
            Format4Delegates[typeof(TimeSpan)] = new Handler(&FormatTimeSpan);
            Format4Delegates[typeof(ushort)] = new Handler(&FormatUInt16);
            Format4Delegates[typeof(uint)] = new Handler(&FormatUInt32);
            Format4Delegates[typeof(ulong)] = new Handler(&FormatUInt64);
            Format4Delegates[typeof(nint)] = new Handler(&FormatIntPtr);
            Format4Delegates[typeof(nuint)] = new Handler(&FormatUIntPtr);
            Format4Delegates[typeof(Version)] = new Handler(&FormatVersion);

            Format4Delegates[typeof(ArraySegment<char>?)] = new Handler(&FormatNullableArraySegmentChar);
            Format4Delegates[typeof(ReadOnlyMemory<char>?)] = new Handler(&FormatNullableReadOnlyMemoryChar);
            Format4Delegates[typeof(Memory<char>?)] = new Handler(&FormatNullableMemoryChar);

            Format4Delegates[typeof(bool?)] = new Handler(&FormatNullableBoolean);
            Format4Delegates[typeof(decimal?)] = new Handler(&FormatNullableDecimal);
            Format4Delegates[typeof(DateTime?)] = new Handler(&FormatNullableDateTime);
            Format4Delegates[typeof(byte?)] = new Handler(&FormatNullableByte);
            Format4Delegates[typeof(DateTimeOffset?)] = new Handler(&FormatNullableDateTimeOffset);
            Format4Delegates[typeof(double?)] = new Handler(&FormatNullableDouble);
            Format4Delegates[typeof(Guid?)] = new Handler(&FormatNullableGuid);

#if NET5_0_OR_GREATER
            Format4Delegates[typeof(Half?)] = new Handler(&FormatNullableHalf);
#endif

            Format4Delegates[typeof(short?)] = new Handler(&FormatNullableInt16);
            Format4Delegates[typeof(int?)] = new Handler(&FormatNullableInt32);
            Format4Delegates[typeof(long?)] = new Handler(&FormatNullableInt64);
            Format4Delegates[typeof(sbyte?)] = new Handler(&FormatNullableSByte);
            Format4Delegates[typeof(float?)] = new Handler(&FormatNullableSingle);
            Format4Delegates[typeof(TimeSpan?)] = new Handler(&FormatNullableTimeSpan);
            Format4Delegates[typeof(ushort?)] = new Handler(&FormatNullableUInt16);
            Format4Delegates[typeof(uint?)] = new Handler(&FormatNullableUInt32);
            Format4Delegates[typeof(ulong?)] = new Handler(&FormatNullableUInt64);
            Format4Delegates[typeof(nint?)] = new Handler(&FormatNullableIntPtr);
            Format4Delegates[typeof(nuint?)] = new Handler(&FormatNullableUIntPtr);
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

            if (typeof(T) == typeof(ArraySegment<char>?))
            {
                var nullable = Unsafe.As<T?, ArraySegment<char>?>(ref value);
                if (nullable == null)
                {
                    charsWritten = 0;
                    return true;
                }

                return TryCopyTo(nullable.GetValueOrDefault().AsSpan(), destination, out charsWritten);
            }

            if (typeof(T) == typeof(ReadOnlyMemory<char>?))
            {
                var nullable = Unsafe.As<T?, ReadOnlyMemory<char>?>(ref value);
                if (nullable == null)
                {
                    charsWritten = 0;
                    return true;
                }

                return TryCopyTo(nullable.GetValueOrDefault().Span, destination, out charsWritten);
            }

            if (typeof(T) == typeof(Memory<char>?))
            {
                var nullable = Unsafe.As<T?, Memory<char>?>(ref value);
                if (nullable == null)
                {
                    charsWritten = 0;
                    return true;
                }

                return TryCopyTo(nullable.GetValueOrDefault().Span, destination, out charsWritten);
            }

            if (typeof(T) == typeof(bool?))
            {
                var nullable = Unsafe.As<T?, bool?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(decimal?))
            {
                var nullable = Unsafe.As<T?, decimal?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(DateTime?))
            {
                var nullable = Unsafe.As<T?, DateTime?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(byte?))
            {
                var nullable = Unsafe.As<T?, byte?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(DateTimeOffset?))
            {
                var nullable = Unsafe.As<T?, DateTimeOffset?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(double?))
            {
                var nullable = Unsafe.As<T?, double?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(Guid?))
            {
                var nullable = Unsafe.As<T?, Guid?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format);

                charsWritten = 0;
                return true;
            }

#if NET5_0_OR_GREATER
            if (typeof(T) == typeof(Half?))
            {
                var nullable = Unsafe.As<T?, Half?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }
#endif

            if (typeof(T) == typeof(short?))
            {
                var nullable = Unsafe.As<T?, short?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(int?))
            {
                var nullable = Unsafe.As<T?, int?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(long?))
            {
                var nullable = Unsafe.As<T?, long?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(sbyte?))
            {
                var nullable = Unsafe.As<T?, sbyte?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(float?))
            {
                var nullable = Unsafe.As<T?, float?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(TimeSpan?))
            {
                var nullable = Unsafe.As<T?, TimeSpan?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(ushort?))
            {
                var nullable = Unsafe.As<T?, ushort?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(uint?))
            {
                var nullable = Unsafe.As<T?, uint?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(ulong?))
            {
                var nullable = Unsafe.As<T?, ulong?>(ref value);
                if (nullable != null)
                    return nullable.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(nint?))
            {
                var nullable = Unsafe.As<T?, nint?>(ref value);
                if (nullable != null)
                    return sizeof(nint) == 8 ? ((long)nullable.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider) : ((int)nullable.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider);

                charsWritten = 0;
                return true;
            }

            if (typeof(T) == typeof(nuint?))
            {
                var nullable = Unsafe.As<T?, nuint?>(ref value);
                if (nullable != null)
                    return sizeof(nint) == 8 ? ((ulong)nullable.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider) : ((uint)nullable.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider);

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

            if (typeof(T).IsValueType && default(T) == null)
                return TryFormatFallback(value, destination, out charsWritten, format, provider);

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
                {
                    if (Format4Delegates.TryGetValue(type, out var format4))
                        return format4.TryFormat(value, destination, out charsWritten, format, provider);

                    return TryFormatFallback(value, destination, out charsWritten, format, provider);
                }
            }

            return FormatHelpers<T>.TryFormat(value, destination, out charsWritten, format, provider);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool TryFormatFallback<T>(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
#if NET6_0_OR_GREATER
            if (value is ISpanFormattable spanFormattable)
                return spanFormattable.TryFormat(destination, out charsWritten, format, provider);
#endif
            var result = value is IFormattable formattable ? formattable.ToString(format.ToString(), provider) : value!.ToString();
            var obj = (result != null ? result : "").AsSpan();
            return TryCopyTo(obj, destination, out charsWritten);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatObject(Span<char> destination, out int charsWritten, ReadOnlySpan<char> _, IFormatProvider? __) => TryCopyTo("System.Object".AsSpan(), destination, out charsWritten);

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
        private static bool FormatArraySegmentChar(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<ArraySegment<char>>(value);
            return TryCopyTo(obj.AsSpan(), destination, out charsWritten);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatReadOnlyMemoryChar(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<ReadOnlyMemory<char>>(value);
            return TryCopyTo(obj.Span, destination, out charsWritten);
        }

        /// <summary>
        ///     Format
        /// </summary>
        private static bool FormatMemoryChar(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = Unsafe.Unbox<Memory<char>>(value);
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
        private static bool FormatNullableArraySegmentChar(object value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var obj = (ArraySegment<char>?)value;
            if (obj == null)
            {
                charsWritten = 0;
                return true;
            }

            return TryCopyTo(obj.GetValueOrDefault().AsSpan(), destination, out charsWritten);
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

            return TryCopyTo(obj.GetValueOrDefault().Span, destination, out charsWritten);
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

            return TryCopyTo(obj.GetValueOrDefault().Span, destination, out charsWritten);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return obj.GetValueOrDefault().TryFormat(destination, out charsWritten, format, provider);
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

            return sizeof(nint) == 8 ? ((long)obj.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider) : ((int)obj.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider);
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

            return sizeof(nint) == 8 ? ((ulong)obj.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider) : ((uint)obj.GetValueOrDefault()).TryFormat(destination, out charsWritten, format, provider);
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