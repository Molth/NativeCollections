using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a parsed composite format string.
    /// </summary>
    [NativeCollection(FromType.Standard)]
    public sealed class NativeCompositeFormat
    {
        /// <summary>
        ///     The parsed segments that make up the composite format string.
        /// </summary>
        /// <remarks>
        ///     Every segment represents either a literal or a format hole,
        ///     based on whether Literal is non-null or ArgIndex is non-negative.
        /// </remarks>
        internal readonly (string? Literal, int ArgIndex, int Alignment, string? Format)[] Segments;

        /// <summary>
        ///     The sum of the lengths of all of the literals in <see cref="Segments" />.
        /// </summary>
        internal readonly int LiteralLength;

        /// <summary>
        ///     The number of segments in <see cref="Segments" /> that represent format holes.
        /// </summary>
        internal readonly int FormattedCount;

        /// <summary>
        ///     The number of args required to satisfy the format holes.
        /// </summary>
        /// <remarks>This is equal to one more than the largest index required by any format hole.</remarks>
        internal readonly int ArgsRequired;

        /// <summary>
        ///     Initializes the instance.
        /// </summary>
        /// <param name="format">The composite format string that was parsed.</param>
        /// <param name="segments">The parsed segments.</param>
        private NativeCompositeFormat(string format, (string? Literal, int ArgIndex, int Alignment, string? Format)[] segments)
        {
            Format = format;
            Segments = segments;
            int literalLength = 0, formattedCount = 0, argsRequired = 0;
            foreach (var segment in segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    literalLength = literalLength.WrappingAdd(literal.Length);
                }
                else if (segment.ArgIndex >= 0)
                {
                    formattedCount = formattedCount.WrappingAdd(1);
                    argsRequired = Math.Max(argsRequired, segment.ArgIndex.WrappingAdd(1));
                }
            }

            LiteralLength = literalLength;
            FormattedCount = formattedCount;
            ArgsRequired = argsRequired;
        }

        /// <summary>
        ///     Gets the original composite format string used to create
        ///     this <see cref="NativeCompositeFormat" /> instance.
        /// </summary>
        public string Format { get; }

        /// <summary>
        ///     Gets the minimum number of arguments that must be passed to a
        ///     formatting operation using this <see cref="NativeCompositeFormat" />.
        /// </summary>
        /// <remarks>
        ///     It's permissible to supply more arguments than this value, but it's an error to pass fewer.
        /// </remarks>
        public int MinimumArgumentCount => ArgsRequired;

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeCompositeFormat";

        /// <summary>
        ///     Parse the composite format string <paramref name="format" />.
        /// </summary>
        /// <param name="format">The string to parse.</param>
        /// <returns>The parsed <see cref="NativeCompositeFormat" />.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">
        ///     Thrown when a format item in <paramref name="format" /> is invalid.
        /// </exception>
        /// <remarks>
        ///     An index in a format item is limited to at most seven digits (values below
        ///     10,000,000); a larger index makes the format item invalid and throws
        ///     <see cref="FormatException" />.
        /// </remarks>
        public static NativeCompositeFormat Parse([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            var segments = UnsafeListBuilder<(string? Literal, int ArgIndex, int Alignment, string? Format)>.Create(4);
            try
            {
                var failureOffset = 0;
                var failureReason = (ExceptionResource)(-1);
                if (!TryParseLiterals(format, ref segments, ref failureOffset, ref failureReason))
                    ThrowHelpers.ThrowFormatInvalidString(failureOffset, failureReason);

                return new NativeCompositeFormat(format, segments.AsSpan().ToArray());
            }
            finally
            {
                segments.Dispose();
            }
        }

        /// <summary>
        ///     Throws an exception if the specified number of arguments is fewer than the number required.
        /// </summary>
        /// <param name="numArgs">The number of arguments provided by the caller.</param>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ValidateNumberOfArgs(int numArgs)
        {
            if (numArgs < ArgsRequired)
                ThrowHelpers.ThrowFormatIndexOutOfRange();
        }

        /// <summary>
        ///     Parse the composite format string into segments.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="segments">The list into which to store the segments.</param>
        /// <param name="failureOffset">
        ///     The offset at which a parsing error occurred if <see langword="false" /> is returned.
        /// </param>
        /// <param name="failureReason">
        ///     The reason for a parsing failure if <see langword="false" /> is returned.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the format string can be parsed
        ///     successfully; otherwise, <see langword="false" />.
        /// </returns>
        [SkipLocalsInit]
        private static bool TryParseLiterals(ReadOnlySpan<char> format, ref UnsafeListBuilder<(string? Literal, int ArgIndex, int Alignment, string? Format)> segments, ref int failureOffset, ref ExceptionResource failureReason)
        {
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                var position = 0;
                while (true)
                {
                    char ch;
                    while (true)
                    {
                        var remainder = format.Slice(position);
                        var countUntilNextBrace = remainder.IndexOfAny('{', '}');
                        if (countUntilNextBrace < 0)
                        {
                            temp.Append(remainder);
                            segments.Append((temp.ToString(), -1, 0, null));
                            return true;
                        }

                        temp.Append(remainder.Slice(0, countUntilNextBrace));
                        position = position.WrappingAdd(countUntilNextBrace);
                        var brace = format[position];
                        if (!TryMoveNext(format, ref position, out ch))
                            goto FailureUnclosedFormatItem;

                        if (I32.FromCharUnchecked(brace) == I32.FromCharUnchecked(ch))
                        {
                            temp.Append(ch);
                            position = position.WrappingAdd(1);
                            continue;
                        }

                        if (I32.FromCharUnchecked(brace) != I32.FromCharUnchecked('{'))
                            goto FailureUnexpectedClosingBrace;

                        segments.Append((temp.ToString(), -1, 0, null));
                        temp.Clear();

                        break;
                    }

                    var width = 0;
                    string? itemFormat = null;
                    var index = ch.WrappingSub('0');
                    if (U32.FromI32Unchecked(index) >= 10U)
                        goto FailureExpectedAsciiDigit;

                    if (!TryMoveNext(format, ref position, out ch))
                        goto FailureUnclosedFormatItem;

                    if (I32.FromCharUnchecked(ch) != I32.FromCharUnchecked('}'))
                    {
                        while (CharHelpers.IsAsciiDigit(ch) && index < 1_000_000)
                        {
                            index = index.WrappingMul(10).WrappingAdd(I32.FromCharUnchecked(ch)).WrappingSub(I32.FromCharUnchecked('0'));
                            if (!TryMoveNext(format, ref position, out ch))
                                goto FailureUnclosedFormatItem;
                        }

                        while (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked(' '))
                        {
                            if (!TryMoveNext(format, ref position, out ch))
                                goto FailureUnclosedFormatItem;
                        }

                        if (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked(','))
                        {
                            while (true)
                            {
                                if (!TryMoveNext(format, ref position, out ch))
                                    goto FailureUnclosedFormatItem;

                                if (I32.FromCharUnchecked(ch) != I32.FromCharUnchecked(' '))
                                    break;
                            }

                            var leftJustify = 1;
                            if (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked('-'))
                            {
                                leftJustify = -1;
                                if (!TryMoveNext(format, ref position, out ch))
                                    goto FailureUnclosedFormatItem;
                            }

                            width = ch.WrappingSub('0');
                            if (U32.FromI32Unchecked(width) >= 10U)
                                goto FailureExpectedAsciiDigit;

                            if (!TryMoveNext(format, ref position, out ch))
                                goto FailureUnclosedFormatItem;

                            while (CharHelpers.IsAsciiDigit(ch))
                            {
                                var digit = I32.FromCharUnchecked(ch).WrappingSub(I32.FromCharUnchecked('0'));
                                if (width > int.MaxValue.WrappingSub(digit) / 10)
                                    goto FailureUnclosedFormatItem;

                                width = width.WrappingMul(10).WrappingAdd(digit);
                                if (!TryMoveNext(format, ref position, out ch))
                                    goto FailureUnclosedFormatItem;
                            }

                            width = width.WrappingMul(leftJustify);
                            while (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked(' '))
                            {
                                if (!TryMoveNext(format, ref position, out ch))
                                    goto FailureUnclosedFormatItem;
                            }
                        }

                        if (I32.FromCharUnchecked(ch) != I32.FromCharUnchecked('}'))
                        {
                            if (I32.FromCharUnchecked(ch) != I32.FromCharUnchecked(':'))
                                goto FailureUnclosedFormatItem;

                            var startingPosition = position;
                            while (true)
                            {
                                if (!TryMoveNext(format, ref position, out ch))
                                    goto FailureUnclosedFormatItem;

                                if (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked('}'))
                                    break;

                                if (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked('{'))
                                    goto FailureUnclosedFormatItem;
                            }

                            startingPosition = startingPosition.WrappingAdd(1);
                            itemFormat = format.Slice(startingPosition, position.WrappingSub(startingPosition)).ToString();
                        }
                    }

                    position = position.WrappingAdd(1);
                    segments.Append((null, index, width, itemFormat));
                }

                FailureUnexpectedClosingBrace:
                failureReason = ExceptionResource.Format_UnexpectedClosingBrace;
                failureOffset = position;
                return false;

                FailureUnclosedFormatItem:
                failureReason = ExceptionResource.Format_UnclosedFormatItem;
                failureOffset = position;
                return false;

                FailureExpectedAsciiDigit:
                failureReason = ExceptionResource.Format_ExpectedAsciiDigit;
                failureOffset = position;
                return false;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static bool TryMoveNext(ReadOnlySpan<char> format, ref int position, out char nextChar)
                {
                    position = position.WrappingAdd(1);
                    if (U32.FromI32Unchecked(position) >= U32.FromI32Unchecked(format.Length))
                    {
                        nextChar = '\0';
                        return false;
                    }

                    nextChar = format[position];
                    return true;
                }
            }
        }
    }
}