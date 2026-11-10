using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a contiguous region of arbitrary native memory.
    /// </summary>
    /// <typeparam name="T">The unmanaged element type.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeArray2<T> : IIsCreated, IDisposable, IEquatable<NativeArray2<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     The size of the first dimension.
        /// </summary>
        private readonly int _x;

        /// <summary>
        ///     The size of the second dimension.
        /// </summary>
        private readonly int _y;

        /// <summary>
        ///     Initializes a new instance of this class with the specified dimensions, using the natural
        ///     alignment of <typeparamref name="T" /> and without zero-initializing the allocated memory.
        /// </summary>
        /// <param name="x">The number of rows (first dimension).</param>
        /// <param name="y">The number of columns (second dimension).</param>
        /// <returns>A newly allocated array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="x" /> or <paramref name="y" /> is negative.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the total number of elements exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray2<T> Create(int x, int y)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            var elementCount = GetElementCount(x, y);
            var buffer = NativeMemoryAllocator.AlignedAlloc<T>(elementCount);
            return new NativeArray2<T>(buffer, x, y);
        }

        /// <summary>
        ///     Initializes a new instance of this class with the specified dimensions, using the natural
        ///     alignment of <typeparamref name="T" /> and optionally zero-initializing the memory.
        /// </summary>
        /// <param name="x">The number of rows (first dimension).</param>
        /// <param name="y">The number of columns (second dimension).</param>
        /// <param name="zeroed">
        ///     <see langword="true" /> to zero-initialize the allocated
        ///     memory; otherwise, the memory content is undefined.
        /// </param>
        /// <returns>A newly allocated array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="x" /> or <paramref name="y" /> is negative.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the total number of elements exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray2<T> Create(int x, int y, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            var elementCount = GetElementCount(x, y);
            var buffer = zeroed ? NativeMemoryAllocator.AlignedAllocZeroed<T>(elementCount) : NativeMemoryAllocator.AlignedAlloc<T>(elementCount);
            return new NativeArray2<T>(buffer, x, y);
        }

        /// <summary>
        ///     Initializes a new instance of this class with the specified dimensions
        ///     and alignment, without zero-initializing the allocated memory.
        /// </summary>
        /// <param name="x">The number of rows (first dimension).</param>
        /// <param name="y">The number of columns (second dimension).</param>
        /// <param name="alignment">
        ///     The required alignment in bytes, which must be a power of two
        ///     and at least the natural alignment of <typeparamref name="T" />.
        /// </param>
        /// <returns>A newly allocated array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="x" />, <paramref name="y" />, or <paramref name="alignment" /> is negative
        ///     or <paramref name="alignment" /> is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the total number of elements exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray2<T> Create(int x, int y, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            var nativeAlignment = Usize.FromI32Unchecked(alignment);
            ThrowHelpers.ThrowIfLessThan(nativeAlignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            var elementCount = GetElementCount(x, y);
            var byteCount = elementCount * Usize.FromI32Unchecked(Unsafe.SizeOf<T>());
            var buffer = (T*)NativeMemoryAllocator.AlignedAlloc(byteCount, nativeAlignment);
            return new NativeArray2<T>(buffer, x, y);
        }

        /// <summary>
        ///     Initializes a new instance of this class with the specified
        ///     dimensions, alignment, and zero-initialization option.
        /// </summary>
        /// <param name="x">The number of rows (first dimension).</param>
        /// <param name="y">The number of columns (second dimension).</param>
        /// <param name="alignment">
        ///     The required alignment in bytes, which must be a power of two
        ///     and at least the natural alignment of <typeparamref name="T" />.
        /// </param>
        /// <param name="zeroed">
        ///     <see langword="true" /> to zero-initialize the allocated
        ///     memory; otherwise, the memory content is undefined.
        /// </param>
        /// <returns>A newly allocated array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="x" />, <paramref name="y" />, or <paramref name="alignment" /> is negative
        ///     or <paramref name="alignment" /> is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the total number of elements exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray2<T> Create(int x, int y, int alignment, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            var nativeAlignment = Usize.FromI32Unchecked(alignment);
            ThrowHelpers.ThrowIfLessThan(nativeAlignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            var elementCount = GetElementCount(x, y);
            var byteCount = elementCount * Usize.FromI32Unchecked(Unsafe.SizeOf<T>());
            var buffer = zeroed ? (T*)NativeMemoryAllocator.AlignedAllocZeroed(byteCount, nativeAlignment) : (T*)NativeMemoryAllocator.AlignedAlloc(byteCount, nativeAlignment);
            return new NativeArray2<T>(buffer, x, y);
        }

        /// <summary>
        ///     Initializes a new instance of this class that wraps an existing native memory buffer.
        /// </summary>
        /// <param name="buffer">A pointer to the existing native memory buffer.</param>
        /// <param name="x">The number of rows (first dimension).</param>
        /// <param name="y">The number of columns (second dimension).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeArray2(T* buffer, int x, int y)
        {
            _buffer = buffer;
            _x = x;
            _y = y;
        }

        /// <summary>
        ///     Creates a two-dimensional array view over an existing native buffer.
        /// </summary>
        /// <param name="buffer">The native buffer to wrap.</param>
        /// <param name="x">The size of the first dimension.</param>
        /// <param name="y">The size of the second dimension.</param>
        /// <returns>A two-dimensional array view that wraps <paramref name="buffer" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="x" /> or <paramref name="y" /> is negative.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the total number of elements exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray2<T> From(T* buffer, int x, int y)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            _ = GetElementCount(x, y);
            return new NativeArray2<T>(buffer, x, y);
        }

        /// <summary>
        ///     Gets the total number of elements for the specified dimensions.
        /// </summary>
        /// <param name="x">The size of the first dimension.</param>
        /// <param name="y">The size of the second dimension.</param>
        /// <returns>The total number of elements.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the total number of elements exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nuint GetElementCount(int x, int y)
        {
            var nativeX = Usize.FromI32Unchecked(x);
            var nativeY = Usize.FromI32Unchecked(y);
            return Usize.FromI32Unchecked(I32.FromUsize(nativeX * nativeY));
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _buffer != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => _x == 0 || _y == 0;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public T* Buffer => _buffer;

        /// <summary>
        ///     The size of the first dimension.
        /// </summary>
        public int X => _x;

        /// <summary>
        ///     The size of the second dimension.
        /// </summary>
        public int Y => _y;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count => _x.WrappingMul(_y);

        /// <summary>
        ///     Gets the one-dimensional array of the specified row.
        /// </summary>
        /// <param name="x">The zero-based index of the row to access.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="x" /> is negative or greater than or equal to <see cref="X" />.
        /// </exception>
        public NativeArray<T> this[int x]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(x), U32.FromI32Unchecked(_x), ExceptionArgument.x);
                return NativeArray<T>.From(UnsafeHelpers.Add<T>(_buffer, Isize.FromI32Unchecked(x.WrappingMul(_y))), _y);
            }
        }

        /// <summary>
        ///     Gets a reference to the element at the specified row and column.
        /// </summary>
        /// <param name="x">The zero-based index of the row to access.</param>
        /// <param name="y">The zero-based index of the column to access.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="x" /> is negative or greater than or equal to <see cref="X" />,
        ///     or <paramref name="y" /> is negative or greater than or equal to <see cref="Y" />.
        /// </exception>
        public ref T this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(x), U32.FromI32Unchecked(_x), ExceptionArgument.x);
                ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(y), U32.FromI32Unchecked(_y), ExceptionArgument.y);
                return ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(x.WrappingMul(_y).WrappingAdd(y)));
            }
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeArray2<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeArray2<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeArray2<{0}>[{1}, {2}]", SR.GetTypeName(typeof(T)), _x, _y);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeArray2<T> left, NativeArray2<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeArray2<T> left, NativeArray2<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

        /// <summary>
        ///     Returns a reference to the 0th element of this. If this is empty, the returned
        ///     reference must not be dereferenced.
        /// </summary>
        /// <returns>A reference to the 0th element of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetPinnableReference() => ref Unsafe.AsRef<T>(_buffer);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => AsSpan().Clear();

        /// <summary>
        ///     Returns a span over the entire contents of this.
        /// </summary>
        /// <returns>A span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_buffer), _x.WrappingMul(_y));

        /// <summary>
        ///     Returns a read-only span over the entire contents of this.
        /// </summary>
        /// <returns>A read-only span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_buffer), _x.WrappingMul(_y));

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        public NativeArray<T>.Enumerator GetEnumerator() => NativeArray<T>.Enumerator.From(NativeArray<T>.From(_buffer, Count));

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
    }
}