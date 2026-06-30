using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe string builder extensions
    /// </summary>
    public static partial class UnsafeStringBuilderExtensions
    {
        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(in this UnsafeStringBuilder<char> builder)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLine;
            builderRef.EnsureCapacity(builderRef.Length + newLine.Length);
            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)builderRef.Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * Unsafe.SizeOf<char>()));
            builderRef.Advance(newLine.Length);
        }

        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(in this UnsafeStringBuilder<char> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLine;
            builderRef.EnsureCapacity(builderRef.Length + buffer.Length + newLine.Length);
            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)builderRef.Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * Unsafe.SizeOf<char>()));
            builderRef.Advance(buffer.Length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)builderRef.Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * Unsafe.SizeOf<char>()));
            builderRef.Advance(newLine.Length);
        }

        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(in this UnsafeStringBuilder<char> builder, char value)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLine;
            builderRef.EnsureCapacity(builderRef.Length + 1 + newLine.Length);
            builderRef.Buffer[builderRef.Length] = value;
            builderRef.Advance(1);
            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)builderRef.Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * Unsafe.SizeOf<char>()));
            builderRef.Advance(newLine.Length);
        }

        /// <summary>
        ///     Append join
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<char> builder, char separator, ReadOnlySpan<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            ref var reference = ref MemoryMarshal.GetReference(values);
            for (var i = 0; i < values.Length; ++i)
            {
                var value = Unsafe.Add(ref reference, (nint)i);
                if (i != 0)
                    builderRef.Append(separator);
                builderRef.AppendFormat(value);
            }
        }

        /// <summary>
        ///     Append join
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<char> builder, char separator, IEnumerable<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            var first = false;
            foreach (var value in values)
            {
                if (!first)
                    first = true;
                else
                    builderRef.Append(separator);
                builderRef.AppendFormat(value);
            }
        }

        /// <summary>
        ///     Append join
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<char> builder, ReadOnlySpan<char> separator, ReadOnlySpan<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            ref var reference = ref MemoryMarshal.GetReference(values);
            for (var i = 0; i < values.Length; ++i)
            {
                var value = Unsafe.Add(ref reference, (nint)i);
                if (i != 0)
                    builderRef.Append(separator);
                builderRef.AppendFormat(value);
            }
        }

        /// <summary>
        ///     Append join
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<char> builder, ReadOnlySpan<char> separator, IEnumerable<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            var first = false;
            foreach (var value in values)
            {
                if (!first)
                    first = true;
                else
                    builderRef.Append(separator);
                builderRef.AppendFormat(value);
            }
        }

        /// <summary>
        ///     Trim start
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TrimStart(in this UnsafeStringBuilder<char> builder)
        {
            ref var builderRef = ref builder.AsRef();
            if (builderRef.IsEmpty)
                return;
            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            var start = 0;
            while (start < builderRef.Length && char.IsWhiteSpace(Unsafe.Add(ref reference, (nint)start)))
                start++;
            if (start > 0 && start < builderRef.Length)
            {
                var count = builderRef.Length - start;
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(count * Unsafe.SizeOf<char>()));
                builderRef.SetLength(count);
            }
            else if (start >= builderRef.Length)
            {
                builderRef.SetLength(0);
            }
        }

        /// <summary>
        ///     Trim end
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TrimEnd(in this UnsafeStringBuilder<char> builder)
        {
            ref var builderRef = ref builder.AsRef();
            if (builderRef.IsEmpty)
                return;
            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            var end = builderRef.Length - 1;
            while (end >= 0 && char.IsWhiteSpace(Unsafe.Add(ref reference, (nint)end)))
                end--;
            builderRef.SetLength(end + 1);
        }

        /// <summary>
        ///     Trim
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Trim(in this UnsafeStringBuilder<char> builder)
        {
            ref var builderRef = ref builder.AsRef();
            if (builderRef.IsEmpty)
                return;
            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            var start = 0;
            var end = builderRef.Length - 1;
            while (start <= end && char.IsWhiteSpace(Unsafe.Add(ref reference, (nint)start)))
                start++;
            while (end >= start && char.IsWhiteSpace(Unsafe.Add(ref reference, (nint)end)))
                end--;
            var newLength = end - start + 1;
            if (newLength <= 0)
            {
                builderRef.SetLength(0);
                return;
            }

            if (start > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(newLength * Unsafe.SizeOf<char>()));
            builderRef.SetLength(newLength);
        }

        /// <summary>
        ///     Pad left
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PadLeft(in this UnsafeStringBuilder<char> builder, int totalWidth)
        {
            ref var builderRef = ref builder.AsRef();
            builderRef.PadLeft(totalWidth, ' ');
        }

        /// <summary>
        ///     Pad right
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PadRight(in this UnsafeStringBuilder<char> builder, int totalWidth)
        {
            ref var builderRef = ref builder.AsRef();
            builderRef.PadRight(totalWidth, ' ');
        }

        /// <summary>
        ///     Is null or white space
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace(in this UnsafeStringBuilder<char> builder)
        {
            ref var builderRef = ref builder.AsRef();
            return ((UnsafeString)builderRef.Text).IsNullOrWhiteSpace();
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T>(in this UnsafeStringBuilder<char> builder, T? obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : struct
        {
            if (!obj.HasValue)
                return;
            builder.AppendFormat(obj.GetValueOrDefault(), format, provider);
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T>(in this UnsafeStringBuilder<char> builder, T? obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!FormatHelpers.TryFormat(obj, builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, bool obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendInterpolated(in this UnsafeStringBuilder<char> builder, [InterpolatedStringHandlerArgument("builder")] ref UnsafeStringBuilderUtf16InterpolatedStringHandler handler)
        {
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendInterpolated(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [InterpolatedStringHandlerArgument("builder", "provider")] ref UnsafeStringBuilderUtf16InterpolatedStringHandler handler)
        {
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormatted(in this UnsafeStringBuilder<char> builder, ref DefaultInterpolatedStringHandler handler, bool clear = true)
        {
            ref var builderRef = ref builder.AsRef();
#if NET10_0_OR_GREATER
            builderRef.Append(handler.Text);
            if (clear)
                handler.Clear();
#else
            ReadOnlySpan<char> buffer = clear ? handler.ToStringAndClear() : handler.ToString();
            builderRef.Append(buffer);
#endif
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormatted(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [InterpolatedStringHandlerArgument("provider")] ref DefaultInterpolatedStringHandler handler, bool clear = true)
        {
            ref var builderRef = ref builder.AsRef();
#if NET10_0_OR_GREATER
            builderRef.Append(handler.Text);
            if (clear)
                handler.Clear();
#else
            ReadOnlySpan<char> buffer = clear ? handler.ToStringAndClear() : handler.ToString();
            builderRef.Append(buffer);
#endif
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable<T>(in this UnsafeStringBuilder<char> builder, in T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }
#else
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, decimal obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, DateTime obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, byte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, DateTimeOffset obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, double obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, Guid obj, ReadOnlySpan<char> format = default, IFormatProvider? _ = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, Half obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }
#endif

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, short obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, int obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, long obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, sbyte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, float obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, TimeSpan obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, ushort obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, uint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, ulong obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, nint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (Environment.Is64BitProcess)
                AppendFormattable(builder, (long)obj, format, provider);
            else
                AppendFormattable(builder, (int)obj, format, provider);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, nuint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (Environment.Is64BitProcess)
                AppendFormattable(builder, (ulong)obj, format, provider);
            else
                AppendFormattable(builder, (uint)obj, format, provider);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, Version obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(charsWritten);
        }
#endif

        /// <summary>
        ///     Index of
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            byte[]? array = null;
            var bytes = byteCount <= 1024 ? stackalloc byte[byteCount] : (array = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);
            try
            {
                Encoding.UTF8.GetBytes(buffer, bytes);
                return builderRef.IndexOf(bytes);
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Last index of
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            byte[]? array = null;
            var bytes = byteCount <= 1024 ? stackalloc byte[byteCount] : (array = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);
            try
            {
                Encoding.UTF8.GetBytes(buffer, bytes);
                return builderRef.LastIndexOf(bytes);
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Index of any
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            byte[]? array = null;
            var bytes = byteCount <= 1024 ? stackalloc byte[byteCount] : (array = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);
            try
            {
                Encoding.UTF8.GetBytes(buffer, bytes);
                return builderRef.IndexOfAny(bytes);
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Last index of any
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            byte[]? array = null;
            var bytes = byteCount <= 1024 ? stackalloc byte[byteCount] : (array = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);
            try
            {
                Encoding.UTF8.GetBytes(buffer, bytes);
                return builderRef.LastIndexOfAny(bytes);
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Contains
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            byte[]? array = null;
            var bytes = byteCount <= 1024 ? stackalloc byte[byteCount] : (array = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);
            try
            {
                Encoding.UTF8.GetBytes(buffer, bytes);
                return builderRef.Contains(bytes);
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Remove
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            byte[]? array = null;
            var bytes1 = byteCount <= 1024 ? stackalloc byte[byteCount] : (array = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);
            try
            {
                Encoding.UTF8.GetBytes(buffer, bytes1);
                var bytes2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(bytes1), bytes1.Length);
                builderRef.Remove(bytes2);
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Insert
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Insert(in this UnsafeStringBuilder<byte> builder, int startIndex, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            byte[]? array = null;
            var bytes1 = byteCount <= 1024 ? stackalloc byte[byteCount] : (array = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);
            try
            {
                Encoding.UTF8.GetBytes(buffer, bytes1);
                var bytes2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(bytes1), bytes1.Length);
                return builderRef.Insert(startIndex, bytes2);
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Replace
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Replace(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount1 = Encoding.UTF8.GetByteCount(oldValue);
            var byteCount2 = Encoding.UTF8.GetByteCount(newValue);
            var byteCount = byteCount1 + byteCount2;
            byte[]? array = null;
            var bytes1 = byteCount <= 1024 ? stackalloc byte[byteCount1] : (array = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount1);
            var bytes2 = byteCount <= 1024 ? stackalloc byte[byteCount2] : array.AsSpan(byteCount1, byteCount2);
            try
            {
                Encoding.UTF8.GetBytes(oldValue, bytes1);
                Encoding.UTF8.GetBytes(newValue, bytes2);
                var bytes3 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(bytes1), bytes1.Length);
                var bytes4 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(bytes2), bytes2.Length);
                return builderRef.Replace(bytes3, bytes4);
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Starts with
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            byte[]? array = null;
            var bytes = byteCount <= 1024 ? stackalloc byte[byteCount] : (array = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);
            try
            {
                Encoding.UTF8.GetBytes(buffer, bytes);
                return builderRef.StartsWith(bytes);
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Ends with
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            byte[]? array = null;
            var bytes = byteCount <= 1024 ? stackalloc byte[byteCount] : (array = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);
            try
            {
                Encoding.UTF8.GetBytes(buffer, bytes);
                return builderRef.EndsWith(bytes);
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Compare
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            byte[]? array = null;
            var bytes = byteCount <= 1024 ? stackalloc byte[byteCount] : (array = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);
            try
            {
                Encoding.UTF8.GetBytes(buffer, bytes);
                return builderRef.Compare(bytes);
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(in this UnsafeStringBuilder<byte> builder, char value, int repeatCount)
        {
            ref var builderRef = ref builder.AsRef();
            ReadOnlySpan<char> buffer = stackalloc char[1] { value };
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            builderRef.EnsureCapacity(builderRef.Length + byteCount * repeatCount);
            Span<byte> bytes = stackalloc byte[byteCount];
            Encoding.UTF8.GetBytes(buffer, bytes);
            ref var destination = ref MemoryMarshal.GetReference(builderRef.Space);
            ref var source = ref MemoryMarshal.GetReference(bytes);
            switch (byteCount)
            {
                case 1:
                    SpanHelpers.Fill(ref destination, (nuint)repeatCount, source);
                    break;
                case 2:
                    SpanHelpers.Fill(ref Unsafe.As<byte, Utf8FormatHelpers.DummyBytes2>(ref destination), (nuint)repeatCount, Unsafe.ReadUnaligned<Utf8FormatHelpers.DummyBytes2>(ref source));
                    break;
                case 3:
                    SpanHelpers.Fill(ref Unsafe.As<byte, Utf8FormatHelpers.DummyBytes3>(ref destination), (nuint)repeatCount, Unsafe.ReadUnaligned<Utf8FormatHelpers.DummyBytes3>(ref source));
                    break;
            }

            builderRef.Advance(byteCount * repeatCount);
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            builderRef.EnsureCapacity(builderRef.Length + byteCount);
            var bytes = builderRef.GetSpan(byteCount);
            Encoding.UTF8.GetBytes(buffer, bytes);
            builderRef.Advance(bytes.Length);
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(in this UnsafeStringBuilder<byte> builder, char value)
        {
            ReadOnlySpan<char> buffer = stackalloc char[1] { value };
            builder.Append(buffer);
        }

        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(in this UnsafeStringBuilder<byte> builder)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLineUtf8;
            builderRef.Append(newLine);
        }

        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLineUtf8;
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            builderRef.EnsureCapacity(builderRef.Length + byteCount + newLine.Length);
            var bytes = builderRef.GetSpan(byteCount);
            Encoding.UTF8.GetBytes(buffer, bytes);
            builderRef.Advance(bytes.Length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(builderRef.Buffer), new IntPtr(builderRef.Length)), ref MemoryMarshal.GetReference(newLine), (uint)newLine.Length);
            builderRef.Advance(newLine.Length);
        }

        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(in this UnsafeStringBuilder<byte> builder, char value)
        {
            ReadOnlySpan<char> buffer = stackalloc char[1] { value };
            builder.AppendLine(buffer);
        }

        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<byte> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLineUtf8;
            builderRef.EnsureCapacity(builderRef.Length + buffer.Length + newLine.Length);
            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref reference, new IntPtr(builderRef.Length)), ref MemoryMarshal.GetReference(buffer), (uint)buffer.Length);
            builderRef.Advance(buffer.Length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref reference, new IntPtr(builderRef.Length)), ref MemoryMarshal.GetReference(newLine), (uint)newLine.Length);
            builderRef.Advance(newLine.Length);
        }

        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(in this UnsafeStringBuilder<byte> builder, byte value)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLineUtf8;
            builderRef.EnsureCapacity(builderRef.Length + 1 + newLine.Length);
            builderRef.Buffer[builderRef.Length] = value;
            builderRef.Advance(1);
            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref reference, new IntPtr(builderRef.Length)), ref MemoryMarshal.GetReference(newLine), (uint)newLine.Length);
            builderRef.Advance(newLine.Length);
        }

        /// <summary>
        ///     Append join
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<byte> builder, char separator, ReadOnlySpan<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendJoin(separator, values);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append join
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<byte> builder, char separator, IEnumerable<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendJoin(separator, values);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append join
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> separator, ReadOnlySpan<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendJoin(separator, values);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append join
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> separator, IEnumerable<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendJoin(separator, values);
                builderRef.Append(temp);
            }
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(in this UnsafeStringBuilder<byte> builder, [InterpolatedStringHandlerArgument("builder")] ref UnsafeStringBuilderUtf8InterpolatedStringHandler handler)
        {
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [InterpolatedStringHandlerArgument("builder", "provider")] ref UnsafeStringBuilderUtf8InterpolatedStringHandler handler)
        {
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormatted(in this UnsafeStringBuilder<byte> builder, ref DefaultInterpolatedStringHandler handler, bool clear = true)
        {
            ref var builderRef = ref builder.AsRef();
#if NET10_0_OR_GREATER
            builderRef.Append(handler.Text);
            if (clear)
                handler.Clear();
#else
            ReadOnlySpan<char> buffer = clear ? handler.ToStringAndClear() : handler.ToString();
            builderRef.Append(buffer);
#endif
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormatted(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [InterpolatedStringHandlerArgument("provider")] ref DefaultInterpolatedStringHandler handler, bool clear = true)
        {
            ref var builderRef = ref builder.AsRef();
#if NET10_0_OR_GREATER
            builderRef.Append(handler.Text);
            if (clear)
                handler.Clear();
#else
            ReadOnlySpan<char> buffer = clear ? handler.ToStringAndClear() : handler.ToString();
            builderRef.Append(buffer);
#endif
        }
#endif

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T>(in this UnsafeStringBuilder<byte> builder, T? obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : struct
        {
            if (!obj.HasValue)
                return;
            builder.AppendFormat(obj.GetValueOrDefault(), format, provider);
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T>(in this UnsafeStringBuilder<byte> builder, T? obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int bytesWritten;
            while (!Utf8FormatHelpers.TryFormat(obj, builderRef.Space, out bytesWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(bytesWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, bool obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            ref var builderRef = ref builder.AsRef();
            Span<char> destination = stackalloc char[8];
            obj.TryFormat(destination, out var charsWritten);
            builderRef.Append(destination.Slice(0, charsWritten));
        }

#if NET8_0_OR_GREATER
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable<T>(in this UnsafeStringBuilder<byte> builder, in T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : IUtf8SpanFormattable
        {
            ref var builderRef = ref builder.AsRef();
            int bytesWritten;
            while (!obj.TryFormat(builderRef.Space, out bytesWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity + 1);
            builderRef.Advance(bytesWritten);
        }
#elif NET6_0_OR_GREATER
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable<T>(in this UnsafeStringBuilder<byte> builder, in T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }
#else
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, decimal obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, DateTime obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, byte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, DateTimeOffset obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, double obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, Guid obj, ReadOnlySpan<char> format = default, IFormatProvider? _ = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, _);
                builderRef.Append(temp);
            }
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, Half obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }
#endif

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, short obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, int obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, long obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, sbyte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, float obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, TimeSpan obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, ushort obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, uint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, ulong obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, nint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (Environment.Is64BitProcess)
                AppendFormattable(builder, (long)obj, format, provider);
            else
                AppendFormattable(builder, (int)obj, format, provider);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, nuint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (Environment.Is64BitProcess)
                AppendFormattable(builder, (ulong)obj, format, provider);
            else
                AppendFormattable(builder, (uint)obj, format, provider);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, Version obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, _, __);
                builderRef.Append(temp);
            }
        }
#endif
    }
}