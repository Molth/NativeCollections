using System;
using System.Runtime.CompilerServices;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632
#pragma warning disable CS8500
#pragma warning disable CS9081

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a parsed composite format string.
    /// </summary>
    public sealed class NativeCompositeFormat
    {
        /// <summary>The parsed segments that make up the composite format string.</summary>
        /// <remarks>
        ///     Every segment represents either a literal or a format hole, based on whether Literal
        ///     is non-null or ArgIndex is non-negative.
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

        /// <summary>The number of args required to satisfy the format holes.</summary>
        /// <remarks>This is equal to one more than the largest index required by any format hole.</remarks>
        internal readonly int ArgsRequired;

        /// <summary>Initializes the instance.</summary>
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
                    literalLength += literal.Length;
                }
                else if (segment.ArgIndex >= 0)
                {
                    formattedCount++;
                    argsRequired = Math.Max(argsRequired, segment.ArgIndex + 1);
                }
            }

            LiteralLength = literalLength;
            FormattedCount = formattedCount;
            ArgsRequired = argsRequired;
        }

        /// <summary>
        ///     Gets the original composite format string used to create this <see cref="NativeCompositeFormat" /> instance.
        /// </summary>
        public string Format { get; }

        /// <summary>
        ///     Gets the minimum number of arguments that must be passed to a formatting operation using this
        ///     <see cref="NativeCompositeFormat" />.
        /// </summary>
        /// <remarks>It's permissible to supply more arguments than this value, but it's an error to pass fewer.</remarks>
        public int MinimumArgumentCount => ArgsRequired;

        /// <summary>Parse the composite format string <paramref name="format" />.</summary>
        /// <param name="format">The string to parse.</param>
        /// <returns>The parsed <see cref="NativeCompositeFormat" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="format" /> is null.</exception>
        /// <exception cref="FormatException">A format item in <paramref name="format" /> is invalid.</exception>
        public static NativeCompositeFormat Parse(
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            using var segments = new NativeListBuilder<(string? Literal, int ArgIndex, int Alignment, string? Format)>(4);
            ref var segmentsRef = ref segments.AsRef();
            var failureOffset = 0;
            var failureReason = (InvalidFormatReason)(-1);
            if (!TryParseLiterals(format, ref segmentsRef, ref failureOffset, ref failureReason))
                ThrowHelpers.ThrowFormatInvalidString(failureOffset, failureReason);
            return new NativeCompositeFormat(format, segmentsRef.AsSpan().ToArray());
        }

        /// <summary>Throws an exception if the specified number of arguments is fewer than the number required.</summary>
        /// <param name="numArgs">The number of arguments provided by the caller.</param>
        /// <exception cref="FormatException">An insufficient number of arguments were provided.</exception>
        internal void ValidateNumberOfArgs(int numArgs)
        {
            if (numArgs < ArgsRequired)
                ThrowHelpers.ThrowFormatIndexOutOfRange();
        }

        /// <summary>Parse the composite format string into segments.</summary>
        /// <param name="format">The format string.</param>
        /// <param name="segments">The list into which to store the segments.</param>
        /// <param name="failureOffset">The offset at which a parsing error occured if <see langword="false" /> is returned.</param>
        /// <param name="failureReason">The reason for a parsing failure if <see langword="false" /> is returned.</param>
        /// <returns>true if the format string can be parsed successfully; otherwise, false.</returns>
        private static bool TryParseLiterals(ReadOnlySpan<char> format, ref NativeListBuilder<(string? Literal, int ArgIndex, int Alignment, string? Format)> segments, ref int failureOffset, ref InvalidFormatReason failureReason)
        {
            using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
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
                    position += countUntilNextBrace;
                    var brace = format[position];
                    if (!TryMoveNext(format, ref position, out ch))
                        goto FailureUnclosedFormatItem;
                    if (brace == ch)
                    {
                        temp.Append(ch);
                        position++;
                        continue;
                    }

                    if (brace != '{')
                        goto FailureUnexpectedClosingBrace;
                    segments.Append((temp.ToString(), -1, 0, null));
                    temp.Clear();
                    break;
                }

                var width = 0;
                string? itemFormat = null;
                var index = ch - '0';
                if ((uint)index >= 10u)
                    goto FailureExpectedAsciiDigit;
                if (!TryMoveNext(format, ref position, out ch))
                    goto FailureUnclosedFormatItem;
                if (ch != '}')
                {
                    while (CharHelpers.IsAsciiDigit(ch))
                    {
                        index = index * 10 + ch - '0';
                        if (!TryMoveNext(format, ref position, out ch))
                            goto FailureUnclosedFormatItem;
                    }

                    while (ch == ' ')
                    {
                        if (!TryMoveNext(format, ref position, out ch))
                            goto FailureUnclosedFormatItem;
                    }

                    if (ch == ',')
                    {
                        do
                        {
                            if (!TryMoveNext(format, ref position, out ch))
                                goto FailureUnclosedFormatItem;
                        } while (ch == ' ');

                        var leftJustify = 1;
                        if (ch == '-')
                        {
                            leftJustify = -1;
                            if (!TryMoveNext(format, ref position, out ch))
                                goto FailureUnclosedFormatItem;
                        }

                        width = ch - '0';
                        if ((uint)width >= 10u)
                            goto FailureExpectedAsciiDigit;
                        if (!TryMoveNext(format, ref position, out ch))
                            goto FailureUnclosedFormatItem;
                        while (CharHelpers.IsAsciiDigit(ch))
                        {
                            width = width * 10 + ch - '0';
                            if (!TryMoveNext(format, ref position, out ch))
                                goto FailureUnclosedFormatItem;
                        }

                        width *= leftJustify;
                        while (ch == ' ')
                        {
                            if (!TryMoveNext(format, ref position, out ch))
                                goto FailureUnclosedFormatItem;
                        }
                    }

                    if (ch != '}')
                    {
                        if (ch != ':')
                            goto FailureUnclosedFormatItem;
                        var startingPosition = position;
                        while (true)
                        {
                            if (!TryMoveNext(format, ref position, out ch))
                                goto FailureUnclosedFormatItem;
                            if (ch == '}')
                                break;
                            if (ch == '{')
                                goto FailureUnclosedFormatItem;
                        }

                        startingPosition++;
                        itemFormat = format.Slice(startingPosition, position - startingPosition).ToString();
                    }
                }

                position++;
                segments.Append((null, index, width, itemFormat));
            }

            FailureUnexpectedClosingBrace:
            failureReason = InvalidFormatReason.UnexpectedClosingBrace;
            failureOffset = position;
            return false;

            FailureUnclosedFormatItem:
            failureReason = InvalidFormatReason.UnclosedFormatItem;
            failureOffset = position;
            return false;

            FailureExpectedAsciiDigit:
            failureReason = InvalidFormatReason.ExpectedAsciiDigit;
            failureOffset = position;
            return false;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool TryMoveNext(ReadOnlySpan<char> format, ref int position, out char nextChar)
            {
                position++;
                if ((uint)position >= (uint)format.Length)
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