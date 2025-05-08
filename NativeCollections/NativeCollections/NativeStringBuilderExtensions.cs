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
        /// <param name="nativeString">Native string</param>
        /// <returns>Appended</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(ref this NativeStringBuilder<char> nativeString)
        {
            var newLine = NativeString.NewLine;
            nativeString.EnsureCapacity(nativeString.Length + newLine.Length);
            ref var reference = ref MemoryMarshal.GetReference(nativeString.Buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, nativeString.Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * sizeof(char)));
            nativeString.Advance(newLine.Length);
        }

        /// <summary>
        ///     Append line
        /// </summary>
        /// <param name="nativeString">Native string</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Appended</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(ref this NativeStringBuilder<char> nativeString, ReadOnlySpan<char> buffer)
        {
            var newLine = NativeString.NewLine;
            nativeString.EnsureCapacity(nativeString.Length + buffer.Length + newLine.Length);
            ref var reference = ref MemoryMarshal.GetReference(nativeString.Buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, nativeString.Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * sizeof(char)));
            nativeString.Advance(buffer.Length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, nativeString.Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * sizeof(char)));
            nativeString.Advance(newLine.Length);
        }

        /// <summary>
        ///     Append line
        /// </summary>
        /// <param name="nativeString">Native string</param>
        /// <param name="value">Value</param>
        /// <returns>Appended</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(ref this NativeStringBuilder<char> nativeString, char value)
        {
            var newLine = NativeString.NewLine;
            nativeString.EnsureCapacity(nativeString.Length + 1 + newLine.Length);
            nativeString.Buffer[nativeString.Length] = value;
            nativeString.Advance(1);
            ref var reference = ref MemoryMarshal.GetReference(nativeString.Buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, nativeString.Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * sizeof(char)));
            nativeString.Advance(newLine.Length);
        }

        /// <summary>
        ///     Trim start
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TrimStart(ref this NativeStringBuilder<char> nativeString)
        {
            if (nativeString.Length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(nativeString.Buffer);
            var start = 0;
            while (start < nativeString.Length && char.IsWhiteSpace(Unsafe.Add(ref reference, start)))
                start++;
            if (start > 0 && start < nativeString.Length)
            {
                var count = nativeString.Length - start;
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, start)), (uint)(count * sizeof(char)));
                nativeString.SetLength(count);
            }
            else if (start >= nativeString.Length)
            {
                nativeString.SetLength(0);
            }
        }

        /// <summary>
        ///     Trim end
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TrimEnd(ref this NativeStringBuilder<char> nativeString)
        {
            if (nativeString.Length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(nativeString.Buffer);
            var end = nativeString.Length - 1;
            while (end >= 0 && char.IsWhiteSpace(Unsafe.Add(ref reference, end)))
                end--;
            nativeString.SetLength(end + 1);
        }

        /// <summary>
        ///     Trim
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Trim(ref this NativeStringBuilder<char> nativeString)
        {
            if (nativeString.Length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(nativeString.Buffer);
            var start = 0;
            var end = nativeString.Length - 1;
            while (start <= end && char.IsWhiteSpace(Unsafe.Add(ref reference, start)))
                start++;
            while (end >= start && char.IsWhiteSpace(Unsafe.Add(ref reference, end)))
                end--;
            var newLength = end - start + 1;
            if (newLength <= 0)
            {
                nativeString.SetLength(0);
                return;
            }

            if (start > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, start)), (uint)(newLength * sizeof(char)));
            nativeString.SetLength(newLength);
        }

        /// <summary>
        ///     Pad left
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PadLeft(ref this NativeStringBuilder<char> nativeString, int totalWidth) => nativeString.PadLeft(totalWidth, ' ');

        /// <summary>
        ///     Pad right
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PadRight(ref this NativeStringBuilder<char> nativeString, int totalWidth) => nativeString.PadRight(totalWidth, ' ');

        /// <summary>
        ///     Is null or white space
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace(ref this NativeStringBuilder<char> nativeString) => ((NativeString)nativeString.Text).IsNullOrWhiteSpace();

        /// <summary>
        ///     Is null or empty
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(ref this NativeStringBuilder<char> nativeString) => ((NativeString)nativeString.Text).IsNullOrEmpty();

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, bool obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable<T>(ref this NativeStringBuilder<char> nativeString, in T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }
#else
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, decimal obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, DateTime obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, byte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, DateTimeOffset obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, double obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, Guid obj, ReadOnlySpan<char> format = default, IFormatProvider? _ = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, Half obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }
#endif

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, short obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, int obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, long obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, sbyte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, float obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, TimeSpan obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, ushort obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, uint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, ulong obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AppendFormattable(ref this NativeStringBuilder<char> nativeString, nint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (sizeof(nint) == 8)
                AppendFormattable(ref nativeString, (long)obj, format, provider);
            else
                AppendFormattable(ref nativeString, (int)obj, format, provider);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AppendFormattable(ref this NativeStringBuilder<char> nativeString, nuint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (sizeof(nint) == 8)
                AppendFormattable(ref nativeString, (ulong)obj, format, provider);
            else
                AppendFormattable(ref nativeString, (uint)obj, format, provider);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(ref this NativeStringBuilder<char> nativeString, Version obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            int charsWritten;
            while (!obj.TryFormat(nativeString.Space, out charsWritten))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(charsWritten);
        }
#endif

#if NET8_0_OR_GREATER
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable<T>(ref this NativeStringBuilder<byte> nativeString, in T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : IUtf8SpanFormattable
        {
            int bytesWritten;
            while (!obj.TryFormat(nativeString.Space, out bytesWritten, format, provider))
                nativeString.EnsureCapacity(nativeString.Capacity + 1);
            nativeString.Advance(bytesWritten);
        }
#endif
    }
}