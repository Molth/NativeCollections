using System;

// ReSharper disable ALL

namespace NativeCollections
{
    public static partial class UnsafeStringBuilderExtensions
    {
        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(1);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(2);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(3);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(4);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(5);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(6);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(7);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(8);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(9);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(10);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(11);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(12);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(13);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <typeparam name="T13">The type of the fourteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <param name="arg13">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(14);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;

                        case 13:
                            handler.AppendFormatted(arg13, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <typeparam name="T13">The type of the fourteenth argument to format.</typeparam>
        /// <typeparam name="T14">The type of the fifteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <param name="arg13">The argument to format.</param>
        /// <param name="arg14">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(15);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;

                        case 13:
                            handler.AppendFormatted(arg13, segment.Alignment, segment.Format);
                            break;

                        case 14:
                            handler.AppendFormatted(arg14, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <typeparam name="T13">The type of the fourteenth argument to format.</typeparam>
        /// <typeparam name="T14">The type of the fifteenth argument to format.</typeparam>
        /// <typeparam name="T15">The type of the sixteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <param name="arg13">The argument to format.</param>
        /// <param name="arg14">The argument to format.</param>
        /// <param name="arg15">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this UnsafeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(16);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;

                        case 13:
                            handler.AppendFormatted(arg13, segment.Alignment, segment.Format);
                            break;

                        case 14:
                            handler.AppendFormatted(arg14, segment.Alignment, segment.Format);
                            break;

                        case 15:
                            handler.AppendFormatted(arg15, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(1);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(2);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(3);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(4);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(5);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(6);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(7);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(8);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(9);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(10);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(11);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(12);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(13);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <typeparam name="T13">The type of the fourteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <param name="arg13">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(14);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;

                        case 13:
                            handler.AppendFormatted(arg13, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <typeparam name="T13">The type of the fourteenth argument to format.</typeparam>
        /// <typeparam name="T14">The type of the fifteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <param name="arg13">The argument to format.</param>
        /// <param name="arg14">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(15);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;

                        case 13:
                            handler.AppendFormatted(arg13, segment.Alignment, segment.Format);
                            break;

                        case 14:
                            handler.AppendFormatted(arg14, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <typeparam name="T13">The type of the fourteenth argument to format.</typeparam>
        /// <typeparam name="T14">The type of the fifteenth argument to format.</typeparam>
        /// <typeparam name="T15">The type of the sixteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <param name="arg13">The argument to format.</param>
        /// <param name="arg14">The argument to format.</param>
        /// <param name="arg15">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(16);
            var handler = new UnsafeStringBuilderUtf16InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;

                        case 13:
                            handler.AppendFormatted(arg13, segment.Alignment, segment.Format);
                            break;

                        case 14:
                            handler.AppendFormatted(arg14, segment.Alignment, segment.Format);
                            break;

                        case 15:
                            handler.AppendFormatted(arg15, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(1);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(2);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(3);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(4);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(5);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(6);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(7);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(8);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(9);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(10);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(11);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(12);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(13);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <typeparam name="T13">The type of the fourteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <param name="arg13">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(14);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;

                        case 13:
                            handler.AppendFormatted(arg13, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <typeparam name="T13">The type of the fourteenth argument to format.</typeparam>
        /// <typeparam name="T14">The type of the fifteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <param name="arg13">The argument to format.</param>
        /// <param name="arg14">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(15);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;

                        case 13:
                            handler.AppendFormatted(arg13, segment.Alignment, segment.Format);
                            break;

                        case 14:
                            handler.AppendFormatted(arg14, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <typeparam name="T13">The type of the fourteenth argument to format.</typeparam>
        /// <typeparam name="T14">The type of the fifteenth argument to format.</typeparam>
        /// <typeparam name="T15">The type of the sixteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <param name="arg13">The argument to format.</param>
        /// <param name="arg14">The argument to format.</param>
        /// <param name="arg15">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this UnsafeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(16);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;

                        case 13:
                            handler.AppendFormatted(arg13, segment.Alignment, segment.Format);
                            break;

                        case 14:
                            handler.AppendFormatted(arg14, segment.Alignment, segment.Format);
                            break;

                        case 15:
                            handler.AppendFormatted(arg15, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(1);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(2);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(3);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(4);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(5);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(6);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(7);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(8);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(9);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(10);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(11);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(12);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(13);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <typeparam name="T13">The type of the fourteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <param name="arg13">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(14);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;

                        case 13:
                            handler.AppendFormatted(arg13, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <typeparam name="T13">The type of the fourteenth argument to format.</typeparam>
        /// <typeparam name="T14">The type of the fifteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <param name="arg13">The argument to format.</param>
        /// <param name="arg14">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(15);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;

                        case 13:
                            handler.AppendFormatted(arg13, segment.Alignment, segment.Format);
                            break;

                        case 14:
                            handler.AppendFormatted(arg14, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string as UTF-8 encoded bytes, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <typeparam name="T4">The type of the fifth argument to format.</typeparam>
        /// <typeparam name="T5">The type of the sixth argument to format.</typeparam>
        /// <typeparam name="T6">The type of the seventh argument to format.</typeparam>
        /// <typeparam name="T7">The type of the eighth argument to format.</typeparam>
        /// <typeparam name="T8">The type of the ninth argument to format.</typeparam>
        /// <typeparam name="T9">The type of the tenth argument to format.</typeparam>
        /// <typeparam name="T10">The type of the eleventh argument to format.</typeparam>
        /// <typeparam name="T11">The type of the twelfth argument to format.</typeparam>
        /// <typeparam name="T12">The type of the thirteenth argument to format.</typeparam>
        /// <typeparam name="T13">The type of the fourteenth argument to format.</typeparam>
        /// <typeparam name="T14">The type of the fifteenth argument to format.</typeparam>
        /// <typeparam name="T15">The type of the sixteenth argument to format.</typeparam>
        /// <param name="builder">The string builder to which to append the formatted string as UTF-8 encoded bytes.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The parsed composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <param name="arg9">The argument to format.</param>
        /// <param name="arg10">The argument to format.</param>
        /// <param name="arg11">The argument to format.</param>
        /// <param name="arg12">The argument to format.</param>
        /// <param name="arg13">The argument to format.</param>
        /// <param name="arg14">The argument to format.</param>
        /// <param name="arg15">The argument to format.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="format" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">Thrown when an insufficient number of arguments is provided.</exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ThrowHelpers.ThrowIfNull(format, ExceptionArgument.format);
            format.ValidateNumberOfArgs(16);
            var handler = new UnsafeStringBuilderUtf8InterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
            foreach (var segment in format.Segments)
            {
                var literal = segment.Literal;
                if (literal != null)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    var index = segment.ArgIndex;
                    switch (index)
                    {
                        case 0:
                            handler.AppendFormatted(arg0, segment.Alignment, segment.Format);
                            break;

                        case 1:
                            handler.AppendFormatted(arg1, segment.Alignment, segment.Format);
                            break;

                        case 2:
                            handler.AppendFormatted(arg2, segment.Alignment, segment.Format);
                            break;

                        case 3:
                            handler.AppendFormatted(arg3, segment.Alignment, segment.Format);
                            break;

                        case 4:
                            handler.AppendFormatted(arg4, segment.Alignment, segment.Format);
                            break;

                        case 5:
                            handler.AppendFormatted(arg5, segment.Alignment, segment.Format);
                            break;

                        case 6:
                            handler.AppendFormatted(arg6, segment.Alignment, segment.Format);
                            break;

                        case 7:
                            handler.AppendFormatted(arg7, segment.Alignment, segment.Format);
                            break;

                        case 8:
                            handler.AppendFormatted(arg8, segment.Alignment, segment.Format);
                            break;

                        case 9:
                            handler.AppendFormatted(arg9, segment.Alignment, segment.Format);
                            break;

                        case 10:
                            handler.AppendFormatted(arg10, segment.Alignment, segment.Format);
                            break;

                        case 11:
                            handler.AppendFormatted(arg11, segment.Alignment, segment.Format);
                            break;

                        case 12:
                            handler.AppendFormatted(arg12, segment.Alignment, segment.Format);
                            break;

                        case 13:
                            handler.AppendFormatted(arg13, segment.Alignment, segment.Format);
                            break;

                        case 14:
                            handler.AppendFormatted(arg14, segment.Alignment, segment.Format);
                            break;

                        case 15:
                            handler.AppendFormatted(arg15, segment.Alignment, segment.Format);
                            break;
                    }
                }
            }
        }
    }
}