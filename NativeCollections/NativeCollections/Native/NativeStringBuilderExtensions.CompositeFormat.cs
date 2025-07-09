using System;
using System.Runtime.CompilerServices;

#pragma warning disable CA2208
#pragma warning disable CS8500
#pragma warning disable CS8632
#pragma warning disable CS9080

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native string builder extensions
    /// </summary>
    public static unsafe partial class NativeStringBuilderExtensions
    {
        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(1);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(2);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(3);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(4);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(5);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(6);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(7);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(8);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(9);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(10);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(11);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(12);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(13);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(14);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(15);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this NativeStringBuilder<char> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(16);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(1);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(2);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(3);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(4);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(5);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(6);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(7);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(8);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(9);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(10);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(11);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(12);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(13);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(14);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(15);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this NativeStringBuilder<char> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            format.ValidateNumberOfArgs(16);
            var handler = new NativeStringBuilderInterpolatedStringHandler(format.LiteralLength, format.FormattedCount, builder, provider);
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

            ref var builderRef = ref builder.AsRef();
            builderRef = handler.StringBuilder;
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3, arg4);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3, arg4, arg5);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this NativeStringBuilder<byte> builder, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2, arg3);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2, arg3, arg4);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2, arg3, arg4, arg5);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
                builderRef.Append(temp);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider, NativeCompositeFormat format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ref var builderRef = ref builder.AsRef();
            using (var temp = new NativeStringBuilder<char>(stackalloc char[512], 0))
            {
                temp.AppendFormat(provider, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
                builderRef.Append(temp);
            }
        }
    }
}