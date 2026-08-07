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
    ///     Represents a variable size last-in-first-out (LIFO) collection of instances of the same specified type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeStack<T> : IIsCreated, IDisposable, IEquatable<UnsafeStack<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private T* _buffer;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        private int _length;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _size;

        /// <summary>
        ///     Used to keep enumerator in sync w/ collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _size == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _size;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeStack(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 4);
            _buffer = NativeMemoryAllocator.AlignedAlloc<T>((uint)capacity);
            _length = capacity;
            _size = 0;
            _version = 0;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeStack<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeStack<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeStack<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeStack<T> left, UnsafeStack<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeStack<T> left, UnsafeStack<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _size = 0;
            _version++;
        }

        /// <summary>
        ///     Pushes an item to the top of the stack.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(in T item)
        {
            var size = _size;
            if ((uint)size < (uint)_length)
            {
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)size) = item;
                _version++;
                _size = size + 1;
            }
            else
            {
                Grow(_size + 1);
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_size) = item;
                _version++;
                _size++;
            }
        }

        /// <summary>
        ///     Attempts to push an item to the top of the stack.
        /// </summary>
        /// <param name="item">The item to push.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully pushed to the stack;
        ///     <see langword="false" /> if the stack is already full and the item could not be pushed.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPush(in T item)
        {
            var size = _size;
            if ((uint)size < (uint)_length)
            {
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)size) = item;
                _version++;
                _size = size + 1;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Removes and returns the object at the top of this.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">this is empty.</exception>
        /// <returns>The object removed from the top of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
            var size = _size - 1;
            ThrowHelpers.ThrowIfEmptyStack((uint)size, (uint)_length);
            _version++;
            _size = size;
            var item = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)size);
            return item;
        }

        /// <summary>
        ///     Removes the object at the top of this, and copies it to the <paramref name="result" /> parameter.
        /// </summary>
        /// <param name="result">The removed object.</param>
        /// <returns>
        ///     <see langword="true" /> if the object is successfully removed;
        ///     <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result)
        {
            var size = _size - 1;
            if ((uint)size >= (uint)_length)
            {
                result = default;
                return false;
            }

            _version++;
            _size = size;
            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)size);
            return true;
        }

        /// <summary>
        ///     Returns the object at the top of this without removing it.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">this is empty.</exception>
        /// <returns>The object at the top of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T Peek()
        {
            var size = _size - 1;
            ThrowHelpers.ThrowIfEmptyStack((uint)size, (uint)_length);
            return Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)size);
        }

        /// <summary>
        ///     Returns a value that indicates whether there is an object at the top of this,
        ///     and if one is present, copies it to the <paramref name="result" /> parameter.
        ///     The object is not removed from this.
        /// </summary>
        /// <param name="result">
        ///     If present, the object at the top of this;
        ///     otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if there is an object at the top of this;
        ///     <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryPeek(out T result)
        {
            var size = _size - 1;
            if ((uint)size >= (uint)_length)
            {
                result = default;
                return false;
            }

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)size);
            return true;
        }

        /// <summary>
        ///     Ensures that the capacity of this is at least the specified <paramref name="capacity" />.
        ///     If the current capacity of this is less than specified <paramref name="capacity" />,
        ///     the capacity is increased to at least <paramref name="capacity" />.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (_length < capacity)
                Grow(capacity);
            return _length;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = (int)(_length * 0.9);
            if (_size < threshold)
                SetCapacity(_size);
            return _length;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (capacity < _size || capacity >= _length)
                return _length;
            SetCapacity(capacity);
            return _length;
        }

        /// <summary>
        ///     Sets the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCapacity(int capacity)
        {
            var newBuffer = NativeMemoryAllocator.AlignedAlloc<T>((uint)capacity);
            if (_size > 0)
                SpanHelpers.Copy(ref Unsafe.AsRef<byte>(newBuffer), ref Unsafe.AsRef<byte>(_buffer), (uint)(_size * Unsafe.SizeOf<T>()));
            NativeMemoryAllocator.AlignedFree(_buffer);
            _buffer = newBuffer;
            _length = capacity;
        }

        /// <summary>
        ///     Grow
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var newCapacity = 2 * _length;
            if ((uint)newCapacity > ArrayHelpers.MaxLength)
                newCapacity = ArrayHelpers.MaxLength;
            var expected = _length + 4;
            newCapacity = Math.Max(newCapacity, expected);
            newCapacity = Math.Max(newCapacity, capacity);
            SetCapacity(newCapacity);
        }

        /// <summary>
        ///     Copies up to the specified number of elements from this.
        ///     The actual number of copied elements is limited by the span's length, the specified count,
        ///     and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from the this.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<T> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var size = Math.Min(buffer.Length, Math.Min(count, _size));
            StackHelpers.Copy(ref reference, ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(_size - size)), size);
            return size;
        }

        /// <summary>
        ///     Copies up to the specified number of elements from this.
        ///     The actual number of copied elements is limited by the span's length, the specified count,
        ///     and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from the this.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer), count);

        /// <summary>
        ///     Copies all elements from this into a destination span.
        ///     The span must have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            StackHelpers.Copy(ref reference, ref Unsafe.AsRef<T>(_buffer), _size);
        }

        /// <summary>
        ///     Copies all elements from this into a destination span.
        ///     The span must have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer));

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeStack<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public Enumerator GetEnumerator() => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
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
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Enumerator
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IIterator<T>
        {
            /// <summary>
            ///     NativeStack
            /// </summary>
            private readonly UnsafeStack<T>* _handle;

            /// <summary>
            ///     Used to keep enumerator in sync w/ collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Current element
            /// </summary>
            private T _currentElement;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UnsafeStack<T>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _index = -2;
                _currentElement = default;
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
                var handle = _handle;
                ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                bool returned;
                if (_index == -2)
                {
                    _index = handle->_size - 1;
                    returned = _index >= 0;
                    if (returned)
                        _currentElement = Unsafe.Add(ref Unsafe.AsRef<T>(handle->_buffer), (nint)_index);
                    return returned;
                }

                if (_index == -1)
                    return false;
                returned = --_index >= 0;
                _currentElement = returned ? Unsafe.Add(ref Unsafe.AsRef<T>(handle->_buffer), (nint)_index) : default;
                return returned;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _index = -2;
                _currentElement = default;
            }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _currentElement;
            }
        }
    }
}