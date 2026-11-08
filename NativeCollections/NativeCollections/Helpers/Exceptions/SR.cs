using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Contains constant string resources used for exception messages and formatting.
    /// </summary>
    internal static class SR
    {
        /// <summary>
        ///     The message used when an obsolete parameter validation path is reached.
        /// </summary>
        public const string parameter_obsolete = "Call this method will always throw an exception.";

        /// <summary>
        ///     The special argument name used to represent the current instance.
        /// </summary>
        public const string parameter_this = "@this";

        /// <summary>
        ///     The parameter name used for a seek origin.
        /// </summary>
        public const string parameter_origin = "origin";

        /// <summary>
        ///     The resource key for an error indicating that a duplicate key is being added.
        /// </summary>
        public const string Argument_AddingDuplicateWithKey = "AddingDuplicateWithKey: {0}";

        /// <summary>
        ///     The format string for an error indicating that a location is not naturally aligned.
        /// </summary>
        public const string Argument_AlignmentMustBeNatural = "MustBeNaturallyAligned: {0}-bit value at bit offset {1}";

        /// <summary>
        ///     The resource key for an error indicating that an alignment is not a power of two.
        /// </summary>
        public const string Argument_AlignmentMustBePow2 = "AlignmentMustBePow2";

        /// <summary>
        ///     The resource key for an error indicating that array lengths differ.
        /// </summary>
        public const string Argument_ArrayLengthsDiffer = "ArrayLengthsDiffer";

        /// <summary>
        ///     The resource key for an error indicating that a buffer did not come from the expected pool.
        /// </summary>
        public const string Argument_BufferNotFromPool = "BufferNotFromPool";

        /// <summary>
        ///     The resource key for an error indicating that an argument is empty.
        /// </summary>
        public const string Argument_Empty = "Empty";

        /// <summary>
        ///     The resource key for an error indicating that a seek origin is invalid.
        /// </summary>
        public const string Argument_InvalidSeekOrigin = "InvalidSeekOrigin";

        /// <summary>
        ///     The resource key for an error indicating that an argument must not be null.
        /// </summary>
        public const string ArgumentNull_MustBeNotNull = "MustBeNotNull";

        /// <summary>
        ///     The format string for an error indicating that a value must be aligned to a specified boundary.
        /// </summary>
        public const string ArgumentOutOfRange_MustBeAlignedTo = "MustBeAlignedTo: {0}";

        /// <summary>
        ///     The resource key for an error indicating that a value must be between zero and one.
        /// </summary>
        public const string ArgumentOutOfRange_MustBeBetweenZeroAndOne = "MustBeBetweenZeroAndOne";

        /// <summary>
        ///     The format string for an error indicating that an actual value must equal an expected value.
        /// </summary>
        public const string ArgumentOutOfRange_MustBeEqual = "MustBeEqual: actual: {0}, expected: {1}";

        /// <summary>
        ///     The resource key for an error indicating that a value must be greater than or equal to another value.
        /// </summary>
        public const string ArgumentOutOfRange_MustBeGreaterOrEqual = "MustBeGreaterOrEqual";

        /// <summary>
        ///     The resource key for an error indicating that a value must be less than another value.
        /// </summary>
        public const string ArgumentOutOfRange_MustBeLess = "MustBeLess";

        /// <summary>
        ///     The resource key for an error indicating that a value must be less than or equal to another value.
        /// </summary>
        public const string ArgumentOutOfRange_MustBeLessOrEqual = "MustBeLessOrEqual";

        /// <summary>
        ///     The resource key for an error indicating that a value must not be negative.
        /// </summary>
        public const string ArgumentOutOfRange_MustBeNonNegative = "MustBeNonNegative";

        /// <summary>
        ///     The resource key for an error indicating that a value must be positive.
        /// </summary>
        public const string ArgumentOutOfRange_MustBeNonNegativeNonZero = "MustBeNonNegativeNonZero";

        /// <summary>
        ///     The format suffix for an error indicating that an ASCII digit was expected.
        /// </summary>
        public const string Format_ExpectedAsciiDigit = ", Reason: ExpectedAsciiDigit";

        /// <summary>
        ///     The resource key for an error indicating that an index is out of range.
        /// </summary>
        public const string Format_IndexOutOfRange = "IndexOutOfRange";

        /// <summary>
        ///     The format string for an error indicating that a string offset is invalid.
        /// </summary>
        public const string Format_InvalidStringWithOffsetAndReason = "InvalidStringWithOffset: {0}{1}";

        /// <summary>
        ///     The format suffix for an error indicating that a format item was not closed.
        /// </summary>
        public const string Format_UnclosedFormatItem = ", Reason: UnclosedFormatItem";

        /// <summary>
        ///     The format suffix for an error indicating that a closing brace was unexpected.
        /// </summary>
        public const string Format_UnexpectedClosingBrace = ", Reason: UnexpectedClosingBrace";

        /// <summary>
        ///     The resource key for an error indicating that concurrent operations are not supported.
        /// </summary>
        public const string InvalidOperation_ConcurrentOperationsNotSupported = "ConcurrentOperationsNotSupported";

        /// <summary>
        ///     The resource key for an error indicating that a duplicate value was encountered.
        /// </summary>
        public const string InvalidOperation_Duplicate = "Duplicate";

        /// <summary>
        ///     The resource key for an error indicating that a queue is empty.
        /// </summary>
        public const string InvalidOperation_EmptyQueue = "EmptyQueue";

        /// <summary>
        ///     The resource key for an error indicating that a stack is empty.
        /// </summary>
        public const string InvalidOperation_EmptyStack = "EmptyStack";

        /// <summary>
        ///     The resource key for an error indicating that enumeration failed because the collection changed.
        /// </summary>
        public const string InvalidOperation_EnumFailedVersion = "EnumFailedVersion";

        /// <summary>
        ///     The resource key for an error indicating that a hash table capacity overflowed.
        /// </summary>
        public const string InvalidOperation_HashtableCapacityOverflow = "HashtableCapacityOverflow";

        /// <summary>
        ///     The resource key for an error indicating that values do not match.
        /// </summary>
        public const string InvalidOperation_Mismatch = "Mismatch";

        /// <summary>
        ///     The resource key for an error indicating that a seek would move before the beginning of a stream.
        /// </summary>
        public const string IO_SeekBeforeBegin = "SeekBeforeBegin";

        /// <summary>
        ///     The resource key for an error indicating that a stream is too long.
        /// </summary>
        public const string IO_StreamTooLong = "StreamTooLong";

        /// <summary>
        ///     The resource key for an error indicating that <see cref="object.Equals(object)" /> cannot be called.
        /// </summary>
        public const string NotSupported_CannotCallEquals = "CannotCallEquals";

        /// <summary>
        ///     The resource key for an error indicating that enumeration cannot be performed.
        /// </summary>
        public const string NotSupported_CannotCallGetEnumerator = "CannotCallGetEnumerator";

        /// <summary>
        ///     The resource key for an error indicating that <see cref="object.GetHashCode" /> cannot be called.
        /// </summary>
        public const string NotSupported_CannotCallGetHashCode = "CannotCallGetHashCode";

        /// <summary>
        ///     Gets the name of the specified <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type whose name to return.</param>
        /// <returns>The name of <paramref name="type" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTypeName(Type type) => type.Name;

        /// <summary>
        ///     Returns the result of processing a composite format string, which
        ///     contains zero or more format items. Each format item is replaced by the
        ///     string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <returns>The formatted string.</returns>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is malformed or a format
        ///     item refers to an argument index that has no corresponding argument.
        /// </exception>
        [SkipLocalsInit]
        public static string Format<T0>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, T0 arg0)
        {
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0);
                return temp.ToString();
            }
        }

        /// <summary>
        ///     Returns the result of processing a composite format string, which
        ///     contains zero or more format items. Each format item is replaced by the
        ///     string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <returns>The formatted string.</returns>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is malformed or a format
        ///     item refers to an argument index that has no corresponding argument.
        /// </exception>
        [SkipLocalsInit]
        public static string Format<T0, T1>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, T0 arg0, T1 arg1)
        {
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1);
                return temp.ToString();
            }
        }

        /// <summary>
        ///     Returns the result of processing a composite format string, which
        ///     contains zero or more format items. Each format item is replaced by the
        ///     string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <returns>The formatted string.</returns>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is malformed or a format
        ///     item refers to an argument index that has no corresponding argument.
        /// </exception>
        [SkipLocalsInit]
        public static string Format<T0, T1, T2>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, T0 arg0, T1 arg1, T2 arg2)
        {
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2);
                return temp.ToString();
            }
        }

        /// <summary>
        ///     Returns the result of processing a composite format string, which
        ///     contains zero or more format items. Each format item is replaced by the
        ///     string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument to format.</typeparam>
        /// <typeparam name="T1">The type of the second argument to format.</typeparam>
        /// <typeparam name="T2">The type of the third argument to format.</typeparam>
        /// <typeparam name="T3">The type of the fourth argument to format.</typeparam>
        /// <param name="format">The composite format string.</param>
        /// <param name="arg0">The argument to format.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <returns>The formatted string.</returns>
        /// <exception cref="FormatException">
        ///     Thrown when <paramref name="format" /> is malformed or a format
        ///     item refers to an argument index that has no corresponding argument.
        /// </exception>
        [SkipLocalsInit]
        public static string Format<T0, T1, T2, T3>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            using (var temp = UnsafeStringBuilder<char>.From(stackalloc char[512], 0))
            {
                temp.AppendFormat(format, arg0, arg1, arg2, arg3);
                return temp.ToString();
            }
        }
    }
}