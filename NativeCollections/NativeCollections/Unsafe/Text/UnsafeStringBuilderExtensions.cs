using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides extension methods for string builders.
    /// </summary>
    public static partial class UnsafeStringBuilderExtensions
    {
        /// <summary>
        ///     Appends the default line terminator to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the result.</param>
        public static void AppendLine(in this UnsafeStringBuilder<char> builder)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLine;
            builderRef.EnsureCapacity(builderRef.Length.WrappingAdd(newLine.Length));
            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(builderRef.Length))), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), Usize.FromI32Unchecked(newLine.Length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
            builderRef.Advance(newLine.Length);
        }

        /// <summary>
        ///     Appends the specified read-only character span followed by the default line terminator to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="buffer">The values to append before the line terminator.</param>
        public static void AppendLine(in this UnsafeStringBuilder<char> builder, [MustNotSelfAlias] ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLine;
            builderRef.EnsureCapacity(builderRef.Length.WrappingAdd(buffer.Length).WrappingAdd(newLine.Length));
            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(builderRef.Length))), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI32Unchecked(buffer.Length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
            builderRef.Advance(buffer.Length);
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(builderRef.Length))), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), Usize.FromI32Unchecked(newLine.Length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
            builderRef.Advance(newLine.Length);
        }

        /// <summary>
        ///     Appends the specified character followed by the default line terminator to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="value">The value to append before the line terminator.</param>
        public static void AppendLine(in this UnsafeStringBuilder<char> builder, char value)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLine;
            builderRef.EnsureCapacity(builderRef.Length.WrappingAdd(1).WrappingAdd(newLine.Length));
            builderRef.Buffer[builderRef.Length] = value;
            builderRef.Advance(1);
            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(builderRef.Length))), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), Usize.FromI32Unchecked(newLine.Length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
            builderRef.Advance(newLine.Length);
        }

        /// <summary>
        ///     Concatenates the strings of the provided span, using the specified separator between
        ///     each string, then appends the result to the current instance of the string builder.
        /// </summary>
        /// <typeparam name="T">The type of the values to join.</typeparam>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="separator">The separator to place between values.</param>
        /// <param name="values">The values to join and append.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<char> builder, char separator, ReadOnlySpan<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            ref var reference = ref MemoryMarshal.GetReference(values);
            for (var i = 0; i < values.Length; i = i.WrappingAdd(1))
            {
                var value = Unsafe.Add(ref reference, Isize.FromI32Unchecked(i));
                if (i != 0)
                    builderRef.Append(separator);

                builderRef.AppendFormat(value);
            }
        }

        /// <summary>
        ///     Concatenates the strings of the provided sequence, using the specified separator between
        ///     each string, then appends the result to the current instance of the string builder.
        /// </summary>
        /// <typeparam name="T">The type of the values to join.</typeparam>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="separator">The separator to place between values.</param>
        /// <param name="values">The values to join and append.</param>
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
        ///     Concatenates the strings of the provided span, using the specified separator between
        ///     each string, then appends the result to the current instance of the string builder.
        /// </summary>
        /// <typeparam name="T">The type of the values to join.</typeparam>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="separator">The separator to place between values.</param>
        /// <param name="values">The values to join and append.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<char> builder, ReadOnlySpan<char> separator, ReadOnlySpan<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            ref var reference = ref MemoryMarshal.GetReference(values);
            for (var i = 0; i < values.Length; i = i.WrappingAdd(1))
            {
                var value = Unsafe.Add(ref reference, Isize.FromI32Unchecked(i));
                if (i != 0)
                    builderRef.Append(separator);

                builderRef.AppendFormat(value);
            }
        }

        /// <summary>
        ///     Concatenates the strings of the provided sequence, using the specified separator between
        ///     each string, then appends the result to the current instance of the string builder.
        /// </summary>
        /// <typeparam name="T">The type of the values to join.</typeparam>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="separator">The separator to place between values.</param>
        /// <param name="values">The values to join and append.</param>
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
        ///     Removes all leading white-space characters from this instance.
        /// </summary>
        /// <param name="builder">The string builder to modify.</param>
        public static void TrimStart(in this UnsafeStringBuilder<char> builder)
        {
            ref var builderRef = ref builder.AsRef();
            if (builderRef.IsEmpty)
                return;

            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            var start = 0;
            while (start < builderRef.Length && char.IsWhiteSpace(Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))))
                start = start.WrappingAdd(1);

            if (start > 0 && start < builderRef.Length)
            {
                var count = builderRef.Length.WrappingSub(start);
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
                builderRef.SetLength(count);
            }
            else if (start >= builderRef.Length)
            {
                builderRef.SetLength(0);
            }
        }

        /// <summary>
        ///     Removes all trailing white-space characters from this instance.
        /// </summary>
        /// <param name="builder">The string builder to modify.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TrimEnd(in this UnsafeStringBuilder<char> builder)
        {
            ref var builderRef = ref builder.AsRef();
            if (builderRef.IsEmpty)
                return;

            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            var end = builderRef.Length.WrappingSub(1);
            while (end >= 0 && char.IsWhiteSpace(Unsafe.Add(ref reference, Isize.FromI32Unchecked(end))))
                end = end.WrappingSub(1);

            builderRef.SetLength(end.WrappingAdd(1));
        }

        /// <summary>
        ///     Removes all leading and trailing white-space characters from this instance.
        /// </summary>
        /// <param name="builder">The string builder to modify.</param>
        public static void Trim(in this UnsafeStringBuilder<char> builder)
        {
            ref var builderRef = ref builder.AsRef();
            if (builderRef.IsEmpty)
                return;

            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            var start = 0;
            var end = builderRef.Length.WrappingSub(1);
            while (start <= end && char.IsWhiteSpace(Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))))
                start = start.WrappingAdd(1);

            while (end >= start && char.IsWhiteSpace(Unsafe.Add(ref reference, Isize.FromI32Unchecked(end))))
                end = end.WrappingSub(1);

            var newLength = end.WrappingSub(start).WrappingAdd(1);
            if (newLength <= 0)
            {
                builderRef.SetLength(0);
                return;
            }

            if (start > 0)
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))), Usize.FromI32Unchecked(newLength).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));

            builderRef.SetLength(newLength);
        }

        /// <summary>
        ///     Pads this instance on the left with spaces to reach the specified total length.
        /// </summary>
        /// <param name="builder">The string builder to modify.</param>
        /// <param name="totalWidth">The total width of the padded string builder.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PadLeft(in this UnsafeStringBuilder<char> builder, int totalWidth)
        {
            ref var builderRef = ref builder.AsRef();
            builderRef.PadLeft(totalWidth, ' ');
        }

        /// <summary>
        ///     Pads this instance on the right with spaces to reach the specified total length.
        /// </summary>
        /// <param name="builder">The string builder to modify.</param>
        /// <param name="totalWidth">The total width of the padded string builder.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PadRight(in this UnsafeStringBuilder<char> builder, int totalWidth)
        {
            ref var builderRef = ref builder.AsRef();
            builderRef.PadRight(totalWidth, ' ');
        }

        /// <summary>
        ///     Indicates whether this instance is empty or consists only of white-space characters.
        /// </summary>
        /// <param name="builder">The string builder to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> if this is uninitialized, empty, or consists
        ///     only of white-space characters; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace(in this UnsafeStringBuilder<char> builder)
        {
            ref var builderRef = ref builder.AsRef();
            return UnsafeString.From(builderRef.Text).IsNullOrWhiteSpace();
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T">The type of the value to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T>(in this UnsafeStringBuilder<char> builder, T? obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : struct
        {
            if (!obj.HasValue)
                return;

            builder.AppendFormat(obj.GetValueOrDefault(), format, provider);
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T">The type of the value to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T>(in this UnsafeStringBuilder<char> builder, T? obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!FormatHelpers.TryFormat(obj, builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="_">The format string, which is ignored by this overload.</param>
        /// <param name="__">The format provider, which is ignored by this overload.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, bool obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Appends the interpolated string produced by the specified handler to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="handler">The interpolated string handler to append.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendInterpolated(in this UnsafeStringBuilder<char> builder, [InterpolatedStringHandlerArgument("builder")] ref UnsafeStringBuilderUtf16InterpolatedStringHandler handler)
        {
        }

        /// <summary>
        ///     Appends the interpolated string produced by the specified handler to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="handler">The interpolated string handler to append.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendInterpolated(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [InterpolatedStringHandlerArgument("builder", "provider")] ref UnsafeStringBuilderUtf16InterpolatedStringHandler handler)
        {
        }

        /// <summary>
        ///     Appends the formatted string produced by the specified handler to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="handler">The interpolated string handler to append.</param>
        /// <param name="clear">
        ///     <see langword="true" /> to clear the handler after appending; otherwise, <see langword="false" />.
        /// </param>
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
        ///     Appends the formatted string produced by the specified handler to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="handler">The interpolated string handler to append.</param>
        /// <param name="clear">
        ///     <see langword="true" /> to clear the handler after appending; otherwise, <see langword="false" />.
        /// </param>
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
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable<T>(in this UnsafeStringBuilder<char> builder, T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }
#else
        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, decimal obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, DateTime obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, byte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, DateTimeOffset obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, double obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="_">The format provider, which is ignored by this overload.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, Guid obj, ReadOnlySpan<char> format = default, IFormatProvider? _ = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, Half obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));
            builderRef.Advance(charsWritten);
        }
#endif

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, short obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, int obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, long obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, sbyte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, float obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, TimeSpan obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, ushort obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, uint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, ulong obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, nint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (Environment.Is64BitProcess)
                builder.AppendFormattable(I64.FromIsizeUnchecked(obj), format, provider);
            else
                builder.AppendFormattable(I32.FromIsizeUnchecked(obj), format, provider);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, nuint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (Environment.Is64BitProcess)
                builder.AppendFormattable(U64.FromUsizeUnchecked(obj), format, provider);
            else
                builder.AppendFormattable(U32.FromUsizeUnchecked(obj), format, provider);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="_">The format string, which is ignored by this overload.</param>
        /// <param name="__">The format provider, which is ignored by this overload.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<char> builder, Version obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            ref var builderRef = ref builder.AsRef();
            int charsWritten;
            while (!obj.TryFormat(builderRef.Space, out charsWritten))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(charsWritten);
        }
#endif

        /// <summary>
        ///     Searches for the specified sequence and returns the index of its first occurrence.
        ///     If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="builder">The string builder to search.</param>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>
        ///     The zero-based character index of the first occurrence of the specified sequence; otherwise, -1.
        /// </returns>
        /// <remarks>
        ///     The returned index is a character position in the decoded UTF-8 text, not a byte offset.
        /// </remarks>
        [SkipLocalsInit]
        public static int IndexOf(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            byte[]? array = null;
            var bytes = byteCount <= 1024 ? stackalloc byte[byteCount] : (array = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);
            try
            {
                Encoding.UTF8.GetBytes(buffer, bytes);
                var byteIndex = builderRef.IndexOf(bytes);
                return byteIndex < 0 ? -1 : Encoding.UTF8.GetCharCount(builderRef.Text.Slice(0, byteIndex));
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Searches for the specified sequence and returns the index of its last occurrence.
        ///     If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="builder">The string builder to search.</param>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>
        ///     The zero-based character index of the last occurrence of the specified sequence; otherwise, -1.
        /// </returns>
        /// <remarks>
        ///     The returned index is a character position in the decoded UTF-8 text, not a byte offset.
        /// </remarks>
        [SkipLocalsInit]
        public static int LastIndexOf(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            byte[]? array = null;
            var bytes = byteCount <= 1024 ? stackalloc byte[byteCount] : (array = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);
            try
            {
                Encoding.UTF8.GetBytes(buffer, bytes);
                var byteIndex = builderRef.LastIndexOf(bytes);
                return byteIndex < 0 ? -1 : Encoding.UTF8.GetCharCount(builderRef.Text.Slice(0, byteIndex));
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Searches for the first index of any of the specified values similar to calling
        ///     IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="builder">The string builder to search.</param>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>
        ///     The zero-based index of the first occurrence of any of the specified values; otherwise, -1.
        /// </returns>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 The search runs over decoded characters instead of the raw UTF-8 bytes.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 The returned index is a character position and a multi-byte character is
        ///                 never matched through one of its continuation bytes.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        [SkipLocalsInit]
        public static int IndexOfAny(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var text = builderRef.Text;
            var charCount = Encoding.UTF8.GetCharCount(text);
            char[]? array = null;
            var chars = charCount <= 512 ? stackalloc char[charCount] : (array = ArrayPool<char>.Shared.Rent(charCount)).AsSpan(0, charCount);
            try
            {
                Encoding.UTF8.GetChars(text, chars);
                return chars.IndexOfAny(buffer);
            }
            finally
            {
                if (array != null)
                    ArrayPool<char>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Searches for the last index of any of the specified values similar to calling
        ///     LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="builder">The string builder to search.</param>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>
        ///     The zero-based index of the last occurrence of any of the specified values; otherwise, -1.
        /// </returns>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 The search runs over decoded characters instead of the raw UTF-8 bytes.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 The returned index is a character position and a multi-byte character is
        ///                 never matched through one of its continuation bytes.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        [SkipLocalsInit]
        public static int LastIndexOfAny(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var text = builderRef.Text;
            var charCount = Encoding.UTF8.GetCharCount(text);
            char[]? array = null;
            var chars = charCount <= 512 ? stackalloc char[charCount] : (array = ArrayPool<char>.Shared.Rent(charCount)).AsSpan(0, charCount);
            try
            {
                Encoding.UTF8.GetChars(text, chars);
                return chars.LastIndexOfAny(buffer);
            }
            finally
            {
                if (array != null)
                    ArrayPool<char>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Searches for the specified values and returns <see langword="true" /> if found. If not
        ///     found, returns <see langword="false" />. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="builder">The string builder to search.</param>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>
        ///     <see langword="true" /> if the builder contains the
        ///     specified sequence; otherwise, <see langword="false" />.
        /// </returns>
        [SkipLocalsInit]
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
        ///     Removes all occurrences of the specified character sequence from this instance.
        /// </summary>
        /// <param name="builder">The string builder to modify.</param>
        /// <param name="buffer">The values to append.</param>
        [SkipLocalsInit]
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
        ///     Inserts the sequence of characters into this instance at the specified character position.
        /// </summary>
        /// <param name="builder">The string builder to modify.</param>
        /// <param name="startIndex">The zero-based character index at which to start the operation.</param>
        /// <param name="buffer">The sequence to insert.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        /// <remarks>
        ///     <paramref name="startIndex" /> is a character position, not a byte offset. It is
        ///     validated against the decoded character count and mapped to the corresponding
        ///     UTF-8 byte offset before the bytes are inserted.
        /// </remarks>
        [SkipLocalsInit]
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
                var text = builderRef.Text;
                var charCount = Encoding.UTF8.GetCharCount(text);
                if (U32.FromI32Unchecked(startIndex) > U32.FromI32Unchecked(charCount))
                    return false;

                char[]? array1 = null;
                var chars = charCount <= 512 ? stackalloc char[charCount] : (array1 = ArrayPool<char>.Shared.Rent(charCount)).AsSpan(0, charCount);
                try
                {
                    Encoding.UTF8.GetChars(text, chars);
                    var byteIndex = Encoding.UTF8.GetByteCount(chars.Slice(0, startIndex));
                    return builderRef.Insert(byteIndex, bytes2);
                }
                finally
                {
                    if (array1 != null)
                        ArrayPool<char>.Shared.Return(array1);
                }
            }
            finally
            {
                if (array != null)
                    ArrayPool<byte>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Replaces all occurrences of the specified character sequence
        ///     in this instance with another specified character sequence.
        /// </summary>
        /// <param name="builder">The string builder to modify.</param>
        /// <param name="oldValue">The sequence to replace.</param>
        /// <param name="newValue">The replacement sequence.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        [SkipLocalsInit]
        public static bool Replace(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount1 = Encoding.UTF8.GetByteCount(oldValue);
            var byteCount2 = Encoding.UTF8.GetByteCount(newValue);
            var byteCount = byteCount1.WrappingAdd(byteCount2);
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
        ///     Determines whether the specified sequence appears at the start of the span.
        /// </summary>
        /// <param name="builder">The string builder to search.</param>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>
        ///     <see langword="true" /> if this starts with the specified sequence; otherwise, <see langword="false" />.
        /// </returns>
        [SkipLocalsInit]
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
        ///     Determines whether the specified sequence appears at the end of the span.
        /// </summary>
        /// <param name="builder">The string builder to search.</param>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>
        ///     <see langword="true" /> if this ends with the specified sequence; otherwise, <see langword="false" />.
        /// </returns>
        [SkipLocalsInit]
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
        ///     Determines the relative order of the sequences being compared
        ///     by comparing the elements using IComparable{T}.CompareTo(T).
        /// </summary>
        /// <param name="builder">The string builder to search.</param>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>A value that indicates the relative order of the sequences.</returns>
        [SkipLocalsInit]
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
        ///     Appends a specified number of copies of the specified character to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="value">The value to append.</param>
        /// <param name="repeatCount">The number of times to append the value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="repeatCount" /> is negative.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the byte count required for <paramref name="repeatCount" /> copies
        ///     exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static void Append(in this UnsafeStringBuilder<byte> builder, char value, int repeatCount)
        {
            ThrowHelpers.ThrowIfNegative(repeatCount, ExceptionArgument.repeatCount);
            ref var builderRef = ref builder.AsRef();
            ReadOnlySpan<char> buffer = stackalloc char[1] { value };
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            builderRef.EnsureCapacity(builderRef.Length.WrappingAdd(byteCount * repeatCount));
            Span<byte> bytes = stackalloc byte[byteCount];
            Encoding.UTF8.GetBytes(buffer, bytes);
            ref var destination = ref MemoryMarshal.GetReference(builderRef.Space);
            ref var source = ref MemoryMarshal.GetReference(bytes);
            switch (byteCount)
            {
                case 1:
                    SpanHelpers.Fill(ref destination, Usize.FromI32Unchecked(repeatCount), source);
                    break;

                case 2:
                    SpanHelpers.Fill(ref Unsafe.As<byte, Utf8FormatHelpers.DummyBytes2>(ref destination), Usize.FromI32Unchecked(repeatCount), Unsafe.ReadUnaligned<Utf8FormatHelpers.DummyBytes2>(ref source));
                    break;

                case 3:
                    SpanHelpers.Fill(ref Unsafe.As<byte, Utf8FormatHelpers.DummyBytes3>(ref destination), Usize.FromI32Unchecked(repeatCount), Unsafe.ReadUnaligned<Utf8FormatHelpers.DummyBytes3>(ref source));
                    break;
            }

            builderRef.Advance(byteCount.WrappingMul(repeatCount));
        }

        /// <summary>
        ///     Appends the specified read-only character span to this instance, encoded as UTF-8.
        /// </summary>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="buffer">The values to append.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            builderRef.EnsureCapacity(builderRef.Length.WrappingAdd(byteCount));
            var bytes = builderRef.GetSpan(byteCount);
            Encoding.UTF8.GetBytes(buffer, bytes);
            builderRef.Advance(bytes.Length);
        }

        /// <summary>
        ///     Appends the specified character to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="value">The value to append.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static void Append(in this UnsafeStringBuilder<byte> builder, char value)
        {
            ReadOnlySpan<char> buffer = stackalloc char[1] { value };
            builder.Append(buffer);
        }

        /// <summary>
        ///     Appends the default line terminator to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the result.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLine(in this UnsafeStringBuilder<byte> builder)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLineUtf8;
            builderRef.Append(newLine);
        }

        /// <summary>
        ///     Appends the specified read-only character span followed by the default line terminator to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="buffer">The values to append before the line terminator.</param>
        public static void AppendLine(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLineUtf8;
            var byteCount = Encoding.UTF8.GetByteCount(buffer);
            builderRef.EnsureCapacity(builderRef.Length.WrappingAdd(byteCount).WrappingAdd(newLine.Length));
            var bytes = builderRef.GetSpan(byteCount);
            Encoding.UTF8.GetBytes(buffer, bytes);
            builderRef.Advance(bytes.Length);
            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(builderRef.Buffer), new IntPtr(builderRef.Length)), ref MemoryMarshal.GetReference(newLine), Usize.FromI32Unchecked(newLine.Length));
            builderRef.Advance(newLine.Length);
        }

        /// <summary>
        ///     Appends the specified character followed by the default line terminator to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="value">The value to append before the line terminator.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static void AppendLine(in this UnsafeStringBuilder<byte> builder, char value)
        {
            ReadOnlySpan<char> buffer = stackalloc char[1] { value };
            builder.AppendLine(buffer);
        }

        /// <summary>
        ///     Appends the specified read-only byte span followed by the default line terminator to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="buffer">The values to append before the line terminator.</param>
        public static void AppendLine(in this UnsafeStringBuilder<byte> builder, [MustNotSelfAlias] ReadOnlySpan<byte> buffer)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLineUtf8;
            builderRef.EnsureCapacity(builderRef.Length.WrappingAdd(buffer.Length).WrappingAdd(newLine.Length));
            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref reference, new IntPtr(builderRef.Length)), ref MemoryMarshal.GetReference(buffer), Usize.FromI32Unchecked(buffer.Length));
            builderRef.Advance(buffer.Length);
            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref reference, new IntPtr(builderRef.Length)), ref MemoryMarshal.GetReference(newLine), Usize.FromI32Unchecked(newLine.Length));
            builderRef.Advance(newLine.Length);
        }

        /// <summary>
        ///     Appends the specified byte followed by the default line terminator to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="value">The value to append before the line terminator.</param>
        public static void AppendLine(in this UnsafeStringBuilder<byte> builder, byte value)
        {
            ref var builderRef = ref builder.AsRef();
            var newLine = UnsafeString.NewLineUtf8;
            builderRef.EnsureCapacity(builderRef.Length.WrappingAdd(1).WrappingAdd(newLine.Length));
            builderRef.Buffer[builderRef.Length] = value;
            builderRef.Advance(1);
            ref var reference = ref MemoryMarshal.GetReference(builderRef.Buffer);
            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref reference, new IntPtr(builderRef.Length)), ref MemoryMarshal.GetReference(newLine), Usize.FromI32Unchecked(newLine.Length));
            builderRef.Advance(newLine.Length);
        }

        /// <summary>
        ///     Concatenates the strings of the provided span, using the specified separator between
        ///     each string, then appends the result to the current instance of the string builder.
        /// </summary>
        /// <typeparam name="T">The type of the values to join.</typeparam>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="separator">The separator to place between values.</param>
        /// <param name="values">The values to join and append.</param>
        [SkipLocalsInit]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<byte> builder, char separator, ReadOnlySpan<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendJoin(separator, values);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Concatenates the strings of the provided sequence, using the specified separator between
        ///     each string, then appends the result to the current instance of the string builder.
        /// </summary>
        /// <typeparam name="T">The type of the values to join.</typeparam>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="separator">The separator to place between values.</param>
        /// <param name="values">The values to join and append.</param>
        [SkipLocalsInit]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<byte> builder, char separator, IEnumerable<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendJoin(separator, values);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Concatenates the strings of the provided span, using the specified separator between
        ///     each string, then appends the result to the current instance of the string builder.
        /// </summary>
        /// <typeparam name="T">The type of the values to join.</typeparam>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="separator">The separator to place between values.</param>
        /// <param name="values">The values to join and append.</param>
        [SkipLocalsInit]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> separator, ReadOnlySpan<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendJoin(separator, values);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Concatenates the strings of the provided sequence, using the specified separator between
        ///     each string, then appends the result to the current instance of the string builder.
        /// </summary>
        /// <typeparam name="T">The type of the values to join.</typeparam>
        /// <param name="builder">The string builder to which to append the result.</param>
        /// <param name="separator">The separator to place between values.</param>
        /// <param name="values">The values to join and append.</param>
        [SkipLocalsInit]
        public static void AppendJoin<T>(in this UnsafeStringBuilder<byte> builder, ReadOnlySpan<char> separator, IEnumerable<T> values)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendJoin(separator, values);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Appends the interpolated string produced by the specified handler to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="handler">The interpolated string handler to append.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendInterpolated(in this UnsafeStringBuilder<byte> builder, [InterpolatedStringHandlerArgument("builder")] ref UnsafeStringBuilderUtf8InterpolatedStringHandler handler)
        {
        }

        /// <summary>
        ///     Appends the interpolated string produced by the specified handler to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="handler">The interpolated string handler to append.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendInterpolated(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [InterpolatedStringHandlerArgument("builder", "provider")] ref UnsafeStringBuilderUtf8InterpolatedStringHandler handler)
        {
        }

        /// <summary>
        ///     Appends the formatted string produced by the specified handler to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="handler">The interpolated string handler to append.</param>
        /// <param name="clear">
        ///     <see langword="true" /> to clear the handler after appending; otherwise, <see langword="false" />.
        /// </param>
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
        ///     Appends the formatted string produced by the specified handler to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="handler">The interpolated string handler to append.</param>
        /// <param name="clear">
        ///     <see langword="true" /> to clear the handler after appending; otherwise, <see langword="false" />.
        /// </param>
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
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T">The type of the value to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T>(in this UnsafeStringBuilder<byte> builder, T? obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : struct
        {
            if (!obj.HasValue)
                return;

            builder.AppendFormat(obj.GetValueOrDefault(), format, provider);
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T">The type of the value to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T>(in this UnsafeStringBuilder<byte> builder, T? obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            int bytesWritten;
            while (!Utf8FormatHelpers.TryFormat(obj, builderRef.Space, out bytesWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(bytesWritten);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="_">The format string, which is ignored by this overload.</param>
        /// <param name="__">The format provider, which is ignored by this overload.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, bool obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            ref var builderRef = ref builder.AsRef();
            Span<char> destination = stackalloc char[8];
            obj.TryFormat(destination, out var charsWritten);
            builderRef.Append(destination.Slice(0, charsWritten));
        }

#if NET8_0_OR_GREATER
        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable<T>(in this UnsafeStringBuilder<byte> builder, T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : IUtf8SpanFormattable
        {
            ref var builderRef = ref builder.AsRef();
            int bytesWritten;
            while (!obj.TryFormat(builderRef.Space, out bytesWritten, format, provider))
                builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

            builderRef.Advance(bytesWritten);
        }
#elif NET6_0_OR_GREATER
        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        public static void AppendFormattable<T>(in this UnsafeStringBuilder<byte> builder, T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }
#else
        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, decimal obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, DateTime obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, byte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, DateTimeOffset obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, double obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="_">The format provider, which is ignored by this overload.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, Guid obj, ReadOnlySpan<char> format = default, IFormatProvider? _ = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, _);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, Half obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }
#endif

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, short obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, int obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, long obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, sbyte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, float obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, TimeSpan obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, ushort obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, uint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, ulong obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, format, provider);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, nint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (Environment.Is64BitProcess)
                builder.AppendFormattable(I64.FromIsizeUnchecked(obj), format, provider);
            else
                builder.AppendFormattable(I32.FromIsizeUnchecked(obj), format, provider);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, nuint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (Environment.Is64BitProcess)
                builder.AppendFormattable(U64.FromUsizeUnchecked(obj), format, provider);
            else
                builder.AppendFormattable(U32.FromUsizeUnchecked(obj), format, provider);
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="builder">The string builder to which to append the formatted value.</param>
        /// <param name="obj">The value to format.</param>
        /// <param name="_">The format string, which is ignored by this overload.</param>
        /// <param name="__">The format provider, which is ignored by this overload.</param>
        [SkipLocalsInit]
        public static void AppendFormattable(in this UnsafeStringBuilder<byte> builder, Version obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormattable(obj, _, __);
                builderRef.Append(temp.AsReadOnlySpan());
            }
        }
#endif
    }
}