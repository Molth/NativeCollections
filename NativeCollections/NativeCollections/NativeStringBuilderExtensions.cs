using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native string builder extensions
    /// </summary>
    public static class NativeStringBuilderExtensions
    {
        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(ref this NativeStringBuilder<char> builder)
        {
            var newLine = NativeString.NewLine;
            builder.EnsureCapacity(builder.Length + newLine.Length);
            ref var reference = ref MemoryMarshal.GetReference(builder.Buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, builder.Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * sizeof(char)));
            builder.Advance(newLine.Length);
        }

        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(ref this NativeStringBuilder<char> builder, ReadOnlySpan<char> buffer)
        {
            var newLine = NativeString.NewLine;
            builder.EnsureCapacity(builder.Length + buffer.Length + newLine.Length);
            ref var reference = ref MemoryMarshal.GetReference(builder.Buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, builder.Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * sizeof(char)));
            builder.Advance(buffer.Length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, builder.Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * sizeof(char)));
            builder.Advance(newLine.Length);
        }

        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(ref this NativeStringBuilder<char> builder, char value)
        {
            var newLine = NativeString.NewLine;
            builder.EnsureCapacity(builder.Length + 1 + newLine.Length);
            builder.Buffer[builder.Length] = value;
            builder.Advance(1);
            ref var reference = ref MemoryMarshal.GetReference(builder.Buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, builder.Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * sizeof(char)));
            builder.Advance(newLine.Length);
        }

        /// <summary>
        ///     Trim start
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TrimStart(ref this NativeStringBuilder<char> builder)
        {
            if (builder.Length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(builder.Buffer);
            var start = 0;
            while (start < builder.Length && char.IsWhiteSpace(Unsafe.Add(ref reference, start)))
                start++;
            if (start > 0 && start < builder.Length)
            {
                var count = builder.Length - start;
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, start)), (uint)(count * sizeof(char)));
                builder.SetLength(count);
            }
            else if (start >= builder.Length)
            {
                builder.SetLength(0);
            }
        }

        /// <summary>
        ///     Trim end
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TrimEnd(ref this NativeStringBuilder<char> builder)
        {
            if (builder.Length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(builder.Buffer);
            var end = builder.Length - 1;
            while (end >= 0 && char.IsWhiteSpace(Unsafe.Add(ref reference, end)))
                end--;
            builder.SetLength(end + 1);
        }

        /// <summary>
        ///     Trim
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Trim(ref this NativeStringBuilder<char> builder)
        {
            if (builder.Length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(builder.Buffer);
            var start = 0;
            var end = builder.Length - 1;
            while (start <= end && char.IsWhiteSpace(Unsafe.Add(ref reference, start)))
                start++;
            while (end >= start && char.IsWhiteSpace(Unsafe.Add(ref reference, end)))
                end--;
            var newLength = end - start + 1;
            if (newLength <= 0)
            {
                builder.SetLength(0);
                return;
            }

            if (start > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, start)), (uint)(newLength * sizeof(char)));
            builder.SetLength(newLength);
        }

        /// <summary>
        ///     Pad left
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PadLeft(ref this NativeStringBuilder<char> builder, int totalWidth) => builder.PadLeft(totalWidth, ' ');

        /// <summary>
        ///     Pad right
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PadRight(ref this NativeStringBuilder<char> builder, int totalWidth) => builder.PadRight(totalWidth, ' ');

        /// <summary>
        ///     Is null or white space
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace(ref this NativeStringBuilder<char> builder) => ((NativeString)builder.Text).IsNullOrWhiteSpace();

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, bool obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormatted(ref this NativeStringBuilder<char> builder, ref DefaultInterpolatedStringHandler message, bool clear = true) => DefaultInterpolatedStringHandlerHelpers.AppendFormatted(ref builder, ref message, clear);

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable<T>(ref this NativeStringBuilder<char> builder, in T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }
#else
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, decimal obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, DateTime obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, byte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, DateTimeOffset obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, double obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, Guid obj, ReadOnlySpan<char> format = default, IFormatProvider? _ = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, Half obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }
#endif

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, short obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, int obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, long obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, sbyte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, float obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, TimeSpan obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, ushort obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, uint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, ulong obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AppendFormattable(ref this NativeStringBuilder<char> builder, nint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (sizeof(nint) == 8)
                AppendFormattable(ref builder, (long)obj, format, provider);
            else
                AppendFormattable(ref builder, (int)obj, format, provider);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AppendFormattable(ref this NativeStringBuilder<char> builder, nuint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (sizeof(nint) == 8)
                AppendFormattable(ref builder, (ulong)obj, format, provider);
            else
                AppendFormattable(ref builder, (uint)obj, format, provider);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> builder, Version obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            int charsWritten;
            while (!obj.TryFormat(builder.Space, out charsWritten))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(charsWritten);
        }
#endif

#if NET8_0_OR_GREATER
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable<T>(ref this NativeStringBuilder<byte> builder, in T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : IUtf8SpanFormattable
        {
            int bytesWritten;
            while (!obj.TryFormat(builder.Space, out bytesWritten, format, provider))
                builder.EnsureCapacity(builder.Capacity + 1);
            builder.Advance(bytesWritten);
        }
#endif
    }
}