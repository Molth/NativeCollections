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
        ///     Initializes a new instance of this class
        ///     with the specified dimensions,
        ///     using the natural alignment of <typeparamref name="T" />
        ///     and without zero-initializing the allocated memory.
        /// </summary>
        /// <param name="x">The number of rows (first dimension).</param>
        /// <param name="y">The number of columns (second dimension).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="x" /> or <paramref name="y" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray2(int x, int y)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            _buffer = NativeMemoryAllocator.AlignedAlloc<T>((uint)(x * y));
            _x = x;
            _y = y;
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified dimensions,
        ///     using the natural alignment of <typeparamref name="T" />
        ///     and optionally zero-initializing the memory.
        /// </summary>
        /// <param name="x">The number of rows (first dimension).</param>
        /// <param name="y">The number of columns (second dimension).</param>
        /// <param name="zeroed">
        ///     <see langword="true" /> to zero-initialize the allocated memory;
        ///     otherwise, the memory content is undefined.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="x" /> or <paramref name="y" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray2(int x, int y, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            _buffer = zeroed ? NativeMemoryAllocator.AlignedAllocZeroed<T>((uint)(x * y)) : NativeMemoryAllocator.AlignedAlloc<T>((uint)(x * y));
            _x = x;
            _y = y;
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified dimensions and alignment,
        ///     without zero-initializing the allocated memory.
        /// </summary>
        /// <param name="x">The number of rows (first dimension).</param>
        /// <param name="y">The number of columns (second dimension).</param>
        /// <param name="alignment">
        ///     The required alignment in bytes,
        ///     which must be a power of two and at least the natural alignment of <typeparamref name="T" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="x" />, <paramref name="y" />, or <paramref name="alignment" /> is negative,
        ///     or if <paramref name="alignment" /> is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="alignment" /> is not a power of two.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray2(int x, int y, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfLessThan((uint)alignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            _buffer = (T*)NativeMemoryAllocator.AlignedAlloc((uint)(x * y), (uint)alignment);
            _x = x;
            _y = y;
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified dimensions, alignment, and zero-initialization option.
        /// </summary>
        /// <param name="x">The number of rows (first dimension).</param>
        /// <param name="y">The number of columns (second dimension).</param>
        /// <param name="alignment">
        ///     The required alignment in bytes,
        ///     which must be a power of two and at least the natural alignment of <typeparamref name="T" />.
        /// </param>
        /// <param name="zeroed">
        ///     <see langword="true" /> to zero-initialize the allocated memory;
        ///     otherwise, the memory content is undefined.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="x" />, <paramref name="y" />, or <paramref name="alignment" /> is negative,
        ///     or if <paramref name="alignment" /> is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="alignment" /> is not a power of two.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray2(int x, int y, int alignment, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfLessThan((uint)alignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            _buffer = zeroed ? (T*)NativeMemoryAllocator.AlignedAllocZeroed((uint)(x * y * Unsafe.SizeOf<T>()), (uint)alignment) : (T*)NativeMemoryAllocator.AlignedAlloc((uint)(x * y * Unsafe.SizeOf<T>()), (uint)alignment);
            _x = x;
            _y = y;
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     that wraps an existing native memory buffer.
        /// </summary>
        /// <param name="buffer">A pointer to the existing native memory buffer.</param>
        /// <param name="x">The number of rows (first dimension).</param>
        /// <param name="y">The number of columns (second dimension).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="x" /> or <paramref name="y" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray2(T* buffer, int x, int y)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            _buffer = buffer;
            _x = x;
            _y = y;
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

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
        public int Count => _x * _y;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public NativeArray<T> this[int x]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(UnsafeHelpers.Add<T>(_buffer, x * _y), _y);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public NativeArray<T> this[uint x]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(UnsafeHelpers.Add<T>(_buffer, (nint)(x * _y)), _y);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(x * _y + y));
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[uint x, uint y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(x * _y + y));
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeArray2<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeArray2<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeArray2<{0}>[{1}, {2}]", SR.GetTypeName(typeof(T)), _x, _y);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeArray2<T> left, NativeArray2<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeArray2<T> left, NativeArray2<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Free(_buffer);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_buffer), _x * _y);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), _x * _y - start);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_buffer), _x * _y);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), _x * _y - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), length);

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(NativeArray2<T> value) => value._buffer;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativeArray2<T> value) => value.AsSpan();

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativeArray2<T> value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<T>(NativeArray2<T> value) => new(value._buffer, value._x * value._y);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryArray<T>(NativeArray2<T> value) => new(value._buffer, value._x * value._y);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSlice<T>(NativeArray2<T> value) => new(value._buffer, value._x * value._y);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeArray2<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public NativeArray<T>.Enumerator GetEnumerator() => new(this);

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
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
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
    }
}