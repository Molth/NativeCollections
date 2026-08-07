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
    public readonly unsafe struct NativeArray<T> : IIsCreated, IDisposable, IEquatable<NativeArray<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            _buffer = NativeMemoryAllocator.AlignedAlloc<T>((uint)length);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="zeroed">Zeroed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray(int length, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            _buffer = zeroed ? NativeMemoryAllocator.AlignedAllocZeroed<T>((uint)length) : NativeMemoryAllocator.AlignedAlloc<T>((uint)length);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray(int length, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfLessThan((uint)alignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            _buffer = (T*)NativeMemoryAllocator.AlignedAlloc((uint)(length * Unsafe.SizeOf<T>()), (uint)alignment);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="alignment">Alignment</param>
        /// <param name="zeroed">Zeroed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray(int length, int alignment, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfLessThan((uint)alignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            _buffer = zeroed ? (T*)NativeMemoryAllocator.AlignedAllocZeroed((uint)(length * Unsafe.SizeOf<T>()), (uint)alignment) : (T*)NativeMemoryAllocator.AlignedAlloc((uint)(length * Unsafe.SizeOf<T>()), (uint)alignment);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray(T* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            _buffer = buffer;
            _length = length;
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index);
        }

        /// <summary>
        ///     Buffer
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
        public bool Equals(NativeArray<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeArray<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeArray<{0}>[{1}]", SR.GetTypeName(typeof(T)), _length);

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        /// <returns>Pointer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(NativeArray<T> value) => value._buffer;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativeArray<T> value) => value.AsSpan();

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativeArray<T> value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(value))]
        public static implicit operator NativeArray<T>([MustBePinned] Span<T> value) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), value.Length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(value))]
        public static implicit operator NativeArray<T>([MustBePinned] ReadOnlySpan<T> value) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), value.Length);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeArray<T> left, NativeArray<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeArray<T> left, NativeArray<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Free(_buffer);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => SpanHelpers.Set(ref Unsafe.AsRef<byte>(_buffer), 0, (uint)(_length * Unsafe.SizeOf<T>()));

        /// <summary>
        ///     Casts a instance of one primitive type to another primitive type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<TTo> Cast<TTo>() where TTo : unmanaged => MemoryMarshal.Cast<T, TTo>(this);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_buffer), _length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), _length - start);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_buffer), _length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), _length - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), length);

        /// <summary>
        ///     Forms a slice out of the given span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> Slice(int start) => new(UnsafeHelpers.Add<T>(_buffer, start), _length - start);

        /// <summary>
        ///     Forms a slice out of the given span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> Slice(int start, int length) => new(UnsafeHelpers.Add<T>(_buffer, start), length);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeArray<T> Empty => default;

        /// <summary>
        ///     Calculates the minimum number of bytes required to store a specified number of elements,
        ///     taking into account alignment requirements for the underlying buffer.
        /// </summary>
        /// <param name="capacity">The number of elements to store. Must be non-negative.</param>
        /// <returns>
        ///     The minimum byte count needed to allocate a buffer capable of
        ///     holding <paramref name="capacity" /> elements with proper alignment.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity) => capacity * Unsafe.SizeOf<T>() + (int)NativeMemoryAllocator.AlignOf<T>() - 1;

        /// <summary>
        ///     Creates a <see cref="NativeArray{T}" /> from a given byte span, aligning the start of the array
        ///     to the specified alignment boundary. The alignment must be a power of two and at least the natural alignment
        ///     of <typeparamref name="T" />.
        /// </summary>
        /// <param name="buffer">The source byte span to interpret as an array of <typeparamref name="T" />.</param>
        /// <param name="alignment">The required alignment in bytes, which must be a power of two.</param>
        /// <returns>
        ///     A <see cref="NativeArray{T}" /> that references the aligned portion of <paramref name="buffer" />
        ///     containing elements of type <typeparamref name="T" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(buffer))]
        public static NativeArray<T> Create([MustBePinned] ReadOnlySpan<byte> buffer, uint alignment)
        {
            ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);
            var ptr = Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
            var alignedPtr = (nint)NativeMemoryAllocator.AlignUp((nuint)(nint)ptr, alignment);
            var byteOffset = alignedPtr - (nint)ptr;
            var alignedBuffer = MemoryMarshal.Cast<byte, T>(buffer.Slice((int)byteOffset));
            return new NativeArray<T>(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(alignedBuffer)), alignedBuffer.Length);
        }

        /// <summary>
        ///     Creates a <see cref="NativeArray{T}" /> from a given byte span, aligning the start of the array
        ///     to the specified alignment boundary, and also returns the byte offset used to achieve alignment.
        /// </summary>
        /// <param name="buffer">The source byte span to interpret as an array of <typeparamref name="T" />.</param>
        /// <param name="alignment">The required alignment in bytes, which must be a power of two.</param>
        /// <param name="byteOffset">
        ///     When this method returns, contains the offset in bytes from the start of <paramref name="buffer" />
        ///     to the aligned start of the returned <see cref="NativeArray{T}" />.
        /// </param>
        /// <returns>
        ///     A <see cref="NativeArray{T}" /> that references the aligned portion of <paramref name="buffer" />
        ///     containing elements of type <typeparamref name="T" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(buffer))]
        public static NativeArray<T> Create([MustBePinned] ReadOnlySpan<byte> buffer, uint alignment, out nint byteOffset)
        {
            ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);
            var ptr = Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
            var alignedPtr = (nint)NativeMemoryAllocator.AlignUp((nuint)(nint)ptr, alignment);
            byteOffset = alignedPtr - (nint)ptr;
            var alignedBuffer = MemoryMarshal.Cast<byte, T>(buffer.Slice((int)byteOffset));
            return new NativeArray<T>(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(alignedBuffer)), alignedBuffer.Length);
        }

        /// <summary>
        ///     Creates a <see cref="NativeArray{T}" /> from a given byte span, using the natural alignment
        ///     of <typeparamref name="T" /> for the array start.
        /// </summary>
        /// <param name="buffer">The source byte span to interpret as an array of <typeparamref name="T" />.</param>
        /// <returns>
        ///     A <see cref="NativeArray{T}" /> that references the aligned portion of <paramref name="buffer" />
        ///     containing elements of type <typeparamref name="T" />, aligned to the natural alignment of
        ///     <typeparamref name="T" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(buffer))]
        public static NativeArray<T> Create([MustBePinned] ReadOnlySpan<byte> buffer) => Create(buffer, NativeMemoryAllocator.AlignOf<T>());

        /// <summary>
        ///     Creates a <see cref="NativeArray{T}" /> from a given byte span, using the natural alignment
        ///     of <typeparamref name="T" />, and returns the byte offset used to achieve alignment.
        /// </summary>
        /// <param name="buffer">The source byte span to interpret as an array of <typeparamref name="T" />.</param>
        /// <param name="byteOffset">
        ///     When this method returns, contains the offset in bytes from the start of <paramref name="buffer" />
        ///     to the aligned start of the returned <see cref="NativeArray{T}" />.
        /// </param>
        /// <returns>
        ///     A <see cref="NativeArray{T}" /> that references the aligned portion of <paramref name="buffer" />
        ///     containing elements of type <typeparamref name="T" />, aligned to the natural alignment of
        ///     <typeparamref name="T" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(buffer))]
        public static NativeArray<T> Create([MustBePinned] ReadOnlySpan<byte> buffer, out nint byteOffset) => Create(buffer, NativeMemoryAllocator.AlignOf<T>(), out byteOffset);

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public Enumerator GetEnumerator() => new(this);

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

        /// <summary>
        ///     Enumerator
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IRefIterator<T>
        {
            /// <summary>
            ///     NativeArray
            /// </summary>
            private readonly NativeArray<T> _handle;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeArray<T> handle)
            {
                _handle = handle;
                _index = -1;
            }

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
                var index = _index + 1;
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _index = -1;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _handle[_index];
            }
        }
    }
}