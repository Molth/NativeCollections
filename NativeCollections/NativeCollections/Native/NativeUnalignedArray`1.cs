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
    public readonly unsafe struct NativeUnalignedArray<T> : IIsCreated, IDisposable, IEquatable<NativeUnalignedArray<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified number of elements,
        ///     using the natural alignment of <typeparamref name="T" />
        ///     and without zero-initializing the allocated memory.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="length" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeUnalignedArray(int length) => this = new NativeArray<T>(length);

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified number of elements,
        ///     using the natural alignment of <typeparamref name="T" /> and optionally zero-initializing the memory.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <param name="zeroed">
        ///     <see langword="true" /> to zero-initialize the allocated memory;
        ///     otherwise, the memory content is undefined.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="length" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeUnalignedArray(int length, bool zeroed) => this = new NativeArray<T>(length, zeroed);

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified number of elements and alignment,
        ///     without zero-initializing the allocated memory.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <param name="alignment">
        ///     The required alignment in bytes,
        ///     which must be a power of two and at least the natural alignment of <typeparamref name="T" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="length" /> or <paramref name="alignment" /> is negative,
        ///     or if <paramref name="alignment" /> is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="alignment" /> is not a power of two.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeUnalignedArray(int length, int alignment) => this = new NativeArray<T>(length, alignment);

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified number of elements, alignment, and zero-initialization option.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <param name="alignment">
        ///     The required alignment in bytes,
        ///     which must be a power of two and at least the natural alignment of <typeparamref name="T" />.
        /// </param>
        /// <param name="zeroed">
        ///     <see langword="true" /> to zero-initialize the allocated memory;
        ///     otherwise, the memory content is undefined.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="length" /> or <paramref name="alignment" /> is negative,
        ///     or if <paramref name="alignment" /> is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="alignment" /> is not a power of two.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeUnalignedArray(int length, int alignment, bool zeroed) => this = new NativeArray<T>(length, alignment, zeroed);

        /// <summary>
        ///     Initializes a new instance of this class
        ///     that wraps an existing native memory buffer.
        /// </summary>
        /// <param name="buffer">A pointer to the existing native memory buffer.</param>
        /// <param name="length">The number of elements in the buffer.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="length" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeUnalignedArray(T* buffer, int length) => this = new NativeArray<T>(buffer, length);

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
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnsafeHelpers.ReadUnaligned(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index), value);
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
        public bool Equals(NativeUnalignedArray<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeUnalignedArray<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeUnalignedArray<{0}>[{1}]", SR.GetTypeName(typeof(T)), _length);

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(NativeUnalignedArray<T> value) => value._buffer;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativeUnalignedArray<T> value) => value.AsSpan();

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativeUnalignedArray<T> value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeUnalignedArray<T>(NativeArray<T> value) => new(value.Buffer, value.Length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeUnalignedArray<T>(NativeMemoryArray<T> value) => new(value.Buffer, value.Length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(value))]
        public static implicit operator NativeUnalignedArray<T>([MustBePinned] Span<T> value) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), value.Length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(value))]
        public static implicit operator NativeUnalignedArray<T>([MustBePinned] ReadOnlySpan<T> value) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), value.Length);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeUnalignedArray<T> left, NativeUnalignedArray<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeUnalignedArray<T> left, NativeUnalignedArray<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Free(_buffer);

        /// <summary>
        ///     Returns a reference to the 0th element of the Span. If the Span is empty, returns null reference.
        ///     It can be used for pinning and is required to support the use of span within a fixed statement.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetPinnableReference() => ref Unsafe.AsRef<T>(_buffer);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => AsSpan().Clear();

        /// <summary>
        ///     Casts a instance of one primitive type to another primitive type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeUnalignedArray<TTo> Cast<TTo>() where TTo : unmanaged => MemoryMarshal.Cast<T, TTo>(this);

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
        public NativeUnalignedArray<T> Slice(int start) => new(UnsafeHelpers.Add<T>(_buffer, start), _length - start);

        /// <summary>
        ///     Forms a slice out of the given span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeUnalignedArray<T> Slice(int start, int length) => new(UnsafeHelpers.Add<T>(_buffer, start), length);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeUnalignedArray<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public Enumerator GetEnumerator() => new(this);

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
        public struct Enumerator : IIterator<T>
        {
            /// <summary>
            ///     NativeUnalignedArray
            /// </summary>
            private readonly NativeUnalignedArray<T> _nativeArray;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeUnalignedArray<T> nativeUnalignedArray)
            {
                _nativeArray = nativeUnalignedArray;
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
                if (index < _nativeArray._length)
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
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _nativeArray[_index];
            }
        }
    }
}