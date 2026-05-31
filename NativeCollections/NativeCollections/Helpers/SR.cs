// ReSharper disable ALL

using System.Runtime.CompilerServices;

namespace NativeCollections
{
    /// <summary>
    ///     String resources
    /// </summary>
    internal static class SR
    {
        public const string Argument_AddingDuplicateWithKey = "AddingDuplicateWithKey: {0}";
        public const string Argument_AlignmentMustBePow2 = "AlignmentMustBePow2";
        public const string Argument_ArrayLengthsDiffer = "ArrayLengthsDiffer";
        public const string Argument_BufferNotFromPool = "BufferNotFromPool";
        public const string Argument_Empty = "Empty";
        public const string Argument_InvalidSeekOrigin = "InvalidSeekOrigin";
        public const string Argument_MustBeAlignedTo = "MustBeAlignedTo: {0}";
        public const string Argument_MustBeBetweenZeroAndOne = "MustBeBetweenZeroAndOne";
        public const string Argument_MustBeGreaterOrEqual = "MustBeGreaterOrEqual";
        public const string Argument_MustBeLess = "MustBeLess";
        public const string Argument_MustBeLessOrEqual = "MustBeLessOrEqual";
        public const string Argument_MustBeNonNegative = "MustBeNonNegative";
        public const string Argument_MustBeNonNegativeNonZero = "MustBeNonNegativeNonZero";
        public const string Argument_MustBeNotNull = "MustBeNotNull";
        public const string Format_ExpectedAsciiDigit = ", Reason: ExpectedAsciiDigit";
        public const string Format_IndexOutOfRange = "IndexOutOfRange";
        public const string Format_InvalidStringWithOffsetAndReason = "InvalidStringWithOffset: {0}{1}";
        public const string Format_UnclosedFormatItem = ", Reason: UnclosedFormatItem";
        public const string Format_UnexpectedClosingBrace = ", Reason: UnexpectedClosingBrace";
        public const string InvalidData_MustBeNonEntirelyZero = "MustBeNonEntirelyZero";
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
        ///     Format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<T0>(string format, T0 arg0)
        {
            using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
            temp.AppendFormat(format, arg0);
            return temp.ToString();
        }

        /// <summary>
        ///     Format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<T0, T1>(string format, T0 arg0, T1 arg1)
        {
            using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
            temp.AppendFormat(format, arg0, arg1);
            return temp.ToString();
        }
    }
}