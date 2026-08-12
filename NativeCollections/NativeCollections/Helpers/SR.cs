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
        public const string parameter_obsolete = "Call this method will always throw an exception.";
        public const string parameter_this = "@this";

        public const string Argument_AddingDuplicateWithKey = "AddingDuplicateWithKey: {0}";
        public const string Argument_AlignmentMustBePow2 = "AlignmentMustBePow2";
        public const string Argument_ArrayLengthsDiffer = "ArrayLengthsDiffer";
        public const string Argument_BufferNotFromPool = "BufferNotFromPool";
        public const string Argument_Empty = "Empty";
        public const string Argument_InvalidSeekOrigin = "InvalidSeekOrigin";

        public const string ArgumentNull_MustBeNotNull = "MustBeNotNull";

        public const string ArgumentOutOfRange_MustBeAlignedTo = "MustBeAlignedTo: {0}";
        public const string ArgumentOutOfRange_MustBeBetweenZeroAndOne = "MustBeBetweenZeroAndOne";
        public const string ArgumentOutOfRange_MustBeEqual = "MustBeEqual: actual: {0}, expected: {1}";
        public const string ArgumentOutOfRange_MustBeGreaterOrEqual = "MustBeGreaterOrEqual";
        public const string ArgumentOutOfRange_MustBeLess = "MustBeLess";
        public const string ArgumentOutOfRange_MustBeLessOrEqual = "MustBeLessOrEqual";
        public const string ArgumentOutOfRange_MustBeNonNegative = "MustBeNonNegative";
        public const string ArgumentOutOfRange_MustBeNonNegativeNonZero = "MustBeNonNegativeNonZero";

        public const string Format_ExpectedAsciiDigit = ", Reason: ExpectedAsciiDigit";
        public const string Format_IndexOutOfRange = "IndexOutOfRange";
        public const string Format_InvalidStringWithOffsetAndReason = "InvalidStringWithOffset: {0}{1}";
        public const string Format_UnclosedFormatItem = ", Reason: UnclosedFormatItem";
        public const string Format_UnexpectedClosingBrace = ", Reason: UnexpectedClosingBrace";

        public const string InvalidOperation_ConcurrentOperationsNotSupported = "ConcurrentOperationsNotSupported";
        public const string InvalidOperation_Duplicate = "Duplicate";
        public const string InvalidOperation_EmptyQueue = "EmptyQueue";
        public const string InvalidOperation_EmptyStack = "EmptyStack";
        public const string InvalidOperation_EnumFailedVersion = "EnumFailedVersion";
        public const string InvalidOperation_HashtableCapacityOverflow = "HashtableCapacityOverflow";
        public const string InvalidOperation_Mismatch = "Mismatch";

        public const string IO_SeekBeforeBegin = "SeekBeforeBegin";
        public const string IO_StreamTooLong = "StreamTooLong";

        public const string NotSupported_CannotCallEquals = "CannotCallEquals";
        public const string NotSupported_CannotCallGetEnumerator = "CannotCallGetEnumerator";
        public const string NotSupported_CannotCallGetHashCode = "CannotCallGetHashCode";

        /// <summary>
        ///     Gets the name of the current member.
        /// </summary>
        /// <returns>A <see cref="T:System.String" /> containing the name of this member.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTypeName(Type type) => type.Name;

        /// <summary>
        ///     Appends the string returned by processing a composite format string,
        ///     which contains zero or more format items, to this instance.
        ///     Each format item is replaced by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<T0>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, T0 arg0)
        {
            using var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0);
            temp.AppendFormat(format, arg0);
            return temp.ToString();
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string,
        ///     which contains zero or more format items, to this instance.
        ///     Each format item is replaced by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<T0, T1>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, T0 arg0, T1 arg1)
        {
            using var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0);
            temp.AppendFormat(format, arg0, arg1);
            return temp.ToString();
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string,
        ///     which contains zero or more format items, to this instance.
        ///     Each format item is replaced by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<T0, T1, T2>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, T0 arg0, T1 arg1, T2 arg2)
        {
            using var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0);
            temp.AppendFormat(format, arg0, arg1, arg2);
            return temp.ToString();
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string,
        ///     which contains zero or more format items, to this instance.
        ///     Each format item is replaced by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<T0, T1, T2, T3>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            using var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0);
            temp.AppendFormat(format, arg0, arg1, arg2, arg3);
            return temp.ToString();
        }
    }
}