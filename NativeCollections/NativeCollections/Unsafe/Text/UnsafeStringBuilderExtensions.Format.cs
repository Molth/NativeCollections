using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(1))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(2))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(3))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(4))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(5))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(6))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(7))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(8))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(9))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(10))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(11))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(12))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(13))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(14))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;

                    case 13:
                        Append(ref builderRef, itemFormatSpan2, arg13, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(15))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;

                    case 13:
                        Append(ref builderRef, itemFormatSpan2, arg13, width, leftJustify);
                        break;

                    case 14:
                        Append(ref builderRef, itemFormatSpan2, arg14, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(16))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;

                    case 13:
                        Append(ref builderRef, itemFormatSpan2, arg13, width, leftJustify);
                        break;

                    case 14:
                        Append(ref builderRef, itemFormatSpan2, arg14, width, leftJustify);
                        break;

                    case 15:
                        Append(ref builderRef, itemFormatSpan2, arg15, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(1))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(2))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(3))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(4))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(5))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(6))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(7))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(8))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(9))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(10))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(11))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(12))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(13))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(14))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;

                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg13, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(15))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;

                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg13, width, leftJustify);
                        break;

                    case 14:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg14, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(16))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;

                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg13, width, leftJustify);
                        break;

                    case 14:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg14, width, leftJustify);
                        break;

                    case 15:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg15, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(1))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(2))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(3))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(4))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(5))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(6))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(7))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(8))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(9))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(10))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(11))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(12))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(13))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(14))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;

                    case 13:
                        Append(ref builderRef, itemFormatSpan2, arg13, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(15))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;

                    case 13:
                        Append(ref builderRef, itemFormatSpan2, arg13, width, leftJustify);
                        break;

                    case 14:
                        Append(ref builderRef, itemFormatSpan2, arg14, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(16))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;

                    case 13:
                        Append(ref builderRef, itemFormatSpan2, arg13, width, leftJustify);
                        break;

                    case 14:
                        Append(ref builderRef, itemFormatSpan2, arg14, width, leftJustify);
                        break;

                    case 15:
                        Append(ref builderRef, itemFormatSpan2, arg15, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(1))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(2))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(3))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(4))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(5))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(6))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(7))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(8))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="arg4">The argument to format.</param>
        /// <param name="arg5">The argument to format.</param>
        /// <param name="arg6">The argument to format.</param>
        /// <param name="arg7">The argument to format.</param>
        /// <param name="arg8">The argument to format.</param>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(9))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(10))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(11))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(12))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(13))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(14))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;

                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg13, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(15))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;

                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg13, width, leftJustify);
                        break;

                    case 14:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg14, width, leftJustify);
                        break;
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
        /// <param name="format">The composite format string.</param>
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
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is invalid or the index of a
        ///     format item is greater than or equal to the number of arguments supplied.
        /// </exception>
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(16))
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;

                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;

                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;

                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;

                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;

                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;

                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;

                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;

                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;

                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;

                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;

                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;

                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;

                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg13, width, leftJustify);
                        break;

                    case 14:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg14, width, leftJustify);
                        break;

                    case 15:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg15, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Parses the next format item from the format string, appending literal text to the builder,
        ///     and extracts the argument index, alignment, formatting string, and alignment direction.
        /// </summary>
        /// <param name="builderRef">Reference to the string builder to which literal text is appended.</param>
        /// <param name="format">The format string containing placeholders.</param>
        /// <param name="position">The current position in the format string; updated to after the parsed item.</param>
        /// <param name="width">The alignment width (positive) or 0 if not specified.</param>
        /// <param name="leftJustify"><see langword="true" /> if left‑justified; otherwise, right‑justified.</param>
        /// <param name="itemFormatSpan1">The format specifier for the argument (after ':'), or empty if none.</param>
        /// <param name="index">The zero‑based argument index of the placeholder.</param>
        /// <returns>
        ///     <see langword="true" /> if a valid format item was parsed;
        ///     <see langword="false" /> if the end of the string was reached.
        /// </returns>
        /// <exception cref="FormatException">Thrown when the format string is malformed.</exception>
        private static bool ParseFormat(ref UnsafeStringBuilder<char> builderRef, ReadOnlySpan<char> format, ref int position, out int width, out bool leftJustify, out ReadOnlySpan<char> itemFormatSpan1, out int index)
        {
            index = default;
            leftJustify = default;
            itemFormatSpan1 = default;
            width = default;
            char ch;
            while (true)
            {
                if (U32.FromI32Unchecked(position) >= U32.FromI32Unchecked(format.Length))
                    return false;

                var remainder = format.Slice(position);
                var countUntilNextBrace = remainder.IndexOfAny('{', '}');
                if (countUntilNextBrace < 0)
                {
                    builderRef.Append(remainder);
                    return false;
                }

                builderRef.Append(remainder.Slice(0, countUntilNextBrace));
                position = position.WrappingAdd(countUntilNextBrace);
                var brace = format[position];
                ch = MoveNext(format, ref position);
                if (I32.FromCharUnchecked(brace) == I32.FromCharUnchecked(ch))
                {
                    builderRef.Append(ch);
                    position = position.WrappingAdd(1);
                    continue;
                }

                if (I32.FromCharUnchecked(brace) != I32.FromCharUnchecked('{'))
                    ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnexpectedClosingBrace);

                break;
            }

            width = 0;
            leftJustify = false;
            itemFormatSpan1 = default;
            index = ch.WrappingSub('0');
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(10))
                ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_ExpectedAsciiDigit);

            ch = MoveNext(format, ref position);
            if (I32.FromCharUnchecked(ch) != I32.FromCharUnchecked('}'))
            {
                while (CharHelpers.IsAsciiDigit(ch) && index < 1_000_000)
                {
                    index = index.WrappingMul(10).WrappingAdd(I32.FromCharUnchecked(ch)).WrappingSub(I32.FromCharUnchecked('0'));
                    ch = MoveNext(format, ref position);
                }

                while (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked(' '))
                    ch = MoveNext(format, ref position);

                if (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked(','))
                {
                    while (true)
                    {
                        ch = MoveNext(format, ref position);

                        if (I32.FromCharUnchecked(ch) != I32.FromCharUnchecked(' '))
                            break;
                    }

                    if (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked('-'))
                    {
                        leftJustify = true;
                        ch = MoveNext(format, ref position);
                    }

                    width = ch.WrappingSub('0');
                    if (U32.FromI32Unchecked(width) >= U32.FromI32Unchecked(10))
                        ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_ExpectedAsciiDigit);

                    ch = MoveNext(format, ref position);
                    while (CharHelpers.IsAsciiDigit(ch) && width < 1_000_000)
                    {
                        width = width.WrappingMul(10).WrappingAdd(I32.FromCharUnchecked(ch)).WrappingSub(I32.FromCharUnchecked('0'));
                        ch = MoveNext(format, ref position);
                    }

                    while (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked(' '))
                        ch = MoveNext(format, ref position);
                }

                if (I32.FromCharUnchecked(ch) != I32.FromCharUnchecked('}'))
                {
                    if (I32.FromCharUnchecked(ch) != I32.FromCharUnchecked(':'))
                        ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnclosedFormatItem);

                    var startingPosition = position;
                    while (true)
                    {
                        ch = MoveNext(format, ref position);
                        if (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked('}'))
                            break;

                        if (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked('{'))
                            ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnclosedFormatItem);
                    }

                    startingPosition = startingPosition.WrappingAdd(1);
                    itemFormatSpan1 = format.Slice(startingPosition, position.WrappingSub(startingPosition));
                }
            }

            position = position.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Appends the formatted representation of an argument to
        ///     the builder, considering alignment and format specifier.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="builderRef">Reference to the string builder to append to.</param>
        /// <param name="itemFormatSpan1">The format specifier for the argument, or empty for default.</param>
        /// <param name="arg">The argument to format and append.</param>
        /// <param name="width">The alignment width; positive for fixed width, 0 for none.</param>
        /// <param name="leftJustify">
        ///     <see langword="true" /> to left‑justify (pad right);
        ///     <see langword="false" /> to right‑justify (pad left).
        /// </param>
        [SkipLocalsInit]
        private static void Append<T>(ref UnsafeStringBuilder<char> builderRef, ReadOnlySpan<char> itemFormatSpan1, T? arg, int width, bool leftJustify)
        {
            int charsWritten;
            if (leftJustify)
            {
                while (!FormatHelpers.TryFormat(arg, builderRef.Space, out charsWritten, itemFormatSpan1))
                    builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

                builderRef.Advance(charsWritten);
                var padding = width.WrappingSub(charsWritten);
                if (width > 0 && padding > 0)
                    builderRef.Append(' ', padding);
            }
            else
            {
                if (arg != null)
                {
                    ReadOnlySpan<char> obj;
                    if (typeof(T) == typeof(string))
                        obj = Unsafe.As<T?, string>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ArraySegment<char>))
                        obj = Unsafe.As<T, ArraySegment<char>>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ReadOnlyMemory<char>))
                        obj = Unsafe.As<T, ReadOnlyMemory<char>>(ref arg).Span;
                    else if (typeof(T) == typeof(Memory<char>))
                        obj = Unsafe.As<T, Memory<char>>(ref arg).Span;
                    else
                        goto label;

                    charsWritten = obj.Length;
                    var padding = width.WrappingSub(charsWritten);
                    if (padding > 0)
                        builderRef.Append(' ', padding);

                    builderRef.Append(obj);
                    return;
                }

                label:
                using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
                {
                    while (!FormatHelpers.TryFormat(arg, temp.Space, out charsWritten, itemFormatSpan1))
                        temp.EnsureCapacity(temp.Capacity.WrappingAdd(1));

                    temp.Advance(charsWritten);
                    var padding2 = width.WrappingSub(charsWritten);
                    if (padding2 > 0)
                        builderRef.Append(' ', padding2);

                    var buffer1 = temp.AsReadOnlySpan();
                    var buffer2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(buffer1), buffer1.Length);
                    builderRef.Append(buffer2);
                }
            }
        }

        /// <summary>
        ///     Appends the formatted representation of an argument to the builder,
        ///     using the provided format provider and optional custom formatter.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="builderRef">Reference to the string builder to append to.</param>
        /// <param name="provider">
        ///     The <see cref="IFormatProvider" /> to use for formatting; may be <see langword="null" />.
        /// </param>
        /// <param name="customFormatter">
        ///     A custom <see cref="ICustomFormatter" />; may be <see langword="null" />.
        /// </param>
        /// <param name="itemFormat">A cached string for the format specifier; may be updated internally.</param>
        /// <param name="itemFormatSpan1">The format specifier span.</param>
        /// <param name="arg">The argument to format and append.</param>
        /// <param name="width">The alignment width; positive for fixed width, 0 for none.</param>
        /// <param name="leftJustify">
        ///     <see langword="true" /> to left‑justify (pad right);
        ///     <see langword="false" /> to right‑justify (pad left).
        /// </param>
        /// <remarks>
        ///     When a custom formatter is present but returns <see langword="null" />, the fallback
        ///     order matches the BCL: <c>ISpanFormattable</c> first, then
        ///     <see cref="IFormattable" />, and finally <c>ToString()</c>.
        /// </remarks>
        [SkipLocalsInit]
        private static void Append<T>(ref UnsafeStringBuilder<char> builderRef, IFormatProvider? provider, ICustomFormatter? customFormatter, ref string? itemFormat, ReadOnlySpan<char> itemFormatSpan1, T? arg, int width, bool leftJustify)
        {
            if (customFormatter != null)
            {
                string? format = null;
                if (!itemFormatSpan1.IsEmpty)
                {
                    if (itemFormat == null || !itemFormat.AsSpan().SequenceEqual(itemFormatSpan1))
                        itemFormat = new string(itemFormatSpan1);

                    format = itemFormat;
                }

                var result = customFormatter.Format(format, arg, provider);
                if (result == null)
                {
#if NET6_0_OR_GREATER
                    if (arg is ISpanFormattable spanFormattable)
                    {
                        using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
                        {
                            int spanCharsWritten;
                            while (!spanFormattable.TryFormat(temp.Space, out spanCharsWritten, itemFormatSpan1, provider))
                                temp.EnsureCapacity(temp.Capacity.WrappingAdd(1));

                            temp.Advance(spanCharsWritten);
                            result = new string(temp.AsReadOnlySpan());
                        }
                    }
                    else
#endif
                    {
                        result = arg is IFormattable formattable ? formattable.ToString(format, provider) : arg?.ToString();
                    }
                }

                var obj = (result ?? "").AsSpan();
                if (width <= obj.Length)
                {
                    builderRef.Append(obj);
                }
                else if (leftJustify)
                {
                    builderRef.Append(obj);
                    builderRef.Append(' ', width.WrappingSub(obj.Length));
                }
                else
                {
                    builderRef.Append(' ', width.WrappingSub(obj.Length));
                    builderRef.Append(obj);
                }

                return;
            }

            int charsWritten;
            if (leftJustify)
            {
                while (!FormatHelpers.TryFormat(arg, builderRef.Space, out charsWritten, itemFormatSpan1, provider))
                    builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

                builderRef.Advance(charsWritten);
                var padding = width.WrappingSub(charsWritten);
                if (width > 0 && padding > 0)
                    builderRef.Append(' ', padding);
            }
            else
            {
                if (arg != null)
                {
                    ReadOnlySpan<char> obj;
                    if (typeof(T) == typeof(string))
                        obj = Unsafe.As<T?, string>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ArraySegment<char>))
                        obj = Unsafe.As<T, ArraySegment<char>>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ReadOnlyMemory<char>))
                        obj = Unsafe.As<T, ReadOnlyMemory<char>>(ref arg).Span;
                    else if (typeof(T) == typeof(Memory<char>))
                        obj = Unsafe.As<T, Memory<char>>(ref arg).Span;
                    else
                        goto label;

                    charsWritten = obj.Length;
                    var padding = width.WrappingSub(charsWritten);
                    if (padding > 0)
                        builderRef.Append(' ', padding);

                    builderRef.Append(obj);
                    return;
                }

                label:
                using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
                {
                    while (!FormatHelpers.TryFormat(arg, temp.Space, out charsWritten, itemFormatSpan1, provider))
                        temp.EnsureCapacity(temp.Capacity.WrappingAdd(1));

                    temp.Advance(charsWritten);
                    var padding2 = width.WrappingSub(charsWritten);
                    if (padding2 > 0)
                        builderRef.Append(' ', padding2);

                    var buffer1 = temp.AsReadOnlySpan();
                    var buffer2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(buffer1), buffer1.Length);
                    builderRef.Append(buffer2);
                }
            }
        }

        /// <summary>
        ///     Parses the next format item from the format string, appending literal UTF‑8 text to the
        ///     builder, and extracts the argument index, alignment, formatting string, and alignment direction.
        /// </summary>
        /// <param name="builderRef">Reference to the UTF‑8 string builder to which literal text is appended.</param>
        /// <param name="format">The format string containing placeholders.</param>
        /// <param name="position">The current position in the format string; updated to after the parsed item.</param>
        /// <param name="width">The alignment width (positive) or 0 if not specified.</param>
        /// <param name="leftJustify"><see langword="true" /> if left‑justified; otherwise, right‑justified.</param>
        /// <param name="itemFormatSpan1">The format specifier for the argument (after ':'), or empty if none.</param>
        /// <param name="index">The zero‑based argument index of the placeholder.</param>
        /// <returns>
        ///     <see langword="true" /> if a valid format item was parsed;
        ///     <see langword="false" /> if the end of the string was reached.
        /// </returns>
        /// <exception cref="FormatException">Thrown when the format string is malformed.</exception>
        private static bool ParseFormat(ref UnsafeStringBuilder<byte> builderRef, ReadOnlySpan<char> format, ref int position, out int width, out bool leftJustify, out ReadOnlySpan<char> itemFormatSpan1, out int index)
        {
            index = default;
            leftJustify = default;
            itemFormatSpan1 = default;
            width = default;
            char ch;
            while (true)
            {
                if (U32.FromI32Unchecked(position) >= U32.FromI32Unchecked(format.Length))
                    return false;

                var remainder = format.Slice(position);
                var countUntilNextBrace = remainder.IndexOfAny('{', '}');
                if (countUntilNextBrace < 0)
                {
                    builderRef.Append(remainder);
                    return false;
                }

                builderRef.Append(remainder.Slice(0, countUntilNextBrace));
                position = position.WrappingAdd(countUntilNextBrace);
                var brace = format[position];
                ch = MoveNext(format, ref position);
                if (I32.FromCharUnchecked(brace) == I32.FromCharUnchecked(ch))
                {
                    builderRef.Append(ch);
                    position = position.WrappingAdd(1);
                    continue;
                }

                if (I32.FromCharUnchecked(brace) != I32.FromCharUnchecked('{'))
                    ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnexpectedClosingBrace);

                break;
            }

            width = 0;
            leftJustify = false;
            itemFormatSpan1 = default;
            index = ch.WrappingSub('0');
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(10))
                ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_ExpectedAsciiDigit);

            ch = MoveNext(format, ref position);
            if (I32.FromCharUnchecked(ch) != I32.FromCharUnchecked('}'))
            {
                while (CharHelpers.IsAsciiDigit(ch) && index < 1_000_000)
                {
                    index = index.WrappingMul(10).WrappingAdd(I32.FromCharUnchecked(ch)).WrappingSub(I32.FromCharUnchecked('0'));
                    ch = MoveNext(format, ref position);
                }

                while (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked(' '))
                    ch = MoveNext(format, ref position);

                if (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked(','))
                {
                    while (true)
                    {
                        ch = MoveNext(format, ref position);

                        if (I32.FromCharUnchecked(ch) != I32.FromCharUnchecked(' '))
                            break;
                    }

                    if (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked('-'))
                    {
                        leftJustify = true;
                        ch = MoveNext(format, ref position);
                    }

                    width = ch.WrappingSub('0');
                    if (U32.FromI32Unchecked(width) >= U32.FromI32Unchecked(10))
                        ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_ExpectedAsciiDigit);

                    ch = MoveNext(format, ref position);
                    while (CharHelpers.IsAsciiDigit(ch) && width < 1_000_000)
                    {
                        width = width.WrappingMul(10).WrappingAdd(I32.FromCharUnchecked(ch)).WrappingSub(I32.FromCharUnchecked('0'));
                        ch = MoveNext(format, ref position);
                    }

                    while (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked(' '))
                        ch = MoveNext(format, ref position);
                }

                if (I32.FromCharUnchecked(ch) != I32.FromCharUnchecked('}'))
                {
                    if (I32.FromCharUnchecked(ch) != I32.FromCharUnchecked(':'))
                        ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnclosedFormatItem);

                    var startingPosition = position;
                    while (true)
                    {
                        ch = MoveNext(format, ref position);
                        if (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked('}'))
                            break;

                        if (I32.FromCharUnchecked(ch) == I32.FromCharUnchecked('{'))
                            ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnclosedFormatItem);
                    }

                    startingPosition = startingPosition.WrappingAdd(1);
                    itemFormatSpan1 = format.Slice(startingPosition, position.WrappingSub(startingPosition));
                }
            }

            position = position.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Appends the UTF‑8 formatted representation of an argument
        ///     to the builder, considering alignment and format specifier.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="builderRef">Reference to the UTF‑8 string builder to append to.</param>
        /// <param name="itemFormatSpan1">The format specifier for the argument, or empty for default.</param>
        /// <param name="arg">The argument to format and append.</param>
        /// <param name="width">The alignment width; positive for fixed width, 0 for none.</param>
        /// <param name="leftJustify">
        ///     <see langword="true" /> to left‑justify (pad right);
        ///     <see langword="false" /> to right‑justify (pad left).
        /// </param>
        [SkipLocalsInit]
        private static void Append<T>(ref UnsafeStringBuilder<byte> builderRef, ReadOnlySpan<char> itemFormatSpan1, T? arg, int width, bool leftJustify)
        {
            int bytesWritten;
            int charsWritten;
            if (leftJustify)
            {
                while (!Utf8FormatHelpers.TryFormat(arg, builderRef.Space, out bytesWritten, itemFormatSpan1))
                    builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

                charsWritten = Encoding.UTF8.GetCharCount(builderRef.GetSpan(bytesWritten));
                builderRef.Advance(bytesWritten);
                var padding1 = width.WrappingSub(charsWritten);
                if (width > 0 && padding1 > 0)
                    builderRef.Append(' ', padding1);
            }
            else
            {
                if (arg != null)
                {
                    ReadOnlySpan<char> obj;
                    if (typeof(T) == typeof(string))
                        obj = Unsafe.As<T?, string>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ArraySegment<char>))
                        obj = Unsafe.As<T, ArraySegment<char>>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ReadOnlyMemory<char>))
                        obj = Unsafe.As<T, ReadOnlyMemory<char>>(ref arg).Span;
                    else if (typeof(T) == typeof(Memory<char>))
                        obj = Unsafe.As<T, Memory<char>>(ref arg).Span;
                    else
                        goto label;

                    charsWritten = obj.Length;
                    var padding = width.WrappingSub(charsWritten);
                    if (padding > 0)
                        builderRef.Append(' ', padding);

                    builderRef.Append(obj);
                    return;
                }

                label:
                using (var temp = UnsafeStringBuilder<byte>.From(stackalloc byte[1024], 0))
                {
                    while (!Utf8FormatHelpers.TryFormat(arg, temp.Space, out bytesWritten, itemFormatSpan1))
                        temp.EnsureCapacity(temp.Capacity.WrappingAdd(1));

                    charsWritten = Encoding.UTF8.GetCharCount(temp.GetSpan(bytesWritten));
                    temp.Advance(bytesWritten);
                    var padding2 = width.WrappingSub(charsWritten);
                    if (padding2 > 0)
                        builderRef.Append(' ', padding2);

                    var buffer1 = temp.AsReadOnlySpan();
                    var buffer2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(buffer1), buffer1.Length);
                    builderRef.Append(buffer2);
                }
            }
        }

        /// <summary>
        ///     Appends the UTF‑8 formatted representation of an argument to the builder,
        ///     using the provided format provider and optional custom formatter.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="builderRef">Reference to the UTF‑8 string builder to append to.</param>
        /// <param name="provider">
        ///     The <see cref="IFormatProvider" /> to use for formatting; may be <see langword="null" />.
        /// </param>
        /// <param name="customFormatter">
        ///     A custom <see cref="ICustomFormatter" />; may be <see langword="null" />.
        /// </param>
        /// <param name="itemFormat">A cached string for the format specifier; may be updated internally.</param>
        /// <param name="itemFormatSpan1">The format specifier span.</param>
        /// <param name="arg">The argument to format and append.</param>
        /// <param name="width">The alignment width; positive for fixed width, 0 for none.</param>
        /// <param name="leftJustify">
        ///     <see langword="true" /> to left‑justify (pad right);
        ///     <see langword="false" /> to right‑justify (pad left).
        /// </param>
        /// <remarks>
        ///     When a custom formatter is present but returns <see langword="null" />, the fallback
        ///     order matches the BCL: <c>ISpanFormattable</c> first, then
        ///     <see cref="IFormattable" />, and finally <c>ToString()</c>.
        /// </remarks>
        [SkipLocalsInit]
        private static void Append<T>(ref UnsafeStringBuilder<byte> builderRef, IFormatProvider? provider, ICustomFormatter? customFormatter, ref string? itemFormat, ReadOnlySpan<char> itemFormatSpan1, T? arg, int width, bool leftJustify)
        {
            if (customFormatter != null)
            {
                string? format = null;
                if (!itemFormatSpan1.IsEmpty)
                {
                    if (itemFormat == null || !itemFormat.AsSpan().SequenceEqual(itemFormatSpan1))
                        itemFormat = new string(itemFormatSpan1);

                    format = itemFormat;
                }

                var result = customFormatter.Format(format, arg, provider);
                if (result == null)
                {
#if NET6_0_OR_GREATER
                    if (arg is ISpanFormattable)
                    {
                        using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
                        {
                            int spanCharsWritten;
                            while (!FormatHelpers.TryFormat(arg, temp.Space, out spanCharsWritten, itemFormatSpan1, provider))
                                temp.EnsureCapacity(temp.Capacity.WrappingAdd(1));

                            temp.Advance(spanCharsWritten);
                            result = new string(temp.AsReadOnlySpan());
                        }
                    }
                    else
#endif
                    {
                        result = arg is IFormattable formattable ? formattable.ToString(format, provider) : arg?.ToString();
                    }
                }

                var obj = (result ?? "").AsSpan();
                if (width <= obj.Length)
                {
                    builderRef.Append(obj);
                }
                else if (leftJustify)
                {
                    builderRef.Append(obj);
                    builderRef.Append(' ', width.WrappingSub(obj.Length));
                }
                else
                {
                    builderRef.Append(' ', width.WrappingSub(obj.Length));
                    builderRef.Append(obj);
                }

                return;
            }

            int bytesWritten;
            int charsWritten;
            if (leftJustify)
            {
                while (!Utf8FormatHelpers.TryFormat(arg, builderRef.Space, out bytesWritten, itemFormatSpan1, provider))
                    builderRef.EnsureCapacity(builderRef.Capacity.WrappingAdd(1));

                charsWritten = Encoding.UTF8.GetCharCount(builderRef.GetSpan(bytesWritten));
                builderRef.Advance(bytesWritten);
                var padding1 = width.WrappingSub(charsWritten);
                if (width > 0 && padding1 > 0)
                    builderRef.Append(' ', padding1);
            }
            else
            {
                if (arg != null)
                {
                    ReadOnlySpan<char> obj;
                    if (typeof(T) == typeof(string))
                        obj = Unsafe.As<T?, string>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ArraySegment<char>))
                        obj = Unsafe.As<T, ArraySegment<char>>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ReadOnlyMemory<char>))
                        obj = Unsafe.As<T, ReadOnlyMemory<char>>(ref arg).Span;
                    else if (typeof(T) == typeof(Memory<char>))
                        obj = Unsafe.As<T, Memory<char>>(ref arg).Span;
                    else
                        goto label;

                    charsWritten = obj.Length;
                    var padding = width.WrappingSub(charsWritten);
                    if (padding > 0)
                        builderRef.Append(' ', padding);

                    builderRef.Append(obj);
                    return;
                }

                label:
                using (var temp = UnsafeStringBuilder<byte>.From(stackalloc byte[1024], 0))
                {
                    while (!Utf8FormatHelpers.TryFormat(arg, temp.Space, out bytesWritten, itemFormatSpan1, provider))
                        temp.EnsureCapacity(temp.Capacity.WrappingAdd(1));

                    charsWritten = Encoding.UTF8.GetCharCount(temp.GetSpan(bytesWritten));
                    temp.Advance(bytesWritten);
                    var padding2 = width.WrappingSub(charsWritten);
                    if (padding2 > 0)
                        builderRef.Append(' ', padding2);

                    var buffer1 = temp.AsReadOnlySpan();
                    var buffer2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(buffer1), buffer1.Length);
                    builderRef.Append(buffer2);
                }
            }
        }

        /// <summary>
        ///     Advances the position by one character, checks for
        ///     end‑of‑string, and returns the character at the new position.
        /// </summary>
        /// <param name="format">The format string being parsed.</param>
        /// <param name="position">The current position; updated to the next character.</param>
        /// <returns>The character at the new position.</returns>
        /// <exception cref="FormatException">
        ///     Thrown when the end of the string is reached unexpectedly (missing closing brace).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char MoveNext(ReadOnlySpan<char> format, ref int position)
        {
            position = position.WrappingAdd(1);
            if (U32.FromI32Unchecked(position) >= U32.FromI32Unchecked(format.Length))
                ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnclosedFormatItem);

            return format[position];
        }
    }
}