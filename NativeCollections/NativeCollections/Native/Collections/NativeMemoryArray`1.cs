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
    public readonly unsafe struct NativeMemoryArray<T> : IIsCreated, IDisposable, IEquatable<NativeMemoryArray<T>>, IReadOnlyCollection<T> where T : unmanaged
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
        ///     Initializes a new instance of this class with the specified number of elements, using the natural
        ///     alignment of <typeparamref name="T" /> and without zero-initializing the allocated memory.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <returns>A newly allocated memory array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMemoryArray<T> Create(int length)
        {
            var array = NativeArray<T>.Create(length);
            return new NativeMemoryArray<T>(array.Buffer, array.Length);
        }

        /// <summary>
        ///     Initializes a new instance of this class with the specified number of elements, using the
        ///     natural alignment of <typeparamref name="T" /> and optionally zero-initializing the memory.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <param name="zeroed">
        ///     <see langword="true" /> to zero-initialize the allocated
        ///     memory; otherwise, the memory content is undefined.
        /// </param>
        /// <returns>A newly allocated memory array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMemoryArray<T> Create(int length, bool zeroed)
        {
            var array = NativeArray<T>.Create(length, zeroed);
            return new NativeMemoryArray<T>(array.Buffer, array.Length);
        }

        /// <summary>
        ///     Initializes a new instance of this class with the specified number of
        ///     elements and alignment, without zero-initializing the allocated memory.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <param name="alignment">
        ///     The required alignment in bytes, which must be a power of two
        ///     and at least the natural alignment of <typeparamref name="T" />.
        /// </param>
        /// <returns>A newly allocated memory array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> or <paramref name="alignment" /> is negative or
        ///     <paramref name="alignment" /> is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMemoryArray<T> Create(int length, int alignment)
        {
            var array = NativeArray<T>.Create(length, alignment);
            return new NativeMemoryArray<T>(array.Buffer, array.Length);
        }

        /// <summary>
        ///     Initializes a new instance of this class with the specified
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
        /// <returns>A newly allocated memory array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> or <paramref name="alignment" /> is negative or
        ///     <paramref name="alignment" /> is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMemoryArray<T> Create(int length, int alignment, bool zeroed)
        {
            var array = NativeArray<T>.Create(length, alignment, zeroed);
            return new NativeMemoryArray<T>(array.Buffer, array.Length);
        }

        /// <summary>
        ///     Initializes a new instance of this class that wraps an existing native memory buffer.
        /// </summary>
        /// <param name="buffer">A pointer to the existing native memory buffer.</param>
        /// <param name="length">The number of elements in the buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeMemoryArray(T* buffer, int length)
        {
            _buffer = buffer;
            _length = length;
        }

        /// <summary>
        ///     Creates an array view over an existing native buffer.
        /// </summary>
        /// <param name="buffer">The native buffer to wrap.</param>
        /// <param name="length">The number of elements in the buffer.</param>
        /// <returns>
        ///     A native memory array that wraps <paramref name="buffer" /> and has <paramref name="length" /> elements.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMemoryArray<T> From(T* buffer, int length)
        {
            var array = NativeArray<T>.From(buffer, length);
            return new NativeMemoryArray<T>(array.Buffer, array.Length);
        }

        /// <summary>
        ///     Creates a new instance that references the specified span.
        /// </summary>
        /// <param name="value">The pinned span to wrap.</param>
        /// <returns>A native memory array that references the memory of <paramref name="value" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMemoryArray<T> From([MustBePinned] Span<T> value) => From(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), value.Length);

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _buffer != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        ///     Gets a pointer to the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to access.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Length" />.
        /// </exception>
        public T* this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_length), ExceptionArgument.index);
                return UnsafeHelpers.Add<T>(_buffer, Isize.FromI32Unchecked(index));
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
        public bool Equals(NativeMemoryArray<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeMemoryArray<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeMemoryArray<{0}>[{1}]", SR.GetTypeName(typeof(T)), _length);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeMemoryArray<T> left, NativeMemoryArray<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeMemoryArray<T> left, NativeMemoryArray<T> right) => !left.Equals(right);

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
        ///     Casts an instance of one primitive type to another primitive type.
        /// </summary>
        /// <typeparam name="TTo">The type of the elements in the returned view.</typeparam>
        /// <returns>
        ///     A native memory array that views the same memory reinterpreted as <typeparamref name="TTo" /> elements.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMemoryArray<TTo> Cast<TTo>() where TTo : unmanaged => NativeMemoryArray<TTo>.From(MemoryMarshal.Cast<T, TTo>(AsSpan()));

        /// <summary>
        ///     Returns a span over the entire contents of this.
        /// </summary>
        /// <returns>A span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_buffer), _length);

        /// <summary>
        ///     Returns a read-only span over the entire contents of this.
        /// </summary>
        /// <returns>A read-only span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_buffer), _length);

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
        public struct Enumerator : IPtrIterator<T>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly NativeMemoryArray<T> _handle;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(NativeMemoryArray<T> handle)
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
            internal static Enumerator From(NativeMemoryArray<T> handle) => new(handle);

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
            public readonly T* Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _handle[_index];
            }
        }
    }
}