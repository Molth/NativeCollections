using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

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
        public static void ThrowIfNegative<T>(T value, string? paramName) where T : unmanaged,
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
                throw new ArgumentOutOfRangeException(paramName, value, "MustBeNonNegative");
        }

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException" /> if <paramref name="value" /> is negative or zero.</summary>
        /// <param name="value">The argument to validate as non-zero or non-negative.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNegativeOrZero<T>(T value, string? paramName) where T : unmanaged,
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
                throw new ArgumentOutOfRangeException(paramName, value, "MustBeNonNegativeNonZero");
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentOutOfRangeException" /> if <paramref name="value" /> is greater than or equal
        ///     <paramref name="other" />.
        /// </summary>
        /// <param name="value">The argument to validate as less than <paramref name="other" />.</param>
        /// <param name="other">The value to compare with <paramref name="value" />.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfGreaterThanOrEqual<T>(T value, T other, string? paramName) where T : unmanaged, IComparable<T>
        {
            if (value.CompareTo(other) >= 0)
                throw new ArgumentOutOfRangeException(paramName, value, "MustBeLess");
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentOutOfRangeException" /> if <paramref name="value" /> is greater than
        ///     <paramref name="other" />.
        /// </summary>
        /// <param name="value">The argument to validate as less or equal than <paramref name="other" />.</param>
        /// <param name="other">The value to compare with <paramref name="value" />.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfGreaterThan<T>(T value, T other, string? paramName) where T : unmanaged, IComparable<T>
        {
            if (value.CompareTo(other) > 0)
                throw new ArgumentOutOfRangeException(paramName, value, "MustBeLessOrEqual");
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentOutOfRangeException" /> if <paramref name="value" /> is less than or equal
        ///     <paramref name="other" />.
        /// </summary>
        /// <param name="value">The argument to validate as greatar than than <paramref name="other" />.</param>
        /// <param name="other">The value to compare with <paramref name="value" />.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfLessThanOrEqual<T>(T value, T other, string? paramName) where T : unmanaged, IComparable<T>
        {
            if (value.CompareTo(other) <= 0)
                throw new ArgumentOutOfRangeException(paramName, value, "MustBeGreater");
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentOutOfRangeException" /> if <paramref name="value" /> is less than
        ///     <paramref name="other" />.
        /// </summary>
        /// <param name="value">The argument to validate as greatar than or equal than <paramref name="other" />.</param>
        /// <param name="other">The value to compare with <paramref name="value" />.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfLessThan<T>(T value, T other, string? paramName) where T : unmanaged, IComparable<T>
        {
            if (value.CompareTo(other) < 0)
                throw new ArgumentOutOfRangeException(paramName, value, "MustBeGreaterOrEqual");
        }

        /// <summary>Throws an <see cref="ArgumentException" /> if <paramref name="value" /> is not a power of two.</summary>
        /// <param name="value">The alignment value to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfAlignmentNotBePow2(uint value, string? paramName)
        {
            if (!BitOperationsHelpers.IsPow2(value))
                throw new ArgumentException("AlignmentMustBePow2", paramName);
        }

        /// <summary>Throws an <see cref="ArgumentNullException" /> if <paramref name="argument" /> is null.</summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNull<T>(T? argument, string? paramName) where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(paramName, "MustBeNotNull");
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
                throw new InvalidOperationException("EnumFailedVersion");
        }

        /// <summary>Throws an <see cref="InvalidOperationException" /> if <paramref name="value" /> is less than zero.</summary>
        /// <param name="value">The enum version value to validate.</param>
        /// <param name="other">The comparison value for detailed error messaging.</param>
        /// <typeparam name="T">The type of the enum version values.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfEnumInvalidVersion<T>(T value, T other) where T : unmanaged,
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
                throw new InvalidOperationException(value.Equals(other) ? "EnumNotStarted" : "EnumEnded");
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
                throw new IOException("SeekBeforeBegin");
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
                throw new IOException("StreamTooLong");
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
                throw new InvalidOperationException("EmptyQueue");
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
                throw new InvalidOperationException("EmptyStack");
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
                throw new InvalidOperationException("HashtableCapacityOverflow");
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
        public static void ThrowAddingDuplicateWithKeyException<T>(T value) where T : unmanaged => throw new ArgumentException($"AddingDuplicateWithKey: {value}");

        /// <summary>
        ///     Throws an <see cref="ArgumentException" /> for invalid seek origin.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowInvalidSeekOriginException() => throw new ArgumentException("InvalidSeekOrigin");

        /// <summary>
        ///     Throws an <see cref="ArgumentException" /> for differing array lengths.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowArrayLengthsDifferException() => throw new ArgumentException("ArrayLengthsDiffer");

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException" /> for values exceeding allowed range.</summary>
        /// <param name="value">The argument value that is out of range.</param>
        /// <param name="paramName">The name of the parameter with the invalid value.</param>
        /// <typeparam name="T">The type of the argument value.</typeparam>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowMustBeLessOrEqualException<T>(T value, string? paramName) where T : unmanaged, IComparable<T> => throw new ArgumentOutOfRangeException(paramName, value, "MustBeLessOrEqual");

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
        public static void ThrowArgumentNullException(string? paramName) => throw new ArgumentNullException(paramName);

        /// <summary>
        ///     Throws an <see cref="InvalidOperationException" /> for concurrent operations not supported.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowConcurrentOperationsNotSupportedException() => throw new InvalidOperationException("ConcurrentOperationsNotSupported");

        /// <summary>
        ///     Throws an <see cref="InvalidOperationException" /> for mismatch errors.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowMismatchException() => throw new InvalidOperationException("Mismatch");

        /// <summary>
        ///     Throws an <see cref="InvalidOperationException" /> for duplicate items.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowDuplicateException() => throw new InvalidOperationException("Duplicate");

        /// <summary>
        ///     Throws an <see cref="InvalidDataException" /> for entirely zero values.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowMustBeNonEntirelyZeroException() => throw new InvalidDataException("MustBeNonEntirelyZero");

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException" /> for unaligned memory.</summary>
        /// <param name="value">The required alignment value in bytes.</param>
        /// <param name="paramName">The name of the parameter with unaligned memory.</param>
        /// <typeparam name="T">The type of the alignment value.</typeparam>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowMustBeAlignedToException<T>(T value, string? paramName) where T : unmanaged => throw new ArgumentOutOfRangeException(paramName, $"MustBeAlignedTo{value}");

        /// <summary>
        ///     Throws an <see cref="ArgumentException" /> for buffers not from a memory pool.
        /// </summary>
        /// <param name="paramName">The name of the parameter with the invalid buffer.</param>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowBufferNotFromPoolException(string? paramName) => throw new ArgumentException("BufferNotFromPool", paramName);

        /// <summary>
        ///     Throws a <see cref="NotSupportedException" /> indicating that the GetEnumerator method cannot be called in this
        ///     context.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowCannotCallGetEnumeratorException() => throw new NotSupportedException("CannotCallGetEnumerator");

        /// <summary>
        ///     Throws a <see cref="NotSupportedException" /> indicating that the Equals method cannot be called in this context.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowCannotCallEqualsException() => throw new NotSupportedException("CannotCallEquals");

        /// <summary>
        ///     Throws a <see cref="NotSupportedException" /> indicating that the GetHashCode method cannot be called in this
        ///     context.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowCannotCallGetHashCodeException() => throw new NotSupportedException("CannotCallGetHashCode");
    }
}