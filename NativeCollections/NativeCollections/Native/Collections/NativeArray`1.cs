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
    public readonly unsafe struct NativeArray<T> : IIsCreated, IDisposable, IEquatable<NativeArray<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Initializes a new instance of this class that wraps an existing native memory buffer.
        /// </summary>
        /// <param name="buffer">A pointer to the existing native memory buffer.</param>
        /// <param name="length">The number of elements in the buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeArray(T* buffer, int length)
        {
            _buffer = buffer;
            _length = length;
        }

        /// <summary>
        ///     Calculates the minimum number of bytes required to store a specified number of
        ///     elements, taking into account alignment requirements for the underlying buffer.
        /// </summary>
        /// <param name="capacity">The number of elements to store. Must be non-negative.</param>
        /// <returns>
        ///     The minimum byte count needed to allocate a buffer capable of
        ///     holding <paramref name="capacity" /> elements with proper alignment.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            var nativeCapacity = Usize.FromI32Unchecked(capacity);
            var byteCount = nativeCapacity * Usize.FromI32Unchecked(Unsafe.SizeOf<T>()) + NativeMemoryAllocator.AlignOf<T>().WrappingSub(Usize.FromI32Unchecked(1));
            return I32.FromUsize(byteCount);
        }

        /// <summary>
        ///     Creates a new instance of this class with the specified number of elements, using the natural
        ///     alignment of <typeparamref name="T" /> and without zero-initializing the allocated memory.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <returns>A newly allocated array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> Create(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            var nativeLength = Usize.FromI32Unchecked(length);
            var buffer = NativeMemoryAllocator.AlignedAlloc<T>(nativeLength);
            return new NativeArray<T>(buffer, length);
        }

        /// <summary>
        ///     Creates a new instance of this class with the specified number of elements, using the
        ///     natural alignment of <typeparamref name="T" /> and optionally zero-initializing the memory.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <param name="zeroed">
        ///     <see langword="true" /> to zero-initialize the allocated
        ///     memory; otherwise, the memory content is undefined.
        /// </param>
        /// <returns>A newly allocated array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> Create(int length, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            var nativeLength = Usize.FromI32Unchecked(length);
            var buffer = zeroed ? NativeMemoryAllocator.AlignedAllocZeroed<T>(nativeLength) : NativeMemoryAllocator.AlignedAlloc<T>(nativeLength);
            return new NativeArray<T>(buffer, length);
        }

        /// <summary>
        ///     Creates a new instance of this class with the specified number of
        ///     elements and alignment, without zero-initializing the allocated memory.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <param name="alignment">
        ///     The required alignment in bytes, which must be a power of two
        ///     and at least the natural alignment of <typeparamref name="T" />.
        /// </param>
        /// <returns>A newly allocated array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> or <paramref name="alignment" /> is negative or
        ///     <paramref name="alignment" /> is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> Create(int length, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            var nativeLength = Usize.FromI32Unchecked(length);
            var nativeAlignment = Usize.FromI32Unchecked(alignment);
            ThrowHelpers.ThrowIfLessThan(nativeAlignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            var buffer = (T*)NativeMemoryAllocator.AlignedAlloc(nativeLength * Usize.FromI32Unchecked(Unsafe.SizeOf<T>()), nativeAlignment);
            return new NativeArray<T>(buffer, length);
        }

        /// <summary>
        ///     Creates a new instance of this class with the specified
        ///     number of elements, alignment, and zero-initialization option.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
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
        ///     Thrown when <paramref name="length" /> or <paramref name="alignment" /> is negative or
        ///     <paramref name="alignment" /> is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> Create(int length, int alignment, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            var nativeLength = Usize.FromI32Unchecked(length);
            var nativeAlignment = Usize.FromI32Unchecked(alignment);
            ThrowHelpers.ThrowIfLessThan(nativeAlignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            var byteCount = nativeLength * Usize.FromI32Unchecked(Unsafe.SizeOf<T>());
            var buffer = zeroed ? (T*)NativeMemoryAllocator.AlignedAllocZeroed(byteCount, nativeAlignment) : (T*)NativeMemoryAllocator.AlignedAlloc(byteCount, nativeAlignment);
            return new NativeArray<T>(buffer, length);
        }

        /// <summary>
        ///     Creates an array view over an existing native buffer.
        /// </summary>
        /// <param name="buffer">The native buffer to wrap.</param>
        /// <param name="length">The number of elements in the buffer.</param>
        /// <returns>
        ///     A native array that wraps <paramref name="buffer" /> and has <paramref name="length" /> elements.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> From(T* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            return new NativeArray<T>(buffer, length);
        }

        /// <summary>
        ///     Creates a new instance that references the specified span.
        /// </summary>
        /// <param name="value">The pinned span to wrap.</param>
        /// <returns>A native array that references the memory of <paramref name="value" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> From([MustBePinned] Span<T> value) => From(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), value.Length);

        /// <summary>
        ///     Creates an array view over the aligned portion of a pinned byte span.
        /// </summary>
        /// <param name="buffer">The pinned byte span to wrap.</param>
        /// <returns>The aligned array view.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="buffer" /> is too short to apply the required alignment padding.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> FromBytes([MustBePinned] Span<byte> buffer) => FromBytes(buffer, NativeMemoryAllocator.AlignOf<T>());

        /// <summary>
        ///     Creates an array view over the aligned portion of a pinned byte span.
        /// </summary>
        /// <param name="buffer">The pinned byte span to wrap.</param>
        /// <param name="alignment">The required alignment.</param>
        /// <returns>The aligned array view.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="buffer" /> is too short to apply the required alignment padding.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> FromBytes([MustBePinned] Span<byte> buffer, nuint alignment) => FromBytes(buffer, alignment, out _);

        /// <summary>
        ///     Creates an array view over the aligned portion of a pinned byte span and returns its byte offset.
        /// </summary>
        /// <param name="buffer">The pinned byte span to wrap.</param>
        /// <param name="alignment">The required alignment.</param>
        /// <param name="byteOffset">Receives the offset to the aligned array.</param>
        /// <returns>The aligned array view.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="buffer" /> is too short to apply the required alignment padding.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> FromBytes([MustBePinned] Span<byte> buffer, nuint alignment, out nint byteOffset)
        {
            ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);
            var ptr = Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
            var alignedPtr = Isize.FromUsizeUnchecked(NativeMemoryAllocator.AlignUp(Usize.FromIsizeUnchecked(Isize.FromPtrUnchecked(ptr)), alignment));
            byteOffset = alignedPtr.WrappingSub(Isize.FromPtrUnchecked(ptr));
            return From(MemoryMarshal.Cast<byte, T>(buffer.Slice(I32.FromIsizeUnchecked(byteOffset))));
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _buffer != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        ///     Gets a reference to the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to access.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Length" />.
        /// </exception>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_length), ExceptionArgument.index);
                return ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index));
            }
        }

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public T* Buffer => _buffer;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        public int Length => _length;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count => _length;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeArray<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeArray<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeArray<{0}>[{1}]", SR.GetTypeName(typeof(T)), _length);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeArray<T> left, NativeArray<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeArray<T> left, NativeArray<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

        /// <summary>
        ///     Returns a reference to the 0th element of the Span. If the Span is empty, returns null reference.
        ///     It can be used for pinning and is required to support the use of span within a fixed statement.
        /// </summary>
        /// <returns>A reference to the 0th element of the Span, or a null reference if the Span is empty.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetPinnableReference() => ref Unsafe.AsRef<T>(_buffer);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => AsSpan().Clear();

        /// <summary>
        ///     Casts an instance of one primitive type to another primitive type.
        /// </summary>
        /// <typeparam name="TTo">The type of the elements in the returned view.</typeparam>
        /// <returns>
        ///     A native array that views the same memory reinterpreted as <typeparamref name="TTo" /> elements.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<TTo> Cast<TTo>() where TTo : unmanaged => NativeArray<TTo>.From(MemoryMarshal.Cast<T, TTo>(AsSpan()));

        /// <summary>
        ///     Returns a span over a portion of this.
        /// </summary>
        /// <returns>A span over a portion of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_buffer), _length);

        /// <summary>
        ///     Returns a read-only span over a portion of this.
        /// </summary>
        /// <returns>A read-only span over a portion of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_buffer), _length);

        /// <summary>
        ///     Forms a slice out of the given span.
        /// </summary>
        /// <param name="start">The starting element index.</param>
        /// <returns>A slice of this starting at <paramref name="start" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="start" /> is negative or greater than <see cref="Length" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> Slice(int start) => From(AsSpan().Slice(start));

        /// <summary>
        ///     Forms a slice out of the given span.
        /// </summary>
        /// <param name="start">The starting element index.</param>
        /// <param name="length">The number of elements in the slice.</param>
        /// <returns>
        ///     A slice of this starting at <paramref name="start" /> with <paramref name="length" /> elements.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="start" /> or <paramref name="length" /> is negative, or
        ///     <paramref name="start" /> + <paramref name="length" /> is greater than <see cref="Length" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> Slice(int start, int length) => From(AsSpan().Slice(start, length));

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        public Enumerator GetEnumerator() => Enumerator.From(this);

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

        /// <summary>
        ///     Supports a simple iteration over a generic collection.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IRefIterator<T>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly NativeArray<T> _handle;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(NativeArray<T> handle)
            {
                _handle = handle;
                _index = -1;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(NativeArray<T> handle) => new(handle);

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
            ///     <see langword="false" /> if the enumerator has passed the end of the collection.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var index = _index.WrappingAdd(1);
                if (index < _handle._length)
                {
                    _index = index;
                    return true;
                }

                return false;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset() => _index = -1;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _handle[_index];
            }
        }
    }
}