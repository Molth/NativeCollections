using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8500
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native string builder extensions
    /// </summary>
    public static unsafe class NativeStringBuilderExtensions
    {
        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(in this NativeStringBuilder<char> builder)
        {
            var newLine = NativeString.NewLine;
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                ptr->EnsureCapacity(ptr->Length + newLine.Length);
                ref var reference = ref MemoryMarshal.GetReference(ptr->Buffer);
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, ptr->Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * sizeof(char)));
                ptr->Advance(newLine.Length);
            }
        }

        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(in this NativeStringBuilder<char> builder, ReadOnlySpan<char> buffer)
        {
            var newLine = NativeString.NewLine;
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                ptr->EnsureCapacity(ptr->Length + buffer.Length + newLine.Length);
                ref var reference = ref MemoryMarshal.GetReference(ptr->Buffer);
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, ptr->Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * sizeof(char)));
                ptr->Advance(buffer.Length);
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, ptr->Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * sizeof(char)));
                ptr->Advance(newLine.Length);
            }
        }

        /// <summary>
        ///     Append line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(in this NativeStringBuilder<char> builder, char value)
        {
            var newLine = NativeString.NewLine;
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                ptr->EnsureCapacity(ptr->Length + 1 + newLine.Length);
                ptr->Buffer[ptr->Length] = value;
                ptr->Advance(1);
                ref var reference = ref MemoryMarshal.GetReference(ptr->Buffer);
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, ptr->Length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * sizeof(char)));
                ptr->Advance(newLine.Length);
            }
        }

        /// <summary>
        ///     Trim start
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TrimStart(in this NativeStringBuilder<char> builder)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                if (ptr->Length == 0)
                    return;
                ref var reference = ref MemoryMarshal.GetReference(ptr->Buffer);
                var start = 0;
                while (start < ptr->Length && char.IsWhiteSpace(Unsafe.Add(ref reference, start)))
                    start++;
                if (start > 0 && start < ptr->Length)
                {
                    var count = ptr->Length - start;
                    Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, start)), (uint)(count * sizeof(char)));
                    ptr->SetLength(count);
                }
                else if (start >= ptr->Length)
                {
                    ptr->SetLength(0);
                }
            }
        }

        /// <summary>
        ///     Trim end
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TrimEnd(in this NativeStringBuilder<char> builder)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                if (ptr->Length == 0)
                    return;
                ref var reference = ref MemoryMarshal.GetReference(ptr->Buffer);
                var end = ptr->Length - 1;
                while (end >= 0 && char.IsWhiteSpace(Unsafe.Add(ref reference, end)))
                    end--;
                ptr->SetLength(end + 1);
            }
        }

        /// <summary>
        ///     Trim
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Trim(in this NativeStringBuilder<char> builder)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                if (ptr->Length == 0)
                    return;
                ref var reference = ref MemoryMarshal.GetReference(ptr->Buffer);
                var start = 0;
                var end = ptr->Length - 1;
                while (start <= end && char.IsWhiteSpace(Unsafe.Add(ref reference, start)))
                    start++;
                while (end >= start && char.IsWhiteSpace(Unsafe.Add(ref reference, end)))
                    end--;
                var newLength = end - start + 1;
                if (newLength <= 0)
                {
                    ptr->SetLength(0);
                    return;
                }

                if (start > 0)
                    Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, start)), (uint)(newLength * sizeof(char)));
                ptr->SetLength(newLength);
            }
        }

        /// <summary>
        ///     Pad left
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PadLeft(in this NativeStringBuilder<char> builder, int totalWidth)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                ptr->PadLeft(totalWidth, ' ');
            }
        }

        /// <summary>
        ///     Pad right
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PadRight(in this NativeStringBuilder<char> builder, int totalWidth)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                ptr->PadRight(totalWidth, ' ');
            }
        }

        /// <summary>
        ///     Is null or white space
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace(in this NativeStringBuilder<char> builder)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                return ((NativeString)ptr->Text).IsNullOrWhiteSpace();
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, bool obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }
#if NET6_0_OR_GREATER
        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormatted(in this NativeStringBuilder<char> builder, ref DefaultInterpolatedStringHandler message, bool clear = true)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                DefaultInterpolatedStringHandlerHelpers.AppendFormatted(ptr, ref message, clear);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable<T>(in this NativeStringBuilder<char> builder, in T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }
#else
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, decimal obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, DateTime obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, byte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, DateTimeOffset obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, double obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, Guid obj, ReadOnlySpan<char> format = default, IFormatProvider? _ = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, Half obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }
#endif

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, short obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, int obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, long obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, sbyte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, float obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, TimeSpan obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, ushort obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, uint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, ulong obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, nint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (sizeof(nint) == 8)
                AppendFormattable(builder, (long)obj, format, provider);
            else
                AppendFormattable(builder, (int)obj, format, provider);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, nuint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (sizeof(nint) == 8)
                AppendFormattable(builder, (ulong)obj, format, provider);
            else
                AppendFormattable(builder, (uint)obj, format, provider);
        }

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this NativeStringBuilder<char> builder, Version obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                int charsWritten;
                while (!obj.TryFormat(ptr->Space, out charsWritten))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(charsWritten);
            }
        }
#endif

#if NET8_0_OR_GREATER
        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable<T>(in this NativeStringBuilder<byte> builder, in T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : IUtf8SpanFormattable
        {
            fixed (NativeStringBuilder<byte>* ptr = &builder)
            {
                int bytesWritten;
                while (!obj.TryFormat(ptr->Space, out bytesWritten, format, provider))
                    ptr->EnsureCapacity(ptr->Capacity + 1);
                ptr->Advance(bytesWritten);
            }
        }
#endif
    }
}