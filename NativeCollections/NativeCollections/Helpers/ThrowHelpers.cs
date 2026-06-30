using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Throw helpers
    /// </summary>
    internal static class ThrowHelpers
    {
        /// <summary>Throws an <see cref="ArgumentOutOfRangeException" /> if <paramref name="value" /> is negative.</summary>
        /// <param name="value">The argument to validate as non-negative.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNegative<T>(T value, ExceptionArgument paramName) where T : unmanaged,
#if NET7_0_OR_GREATER
            ISignedNumber<T>
#else
            IComparable<T>
#endif
        {
#if NET7_0_OR_GREATER
            if (T.IsNegative(value))
#else
            if (value.CompareTo(default) < 0)
#endif
                throw new ArgumentOutOfRangeException(GetArgumentName(paramName), value, SR.Argument_MustBeNonNegative);
        }

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException" /> if <paramref name="value" /> is negative or zero.</summary>
        /// <param name="value">The argument to validate as non-zero or non-negative.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNegativeOrZero<T>(T value, ExceptionArgument paramName) where T : unmanaged,
#if NET7_0_OR_GREATER
            ISignedNumber<T>
#else
            IComparable<T>
#endif
        {
#if NET7_0_OR_GREATER
            if (T.IsNegative(value) || T.IsZero(value))
#else
            if (value.CompareTo(default) <= 0)
#endif
                throw new ArgumentOutOfRangeException(GetArgumentName(paramName), value, SR.Argument_MustBeNonNegativeNonZero);
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentOutOfRangeException" /> if <paramref name="value" /> is greater than or equal
        ///     <paramref name="other" />.
        /// </summary>
        /// <param name="value">The argument to validate as less than <paramref name="other" />.</param>
        /// <param name="other">The value to compare with <paramref name="value" />.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfGreaterThanOrEqual<T>(T value, T other, ExceptionArgument paramName) where T : unmanaged, IComparable<T>
        {
            if (value.CompareTo(other) >= 0)
                throw new ArgumentOutOfRangeException(GetArgumentName(paramName), value, SR.Argument_MustBeLess);
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentOutOfRangeException" /> if <paramref name="value" /> is greater than
        ///     <paramref name="other" />.
        /// </summary>
        /// <param name="value">The argument to validate as less or equal than <paramref name="other" />.</param>
        /// <param name="other">The value to compare with <paramref name="value" />.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfGreaterThan<T>(T value, T other, ExceptionArgument paramName) where T : unmanaged, IComparable<T>
        {
            if (value.CompareTo(other) > 0)
                throw new ArgumentOutOfRangeException(GetArgumentName(paramName), value, SR.Argument_MustBeLessOrEqual);
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentOutOfRangeException" /> if <paramref name="value" /> is less than
        ///     <paramref name="other" />.
        /// </summary>
        /// <param name="value">The argument to validate as greater than or equal than <paramref name="other" />.</param>
        /// <param name="other">The value to compare with <paramref name="value" />.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfLessThan<T>(T value, T other, ExceptionArgument paramName) where T : unmanaged, IComparable<T>
        {
            if (value.CompareTo(other) < 0)
                throw new ArgumentOutOfRangeException(GetArgumentName(paramName), value, SR.Argument_MustBeGreaterOrEqual);
        }

        /// <summary>Throws an <see cref="ArgumentException" /> if <paramref name="value" /> is not a power of two.</summary>
        /// <param name="value">The alignment value to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfAlignmentNotBePow2(uint value, ExceptionArgument paramName)
        {
            if (!BitOperationsHelpers.IsPow2(value))
                throw new ArgumentException(SR.Argument_AlignmentMustBePow2, GetArgumentName(paramName));
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentOutOfRangeException" /> if <paramref name="value" /> is not between 0.0 and 1.0.
        /// </summary>
        /// <param name="value">The argument to validate as a probability between 0.0 and 1.0 inclusive.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfProbabilityOutOfRange(double value, ExceptionArgument paramName)
        {
            if (value < 0.0 || value > 1.0)
                throw new ArgumentOutOfRangeException(GetArgumentName(paramName), value, SR.Argument_MustBeBetweenZeroAndOne);
        }

        /// <summary>Throws an <see cref="ArgumentNullException" /> if <paramref name="argument" /> is null.</summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNull<T>(T? argument, ExceptionArgument paramName) where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(GetArgumentName(paramName), SR.Argument_MustBeNotNull);
        }

        /// <summary>Throws an <see cref="ArgumentException" /> if <paramref name="argument" /> is empty.</summary>
        /// <param name="argument">The buffer to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument" /> corresponds.</param>
        /// <typeparam name="T">The type of the count value.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfSpanEmpty<T>(Span<T> argument, ExceptionArgument paramName)
        {
            if (argument.IsEmpty)
                throw new ArgumentException(SR.Argument_Empty, GetArgumentName(paramName));
        }

        /// <summary>Throws an <see cref="ArgumentException" /> if <paramref name="argument" /> is empty.</summary>
        /// <param name="argument">The buffer to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument" /> corresponds.</param>
        /// <typeparam name="T">The type of the count value.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfReadOnlySpanEmpty<T>(ReadOnlySpan<T> argument, ExceptionArgument paramName)
        {
            if (argument.IsEmpty)
                throw new ArgumentException(SR.Argument_Empty, GetArgumentName(paramName));
        }

        /// <summary>
        ///     Throws an <see cref="InvalidOperationException" /> if <paramref name="value" /> does not equal
        ///     <paramref name="other" />.
        /// </summary>
        /// <param name="value">The current enum version to validate.</param>
        /// <param name="other">The expected enum version to compare with.</param>
        /// <typeparam name="T">The type of the enum version values.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfEnumFailedVersion<T>(T value, T other) where T : unmanaged, IEquatable<T>
        {
            if (!value.Equals(other))
                throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
        }

        /// <summary>Throws an <see cref="IOException" /> if <paramref name="value" /> is negative (seek before begin).</summary>
        /// <param name="value">The seek position to validate.</param>
        /// <typeparam name="T">The type of the position value.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfSeekBeforeBegin<T>(T value) where T : unmanaged,
#if NET7_0_OR_GREATER
            ISignedNumber<T>
#else
            IComparable<T>
#endif
        {
#if NET7_0_OR_GREATER
            if (T.IsNegative(value))
#else
            if (value.CompareTo(default) < 0)
#endif
                throw new IOException(SR.IO_SeekBeforeBegin);
        }

        /// <summary>Throws an <see cref="IOException" /> if <paramref name="value" /> is negative (stream too long).</summary>
        /// <param name="value">The stream length value to validate.</param>
        /// <typeparam name="T">The type of the length value.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfStreamTooLong<T>(T value) where T : unmanaged,
#if NET7_0_OR_GREATER
            ISignedNumber<T>
#else
            IComparable<T>
#endif
        {
#if NET7_0_OR_GREATER
            if (T.IsNegative(value))
#else
            if (value.CompareTo(default) < 0)
#endif
                throw new IOException(SR.IO_StreamTooLong);
        }

        /// <summary>Throws an <see cref="InvalidOperationException" /> if <paramref name="value" /> is zero (empty queue).</summary>
        /// <param name="value">The queue count to validate.</param>
        /// <typeparam name="T">The type of the count value.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfEmptyQueue<T>(T value) where T : unmanaged,
#if NET7_0_OR_GREATER
            INumberBase<T>
#else
            IComparable<T>
#endif
        {
#if NET7_0_OR_GREATER
            if (T.IsZero(value))
#else
            if (value.CompareTo(default) == 0)
#endif
                throw new InvalidOperationException(SR.InvalidOperation_EmptyQueue);
        }

        /// <summary>
        ///     Throws an <see cref="InvalidOperationException" /> if <paramref name="value" /> is greater than or equal to
        ///     <paramref name="other" /> (empty stack).
        /// </summary>
        /// <param name="value">The current stack index to validate.</param>
        /// <param name="other">The stack capacity to compare against.</param>
        /// <typeparam name="T">The type of the index and capacity values.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfEmptyStack<T>(T value, T other) where T : unmanaged, IComparable<T>
        {
            if (value.CompareTo(other) >= 0)
                throw new InvalidOperationException(SR.InvalidOperation_EmptyStack);
        }

        /// <summary>
        ///     Throws an <see cref="InvalidOperationException" /> if <paramref name="value" /> is negative (hashtable
        ///     capacity overflow).
        /// </summary>
        /// <param name="value">The hashtable capacity to validate.</param>
        /// <typeparam name="T">The type of the capacity value.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfHashtableCapacityOverflow<T>(T value) where T : unmanaged,
#if NET7_0_OR_GREATER
            ISignedNumber<T>
#else
            IComparable<T>
#endif
        {
#if NET7_0_OR_GREATER
            if (T.IsNegative(value))
#else
            if (value.CompareTo(default) < 0)
#endif
                throw new InvalidOperationException(SR.InvalidOperation_HashtableCapacityOverflow);
        }

        /// <summary>Throws a <see cref="KeyNotFoundException" /> with the specified key value.</summary>
        /// <param name="value">The key value that was not found.</param>
        /// <typeparam name="T">The type of the key value.</typeparam>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowKeyNotFoundException<T>(T value) where T : unmanaged => throw new KeyNotFoundException(value.ToString());

        /// <summary>Throws an <see cref="ArgumentException" /> for duplicate key addition.</summary>
        /// <param name="value">The duplicate key value.</param>
        /// <typeparam name="T">The type of the key value.</typeparam>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowAddingDuplicateWithKeyException<T>(T value) where T : unmanaged => throw new ArgumentException(SR.Format(SR.Argument_AddingDuplicateWithKey, value));

        /// <summary>
        ///     Throws an <see cref="ArgumentException" /> for invalid seek origin.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowInvalidSeekOriginException() => throw new ArgumentException(SR.Argument_InvalidSeekOrigin);

        /// <summary>
        ///     Throws an <see cref="ArgumentException" /> for differing array lengths.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowArrayLengthsDifferException() => throw new ArgumentException(SR.Argument_ArrayLengthsDiffer);

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException" /> for values exceeding allowed range.</summary>
        /// <param name="value">The argument value that is out of range.</param>
        /// <param name="paramName">The name of the parameter with the invalid value.</param>
        /// <typeparam name="T">The type of the argument value.</typeparam>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowMustBeLessOrEqualException<T>(T value, ExceptionArgument paramName) where T : unmanaged, IComparable<T> => throw new ArgumentOutOfRangeException(GetArgumentName(paramName), value, SR.Argument_MustBeLessOrEqual);

        /// <summary>
        ///     Throws an <see cref="OutOfMemoryException" />.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowOutOfMemoryException() => throw new OutOfMemoryException();

        /// <summary>Throws an <see cref="ArgumentNullException" /> for null arguments.</summary>
        /// <param name="paramName">The name of the parameter that is null.</param>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowArgumentNullException(ExceptionArgument paramName) => throw new ArgumentNullException(GetArgumentName(paramName));

        /// <summary>
        ///     Throws an <see cref="InvalidOperationException" /> for concurrent operations not supported.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowConcurrentOperationsNotSupportedException() => throw new InvalidOperationException(SR.InvalidOperation_ConcurrentOperationsNotSupported);

        /// <summary>
        ///     Throws an <see cref="InvalidOperationException" /> for mismatch errors.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowMismatchException() => throw new InvalidOperationException(SR.InvalidOperation_Mismatch);

        /// <summary>
        ///     Throws an <see cref="InvalidOperationException" /> for duplicate items.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowDuplicateException() => throw new InvalidOperationException(SR.InvalidOperation_Duplicate);

        /// <summary>
        ///     Throws an <see cref="InvalidDataException" /> for entirely zero values.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowMustBeNonEntirelyZeroException() => throw new InvalidDataException(SR.InvalidData_MustBeNonEntirelyZero);

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException" /> for unaligned memory.</summary>
        /// <param name="value">The required alignment value in bytes.</param>
        /// <param name="paramName">The name of the parameter with unaligned memory.</param>
        /// <typeparam name="T">The type of the alignment value.</typeparam>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowMustBeAlignedToException<T>(T value, ExceptionArgument paramName) where T : unmanaged => throw new ArgumentOutOfRangeException(GetArgumentName(paramName), SR.Format(SR.Argument_MustBeAlignedTo, value));

        /// <summary>
        ///     Throws an <see cref="ArgumentException" /> for buffers not from a memory pool.
        /// </summary>
        /// <param name="paramName">The name of the parameter with the invalid buffer.</param>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowBufferNotFromPoolException(ExceptionArgument paramName) => throw new ArgumentException(SR.Argument_BufferNotFromPool, GetArgumentName(paramName));

        /// <summary>
        ///     Throws a <see cref="NotSupportedException" /> indicating that the GetEnumerator method cannot be called in this
        ///     context.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowCannotCallGetEnumeratorException() => throw new NotSupportedException(SR.NotSupported_CannotCallGetEnumerator);

        /// <summary>
        ///     Throws a <see cref="NotSupportedException" /> indicating that the Equals method cannot be called in this context.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowCannotCallEqualsException() => throw new NotSupportedException(SR.NotSupported_CannotCallEquals);

        /// <summary>
        ///     Throws a <see cref="NotSupportedException" /> indicating that the GetHashCode method cannot be called in this
        ///     context.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowCannotCallGetHashCodeException() => throw new NotSupportedException(SR.NotSupported_CannotCallGetHashCode);

        /// <summary>
        ///     Throws a <see cref="NotSupportedException" />.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowNotSupportedException() => throw new NotSupportedException();

#if !NET5_0_OR_GREATER
        /// <summary>
        ///     Throws a <see cref="NullReferenceException" />.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowNullReferenceException() => throw new NullReferenceException();
#endif

        /// <summary>
        ///     Throws a <see cref="FormatException" /> indicating that the string format is invalid at a specific offset.
        /// </summary>
        /// <param name="offset">The offset where the format is invalid.</param>
        /// <param name="reason">The reason why the format is invalid.</param>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowFormatInvalidString(int offset, ExceptionResource reason) => throw new FormatException(SR.Format(SR.Format_InvalidStringWithOffsetAndReason, offset, GetResourceString(reason)));

        /// <summary>
        ///     Throws a <see cref="FormatException" /> indicating that the index is out of range for the format.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowFormatIndexOutOfRange() => throw new FormatException(SR.Format_IndexOutOfRange);

        /// <summary>
        ///     Throws a <see cref="InvalidOperationException" />.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowInvalidOperationException() => throw new InvalidOperationException();

        /// <summary>
        ///     Returns the argument name string associated with the specified <see cref="ExceptionArgument" /> value.
        /// </summary>
        /// <param name="argument">The <see cref="ExceptionArgument" /> value to convert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? GetArgumentName(ExceptionArgument argument)
        {
            return argument switch
            {
                ExceptionArgument.addValueFactory => "addValueFactory",
                ExceptionArgument.alignment => "alignment",
                ExceptionArgument.buffer => "buffer",
                ExceptionArgument.byteCount => "byteCount",
                ExceptionArgument.capacity => "capacity",
                ExceptionArgument.charCount => "charCount",
                ExceptionArgument.count => "count",
                ExceptionArgument.format => "format",
                ExceptionArgument.index => "index",
                ExceptionArgument.key => "key",
                ExceptionArgument.left => "left",
                ExceptionArgument.length => "length",
                ExceptionArgument.maxFreeChunks => "maxFreeChunks",
                ExceptionArgument.maxFreeSlabs => "maxFreeSlabs",
                ExceptionArgument.maxLength => "maxLength",
                ExceptionArgument.minimumLength => "minimumLength",
                ExceptionArgument.obj => "obj",
                ExceptionArgument.offset => "offset",
                ExceptionArgument.position => "position",
                ExceptionArgument.right => "right",
                ExceptionArgument.size => "size",
                ExceptionArgument.sleep1Threshold => "sleep1Threshold",
                ExceptionArgument.source => "source",
                ExceptionArgument.trueProbability => "trueProbability",
                ExceptionArgument.updateValueFactory => "updateValueFactory",
                ExceptionArgument.value => "value",
                ExceptionArgument.valueFactory => "valueFactory",
                ExceptionArgument.x => "x",
                ExceptionArgument.y => "y",
                ExceptionArgument.z => "z",
                _ => null
            };
        }

        /// <summary>
        ///     Returns the resource string associated with the specified <see cref="ExceptionResource" /> value.
        /// </summary>
        /// <param name="resource">The <see cref="ExceptionResource" /> value to convert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? GetResourceString(ExceptionResource resource)
        {
            return resource switch
            {
                ExceptionResource.Format_ExpectedAsciiDigit => SR.Format_ExpectedAsciiDigit,
                ExceptionResource.Format_UnclosedFormatItem => SR.Format_UnclosedFormatItem,
                ExceptionResource.Format_UnexpectedClosingBrace => SR.Format_UnexpectedClosingBrace,
                _ => null
            };
        }
    }
}