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
    ///     Represents a fixed-size circular buffer that supports insertion and removal from both ends,
    ///     overwriting the oldest element when full.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeRingBuffer<T> : IIsCreated, IDisposable, IEquatable<UnsafeRingBuffer<T>>, IReadOnlyCollection<T> where T : unmanaged
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
        ///     Head
        /// </summary>
        private int _head;

        /// <summary>
        ///     Tail
        /// </summary>
        private int _tail;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _size;

        /// <summary>
        ///     Used to keep enumerator in sync w/ collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _size == 0;

        /// <summary>
        ///     Returns `true` if the queue is full.
        /// </summary>
        public readonly bool IsFull => Count == Capacity;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _size;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _length;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), RingBufferHelpers.GetElementOffset(index, _head, _length));
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), RingBufferHelpers.GetElementOffset((nint)index, _head, _length));
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeRingBuffer(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 4);
            _buffer = NativeMemoryAllocator.AlignedAlloc<T>((uint)capacity);
            _length = capacity;
            _head = 0;
            _tail = 0;
            _size = 0;
            _version = 0;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeRingBuffer<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeRingBuffer<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeRingBuffer<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeRingBuffer<T> left, UnsafeRingBuffer<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeRingBuffer<T> left, UnsafeRingBuffer<T> right) => !left.Equals(right);

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
            _head = 0;
            _tail = 0;
            _version++;
        }

        /// <summary>
        ///     Adds an item to the head of the ring buffer.
        ///     If the buffer is full, the oldest element at the tail is overwritten.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was added successfully
        ///     <see cref="InsertResult.Success" /> or if an existing element was overwritten
        ///     <see cref="InsertResult.Overwritten" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueHead(in T item)
        {
            InsertResult result;
            if (_size == _length)
            {
                if (--_tail == -1)
                    _tail = _length - 1;
                result = InsertResult.Overwritten;
            }
            else
            {
                ++_size;
                result = InsertResult.Success;
            }

            if (--_head == -1)
                _head = _length - 1;
            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head) = item;
            ++_version;
            return result;
        }

        /// <summary>
        ///     Adds an item to the head of the ring buffer.
        ///     If the buffer is full, the oldest element at the tail is overwritten and returned via
        ///     <paramref name="overwritten" />.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="overwritten">
        ///     When this method returns, contains the element that was overwritten if the buffer was full;
        ///     otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was added successfully
        ///     <see cref="InsertResult.Success" /> or if an existing element was overwritten
        ///     <see cref="InsertResult.Overwritten" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueHead(in T item, out T overwritten)
        {
            InsertResult result;
            if (_size == _length)
            {
                if (--_tail == -1)
                    _tail = _length - 1;
                overwritten = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_tail);
                result = InsertResult.Overwritten;
            }
            else
            {
                overwritten = default;
                ++_size;
                result = InsertResult.Success;
            }

            if (--_head == -1)
                _head = _length - 1;
            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head) = item;
            ++_version;
            return result;
        }

        /// <summary>
        ///     Attempts to add an item to the head of the queue.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the queue;
        ///     <see langword="false" /> if the queue is already full and the item could not be enqueued.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueHead(in T item)
        {
            if (_size == _length)
                return false;
            if (--_head == -1)
                _head = _length - 1;
            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head) = item;
            ++_size;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Adds an item to the tail of the ring buffer.
        ///     If the buffer is full, the oldest element at the head is overwritten.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was added successfully
        ///     <see cref="InsertResult.Success" /> or if an existing element was overwritten
        ///     <see cref="InsertResult.Overwritten" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueTail(in T item)
        {
            InsertResult result;
            if (_size == _length)
            {
                if (++_head == _length)
                    _head = 0;
                result = InsertResult.Overwritten;
            }
            else
            {
                ++_size;
                result = InsertResult.Success;
            }

            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_tail) = item;
            if (++_tail == _length)
                _tail = 0;
            ++_version;
            return result;
        }

        /// <summary>
        ///     Adds an item to the tail of the ring buffer.
        ///     If the buffer is full, the oldest element at the head is overwritten and returned via
        ///     <paramref name="overwritten" />.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="overwritten">
        ///     When this method returns, contains the element that was overwritten if the buffer was full;
        ///     otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was added successfully
        ///     <see cref="InsertResult.Success" /> or if an existing element was overwritten
        ///     <see cref="InsertResult.Overwritten" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueTail(in T item, out T overwritten)
        {
            InsertResult result;
            if (_size == _length)
            {
                overwritten = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head);
                if (++_head == _length)
                    _head = 0;
                result = InsertResult.Overwritten;
            }
            else
            {
                overwritten = default;
                ++_size;
                result = InsertResult.Success;
            }

            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_tail) = item;
            if (++_tail == _length)
                _tail = 0;
            ++_version;
            return result;
        }

        /// <summary>
        ///     Attempts to add an item to the tail of the queue.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the queue;
        ///     <see langword="false" /> if the queue is already full and the item could not be enqueued.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueTail(in T item)
        {
            if (_size == _length)
                return false;
            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_tail) = item;
            if (++_tail == _length)
                _tail = 0;
            ++_size;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Removes the object at the beginning of this, and copies it to the <paramref name="result" /> parameter.
        /// </summary>
        /// <param name="result">The removed object.</param>
        /// <returns>
        ///     <see langword="true" /> if the object is successfully removed;
        ///     <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueHead(out T result)
        {
            if (_size == 0)
            {
                result = default;
                return false;
            }

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head);
            if (++_head == _length)
                _head = 0;
            --_size;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Removes the object at the ending of this, and copies it to the <paramref name="result" /> parameter.
        /// </summary>
        /// <param name="result">The removed object.</param>
        /// <returns>
        ///     <see langword="true" /> if the object is successfully removed;
        ///     <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueTail(out T result)
        {
            if (_size == 0)
            {
                result = default;
                return false;
            }

            if (--_tail == -1)
                _tail = _length - 1;
            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_tail);
            --_size;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Returns a value that indicates whether there is an object at the beginning of this,
        ///     and if one is present, copies it to the <paramref name="result" /> parameter.
        ///     The object is not removed from this.
        /// </summary>
        /// <param name="result">
        ///     If present, the object at the beginning of this;
        ///     otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if there is an object at the beginning of this;
        ///     <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryPeekHead(out T result)
        {
            if (_size == 0)
            {
                result = default;
                return false;
            }

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head);
            return true;
        }

        /// <summary>
        ///     Returns a value that indicates whether there is an object at the ending of this,
        ///     and if one is present, copies it to the <paramref name="result" /> parameter.
        ///     The object is not removed from this.
        /// </summary>
        /// <param name="result">
        ///     If present, the object at the ending of this;
        ///     otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if there is an object at the ending of this;
        ///     <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryPeekTail(out T result)
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
            RingBufferHelpers.Copy(ref reference, ref Unsafe.AsRef<T>(_buffer), size, _length, _head);
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
            RingBufferHelpers.Copy(ref reference, ref Unsafe.AsRef<T>(_buffer), _size, _length, _head);
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
        public static UnsafeRingBuffer<T> Empty => default;

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
            ///     NativeDeque
            /// </summary>
            private readonly UnsafeRingBuffer<T>* _handle;

            /// <summary>
            ///     Used to keep enumerator in sync w/ collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private T _currentElement;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UnsafeRingBuffer<T>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _index = -1;
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
                if (_index == -2)
                    return false;
                _index++;
                if (_index == handle->_size)
                {
                    _index = -2;
                    _currentElement = default;
                    return false;
                }

                var buffer = handle->_buffer;
                var capacity = (uint)handle->_length;
                var index = (uint)(handle->_head + _index);
                if (index >= capacity)
                    index -= capacity;
                _currentElement = Unsafe.Add(ref Unsafe.AsRef<T>(buffer), (nint)index);
                return true;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _index = -1;
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