using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET9_0_OR_GREATER
using System.Collections;
#endif

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a builder that dynamically grows using pooled arrays.
    /// </summary>
    /// <typeparam name="T">The type of elements in the builder.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    [IsReferenceOrContainsReferences]
    [BindingType(typeof(ArrayPool<>))]
    [IsAssignableTo(typeof(IIsCreated), typeof(IDisposable), typeof(IReadOnlyCollection<>))]
    public ref struct UnsafeListBuilder<T>
#if NET9_0_OR_GREATER
        : IIsCreated, IDisposable, IReadOnlyCollection<T>
#endif
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private Span<T> _buffer;

        /// <summary>
        ///     The array rented from <see cref="ArrayPool{T}.Shared" />,
        ///     or <see langword="null" /> if the buffer is user-provided.
        /// </summary>
        private T[]? _array;

        /// <summary>
        ///     Gets the number of elements in this.
        /// </summary>
        private int _length;

        /// <summary>
        ///     Gets or sets the number of elements in this.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the value is negative or greater than <see cref="Capacity" />.
        /// </exception>
        public int Length
        {
            readonly get => _length;
            set => SetLength(value);
        }

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _length;

        /// <summary>
        ///     Gets or sets the total number of elements the internal data structure can hold.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the value is less than <see cref="Length" />.
        /// </exception>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _buffer.Length;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetCapacity(value);
        }

        /// <summary>
        ///     Gets a reference to the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to access.</param>
        /// <exception cref="IndexOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is outside the bounds of the underlying buffer.
        /// </exception>
        /// <remarks>
        ///     The index is validated against <see cref="Capacity" /> by the underlying
        ///     <see cref="Span{T}" /> indexer rather than against <see cref="Count" />.
        /// </remarks>
        public ref T this[int index] => ref _buffer[index];

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !Unsafe.IsNullRef(ref MemoryMarshal.GetReference(_buffer));

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _length == 0;

        /// <summary>
        ///     Initializes a new instance of this class with the specified span and length.
        /// </summary>
        /// <param name="buffer">The buffer to use as storage.</param>
        /// <param name="length">The initial number of elements in the builder.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeListBuilder(Span<T> buffer, int length)
        {
            _buffer = buffer;
            _array = null;
            _length = length;
        }

        /// <summary>
        ///     Initializes a new instance of this class with a rented array and length.
        /// </summary>
        /// <param name="array">The array to use as storage.</param>
        /// <param name="length">The initial number of elements in the builder.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeListBuilder(T[] array, int length)
        {
            _buffer = array;
            _array = array;
            _length = length;
        }

        /// <summary>
        ///     Wraps an external span as storage with an initial length of zero.
        /// </summary>
        /// <param name="buffer">The span to use as the underlying storage.</param>
        /// <returns>A builder that uses <paramref name="buffer" /> as its storage.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeListBuilder<T> From(Span<T> buffer) => new(buffer, 0);

        /// <summary>
        ///     Wraps an external span as storage with the specified initial length.
        /// </summary>
        /// <param name="buffer">The span to use as the underlying storage.</param>
        /// <param name="length">The initial number of elements considered in use.</param>
        /// <returns>A builder that uses <paramref name="buffer" /> as its storage.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative or exceeds the length of <paramref name="buffer" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeListBuilder<T> From(Span<T> buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(length, buffer.Length, ExceptionArgument.length);
            return new UnsafeListBuilder<T>(buffer, length);
        }

        /// <summary>
        ///     Creates a builder with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold. Must be non-negative.
        /// </param>
        /// <returns>A newly allocated builder.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeListBuilder<T> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            var array = ArrayPool<T>.Shared.Rent(capacity);
            return new UnsafeListBuilder<T>(array, 0);
        }

        /// <summary>
        ///     Creates a builder with the specified initial capacity and length.
        /// </summary>
        /// <param name="capacity">
        ///     The initial storage capacity. Must be non‑negative.
        /// </param>
        /// <param name="length">
        ///     The initial number of elements considered in use. Must
        ///     be between 0 and <paramref name="capacity" />, inclusive.
        /// </param>
        /// <returns>A newly allocated builder.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> or <paramref name="length" /> is
        ///     negative or <paramref name="length" /> exceeds <paramref name="capacity" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeListBuilder<T> Create(int capacity, int length)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(length, capacity, ExceptionArgument.length);
            var array = ArrayPool<T>.Shared.Rent(capacity);
            return new UnsafeListBuilder<T>(array, length);
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="buffer">The values to copy into this.</param>
        /// <returns>A new instance containing the specified values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeListBuilder<T> Create(ReadOnlySpan<T> buffer)
        {
            var temp = Create(buffer.Length, buffer.Length);
            buffer.CopyTo(temp.AsSpan());
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeListBuilder<T> Create(T arg0)
        {
            var temp = Create(1, 1);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeListBuilder<T> Create(T arg0, T arg1)
        {
            var temp = Create(2, 2);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2)
        {
            var temp = Create(3, 3);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2, T arg3)
        {
            var temp = Create(4, 4);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4)
        {
            var temp = Create(5, 5);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5)
        {
            var temp = Create(6, 6);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6)
        {
            var temp = Create(7, 7);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7)
        {
            var temp = Create(8, 8);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8)
        {
            var temp = Create(9, 9);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9)
        {
            var temp = Create(10, 10);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <param name="arg10">The value to store at index 10 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9, T arg10)
        {
            var temp = Create(11, 11);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            buffer[10] = arg10;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <param name="arg10">The value to store at index 10 in the buffer.</param>
        /// <param name="arg11">The value to store at index 11 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9, T arg10, T arg11)
        {
            var temp = Create(12, 12);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            buffer[10] = arg10;
            buffer[11] = arg11;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <param name="arg10">The value to store at index 10 in the buffer.</param>
        /// <param name="arg11">The value to store at index 11 in the buffer.</param>
        /// <param name="arg12">The value to store at index 12 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9, T arg10, T arg11, T arg12)
        {
            var temp = Create(13, 13);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            buffer[10] = arg10;
            buffer[11] = arg11;
            buffer[12] = arg12;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <param name="arg10">The value to store at index 10 in the buffer.</param>
        /// <param name="arg11">The value to store at index 11 in the buffer.</param>
        /// <param name="arg12">The value to store at index 12 in the buffer.</param>
        /// <param name="arg13">The value to store at index 13 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9, T arg10, T arg11, T arg12, T arg13)
        {
            var temp = Create(14, 14);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            buffer[10] = arg10;
            buffer[11] = arg11;
            buffer[12] = arg12;
            buffer[13] = arg13;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <param name="arg10">The value to store at index 10 in the buffer.</param>
        /// <param name="arg11">The value to store at index 11 in the buffer.</param>
        /// <param name="arg12">The value to store at index 12 in the buffer.</param>
        /// <param name="arg13">The value to store at index 13 in the buffer.</param>
        /// <param name="arg14">The value to store at index 14 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9, T arg10, T arg11, T arg12, T arg13, T arg14)
        {
            var temp = Create(15, 15);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            buffer[10] = arg10;
            buffer[11] = arg11;
            buffer[12] = arg12;
            buffer[13] = arg13;
            buffer[14] = arg14;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <param name="arg10">The value to store at index 10 in the buffer.</param>
        /// <param name="arg11">The value to store at index 11 in the buffer.</param>
        /// <param name="arg12">The value to store at index 12 in the buffer.</param>
        /// <param name="arg13">The value to store at index 13 in the buffer.</param>
        /// <param name="arg14">The value to store at index 14 in the buffer.</param>
        /// <param name="arg15">The value to store at index 15 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        public static UnsafeListBuilder<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9, T arg10, T arg11, T arg12, T arg13, T arg14, T arg15)
        {
            var temp = Create(16, 16);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            buffer[10] = arg10;
            buffer[11] = arg11;
            buffer[12] = arg12;
            buffer[13] = arg13;
            buffer[14] = arg14;
            buffer[15] = arg15;
            return temp;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var array = _array;
            if (array == null)
                return;

            _array = null;
            ArrayPool<T>.Shared.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _length = 0;

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        /// <param name="clear">
        ///     <see langword="true" /> to clear the buffer before resetting
        ///     the length; otherwise, <see langword="false" />.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(bool clear)
        {
            if (clear)
                _buffer.Clear();

            _length = 0;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeListBuilder<{0}>[{1}]", SR.GetTypeName(typeof(T)), _length);

        /// <summary>
        ///     Appends the specified object to this instance.
        /// </summary>
        /// <param name="item">The item to append to this.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(T item)
        {
            var length = _length;
            var buffer = _buffer;
            if (U32.FromI32Unchecked(length) < U32.FromI32Unchecked(buffer.Length))
            {
                buffer[length] = item;
                _length = length.WrappingAdd(1);
            }
            else
            {
                Grow();
                _buffer[length] = item;
                _length = length.WrappingAdd(1);
            }
        }

        /// <summary>
        ///     Appends the elements of a specified read-only span to this instance.
        /// </summary>
        /// <param name="source">The values to append to this.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append([MustNotSelfAlias] ReadOnlySpan<T> source)
        {
            var length = _length;
            var buffer = _buffer;
            if (source.Length == 1 && U32.FromI32Unchecked(length) < U32.FromI32Unchecked(buffer.Length))
            {
                buffer[length] = source[0];
                _length = length.WrappingAdd(1);
            }
            else
            {
                if (U32.FromI32Unchecked(_length.WrappingAdd(source.Length)) > U32.FromI32Unchecked(Capacity))
                    Grow(Capacity.WrappingSub(_length).WrappingAdd(source.Length));

                source.CopyTo(_buffer.Slice(_length));
                _length = _length.WrappingAdd(source.Length);
            }
        }

        /// <summary>
        ///     Returns a span over the entire contents of this.
        /// </summary>
        /// <returns>A span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan() => _buffer.Slice(0, _length);

        /// <summary>
        ///     Returns a read-only span over the entire contents of this.
        /// </summary>
        /// <returns>A read-only span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan() => _buffer.Slice(0, _length);

        /// <summary>
        ///     Returns memory over the elements currently considered part of this.
        /// </summary>
        /// <returns>The memory currently considered part of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when this instance wraps an external span (see <see cref="From(Span{T}, int)" />)
        ///     and <see cref="Length" /> is greater than zero.
        /// </exception>
        /// <remarks>
        ///     When this instance wraps an external span, no array backs the storage and the returned
        ///     memory would have to be constructed over a <see langword="null" /> array, which is
        ///     rejected by the <see cref="Memory{T}" /> constructor. When this instance rents its
        ///     storage from <see cref="ArrayPool{T}" />, the returned memory refers to the array that
        ///     is currently rented; it becomes invalid after a growth operation (the old array is
        ///     returned to the pool).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Memory<T> AsMemory() => new(_array, 0, _length);

        /// <summary>
        ///     Returns read-only memory over the elements currently considered part of this.
        /// </summary>
        /// <returns>The read-only memory currently considered part of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when this instance wraps an external span (see <see cref="From(Span{T}, int)" />)
        ///     and <see cref="Length" /> is greater than zero.
        /// </exception>
        /// <remarks>
        ///     When this instance wraps an external span, no array backs the storage and the returned
        ///     memory would have to be constructed over a <see langword="null" /> array, which is
        ///     rejected by the <see cref="ReadOnlyMemory{T}" /> constructor. When this instance rents
        ///     its storage from <see cref="ArrayPool{T}" />, the returned memory refers to the array
        ///     that is currently rented; it becomes invalid after a growth operation (the old array is
        ///     returned to the pool).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlyMemory<T> AsReadOnlyMemory() => new(_array, 0, _length);

        /// <summary>
        ///     Sets the number of elements considered in use.
        /// </summary>
        /// <param name="length">
        ///     The new length. Must be between 0 and <see cref="Capacity" /> (inclusive).
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative or exceeds the current capacity.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(int length)
        {
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(length), U32.FromI32Unchecked(Capacity), ExceptionArgument.length);
            _length = length;
        }

        /// <summary>
        ///     Ensures that the capacity of this is at least the specified <paramref name="capacity" />.
        ///     If the current capacity of this is less than specified <paramref name="capacity" />,
        ///     the capacity is increased to at least <paramref name="capacity" />.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (Capacity < capacity)
                Grow(capacity.WrappingSub(Capacity));

            return Capacity;
        }

        /// <summary>
        ///     Trims the excess capacity of this based on the current count.
        /// </summary>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = I32.FromF64Unchecked(F64.FromI32Unchecked(Capacity) * 0.9);
            if (_length < threshold)
                SetCapacity(_length);

            return Capacity;
        }

        /// <summary>
        ///     Trims the excess capacity of this to the specified number of elements.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <returns>The new capacity of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (capacity < _length || capacity >= Capacity)
                return Capacity;

            SetCapacity(capacity);
            return Capacity;
        }

        /// <summary>
        ///     Sets the capacity of this to the specified number of elements.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is less than the current count.
        /// </exception>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 The effective capacity may be rounded up to the next pool bucket size;
        ///                 <see cref="Capacity" /> reports the actual value.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 Storage is rented from <see cref="ArrayPool{T}.Shared" />. A span passed to
        ///                 <see cref="From(Span{T})" /> remains owned by the caller and is never returned to the pool.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public void SetCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(capacity, _length, ExceptionArgument.capacity);
            if (capacity != Capacity)
            {
                var destination = ArrayPool<T>.Shared.Rent(capacity);
                if (_length > 0)
                    _buffer.Slice(0, _length).CopyTo(destination);

                var array = _array;
                _buffer = (Span<T>)(_array = destination);
                if (array == null)
                    return;

                ArrayPool<T>.Shared.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            }
        }

        /// <summary>
        ///     Increases the capacity of this to a new size that is at least the specified minimum capacity.
        /// </summary>
        /// <param name="additionalCapacityRequired">The number of additional elements that must fit in this.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int additionalCapacityRequired = 1)
        {
            var minimumLength = BuilderHelpers.GrowCapacity(Capacity, Capacity.WrappingAdd(additionalCapacityRequired));
            SetCapacity(minimumLength);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        public readonly Span<T>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();

#if NET9_0_OR_GREATER
        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
#endif
    }
}