using System;
using System.Runtime.InteropServices;
#if NET6_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632
#pragma warning disable CS8500
#pragma warning disable CS9081

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a handler used by the language compiler to append interpolated strings into
    ///     <see cref="NativeString" /> instances.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
#if NET6_0_OR_GREATER
    [InterpolatedStringHandler]
#endif
    public ref struct NativeStringInterpolatedStringHandler
    {
        /// <summary>
        ///     The associated NativeStringBuilder to which to append.
        /// </summary>
        internal readonly NativeString StringBuilder;

        /// <summary>
        ///     Optional provider to pass to IFormattable.ToString or ISpanFormattable.TryFormat calls.
        /// </summary>
        private readonly IFormatProvider? _provider;

        /// <summary>Whether <see cref="_provider" /> provides an ICustomFormatter.</summary>
        /// <remarks>
        ///     Custom formatters are very rare.  We want to support them, but it's ok if we make them more expensive
        ///     in order to make them as pay-for-play as possible.  So, we avoid adding another reference type field
        ///     to reduce the size of the handler and to reduce required zero'ing, by only storing whether the provider
        ///     provides a formatter, rather than actually storing the formatter.  This in turn means, if there is a
        ///     formatter, we pay for the extra interface call on each AppendFormatted that needs it.
        /// </remarks>
        private readonly bool _hasCustomFormatter;

        /// <summary>
        ///     Tracks the cumulative success state of all append operations.
        /// </summary>
        internal bool Result;

        /// <summary>Creates a handler used to append an interpolated string into a <see cref="NativeString" />.</summary>
        /// <param name="literalLength">
        ///     The number of constant characters outside of interpolation expressions in the interpolated
        ///     string.
        /// </param>
        /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
        /// <param name="stringBuilder">The associated NativeStringBuilder to which to append.</param>
        /// <remarks>
        ///     This is intended to be called only by compiler-generated code. Arguments are not validated as they'd otherwise
        ///     be for members intended to be used directly.
        /// </remarks>
        public NativeStringInterpolatedStringHandler(int literalLength, int formattedCount, in NativeString stringBuilder)
        {
            StringBuilder = stringBuilder;
            _provider = null;
            _hasCustomFormatter = false;
            Result = true;
        }

        /// <summary>Creates a handler used to translate an interpolated string into a <see cref="string" />.</summary>
        /// <param name="literalLength">
        ///     The number of constant characters outside of interpolation expressions in the interpolated
        ///     string.
        /// </param>
        /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
        /// <param name="stringBuilder">The associated NativeStringBuilder to which to append.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <remarks>
        ///     This is intended to be called only by compiler-generated code. Arguments are not validated as they'd otherwise
        ///     be for members intended to be used directly.
        /// </remarks>
        public NativeStringInterpolatedStringHandler(int literalLength, int formattedCount, in NativeString stringBuilder, IFormatProvider? provider)
        {
            StringBuilder = stringBuilder;
            _provider = provider;
            _hasCustomFormatter = provider != null && FormatHelpers.HasCustomFormatter(provider);
            Result = true;
        }

        /// <summary>Writes the specified string to the handler.</summary>
        /// <param name="value">The string to write.</param>
        public void AppendLiteral(string value)
        {
            ref var sbRef = ref StringBuilder.AsRef();
            Result &= sbRef.Append(value);
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public void AppendFormatted<T>(T? value) where T : struct
        {
            if (value == null)
                return;
            AppendFormatted(value.GetValueOrDefault());
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public void AppendFormatted<T>(T value)
        {
            ref var sbRef = ref StringBuilder.AsRef();
            if (_hasCustomFormatter)
            {
                var formatter = (ICustomFormatter?)_provider!.GetFormat(typeof(ICustomFormatter));
                if (formatter != null)
                    Result &= sbRef.Append(formatter.Format(null, value, _provider));
                return;
            }

            Result &= sbRef.AppendFormat(value, default, _provider);
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The format string.</param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public void AppendFormatted<T>(T? value, string? format) where T : struct
        {
            if (value == null)
                return;
            AppendFormatted(value.GetValueOrDefault(), format);
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The format string.</param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public void AppendFormatted<T>(T value, string? format)
        {
            ref var sbRef = ref StringBuilder.AsRef();
            if (_hasCustomFormatter)
            {
                var formatter = (ICustomFormatter?)_provider!.GetFormat(typeof(ICustomFormatter));
                if (formatter != null)
                    Result &= sbRef.Append(formatter.Format(format, value, _provider));
                return;
            }

            Result &= sbRef.AppendFormat(value, format, _provider);
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="alignment">
        ///     Minimum number of characters that should be written for this value.  If the value is negative,
        ///     it indicates left-aligned and the required minimum is the absolute value.
        /// </param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public void AppendFormatted<T>(T? value, int alignment) where T : struct
        {
            if (value == null)
                return;
            AppendFormatted(value.GetValueOrDefault(), alignment);
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="alignment">
        ///     Minimum number of characters that should be written for this value.  If the value is negative,
        ///     it indicates left-aligned and the required minimum is the absolute value.
        /// </param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public void AppendFormatted<T>(T value, int alignment) => AppendFormatted(value, alignment, null);

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The format string.</param>
        /// <param name="alignment">
        ///     Minimum number of characters that should be written for this value.  If the value is negative,
        ///     it indicates left-aligned and the required minimum is the absolute value.
        /// </param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public void AppendFormatted<T>(T? value, int alignment, string? format) where T : struct
        {
            if (value == null)
                return;
            AppendFormatted(value.GetValueOrDefault(), alignment, format);
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The format string.</param>
        /// <param name="alignment">
        ///     Minimum number of characters that should be written for this value.  If the value is negative,
        ///     it indicates left-aligned and the required minimum is the absolute value.
        /// </param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public void AppendFormatted<T>(T value, int alignment, string? format)
        {
            if (alignment == 0)
            {
                AppendFormatted(value, format);
            }
            else if (alignment < 0)
            {
                ref var sbRef = ref StringBuilder.AsRef();
                var start = sbRef.Length;
                AppendFormatted(value, format);
                var paddingRequired = -alignment - (sbRef.Length - start);
                if (paddingRequired > 0)
                    Result &= sbRef.Append(' ', paddingRequired);
            }
            else
            {
                AppendFormattedWithTempSpace(value, alignment, format);
            }
        }

        /// <summary>
        ///     Formats into temporary space and then appends the result into the NativeStringBuilder.
        /// </summary>
        private void AppendFormattedWithTempSpace<T>(T value, int alignment, string? format)
        {
            using var handler = new NativeStringBuilder<char>(stackalloc char[512], 0);
            handler.AppendFormat(value, format, _provider);
            var buffer = handler.Text;
            AppendFormatted(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(buffer), buffer.Length), alignment);
        }

        /// <summary>Writes the specified character span to the handler.</summary>
        /// <param name="value">The span to write.</param>
        public void AppendFormatted(ReadOnlySpan<char> value)
        {
            ref var sbRef = ref StringBuilder.AsRef();
            Result &= sbRef.Append(value);
        }

        /// <summary>Writes the specified string of chars to the handler.</summary>
        /// <param name="value">The span to write.</param>
        /// <param name="alignment">
        ///     Minimum number of characters that should be written for this value.  If the value is negative,
        ///     it indicates left-aligned and the required minimum is the absolute value.
        /// </param>
        /// <param name="format">The format string.</param>
        public void AppendFormatted(ReadOnlySpan<char> value, int alignment, string? format = null)
        {
            ref var sbRef = ref StringBuilder.AsRef();
            if (alignment == 0)
            {
                Result &= sbRef.Append(value);
            }
            else
            {
                var leftAlign = false;
                if (alignment < 0)
                {
                    leftAlign = true;
                    alignment = -alignment;
                }

                var paddingRequired = alignment - value.Length;
                if (paddingRequired <= 0)
                {
                    Result &= sbRef.Append(value);
                }
                else if (leftAlign)
                {
                    Result &= sbRef.Append(value);
                    Result &= sbRef.Append(' ', paddingRequired);
                }
                else
                {
                    Result &= sbRef.Append(' ', paddingRequired);
                    Result &= sbRef.Append(value);
                }
            }
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        public void AppendFormatted(string? value)
        {
            if (!_hasCustomFormatter)
            {
                ref var sbRef = ref StringBuilder.AsRef();
                Result &= sbRef.Append(value);
            }
            else
            {
                AppendFormatted<string?>(value);
            }
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="alignment">
        ///     Minimum number of characters that should be written for this value.  If the value is negative,
        ///     it indicates left-aligned and the required minimum is the absolute value.
        /// </param>
        /// <param name="format">The format string.</param>
        public void AppendFormatted(string? value, int alignment, string? format = null) => AppendFormatted<string?>(value, alignment, format);

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="alignment">
        ///     Minimum number of characters that should be written for this value.  If the value is negative,
        ///     it indicates left-aligned and the required minimum is the absolute value.
        /// </param>
        /// <param name="format">The format string.</param>
        public void AppendFormatted(object? value, int alignment = 0, string? format = null) => AppendFormatted<object?>(value, alignment, format);
    }
}