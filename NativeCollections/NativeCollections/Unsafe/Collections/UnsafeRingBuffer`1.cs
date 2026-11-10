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
    ///     Represents a fixed-size circular buffer that supports insertion and
    ///     removal from both ends, overwriting the oldest element when full.
    /// </summary>
    /// <typeparam name="T">The type of elements in the ring buffer.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeRingBuffer<T> : IIsCreated, IDisposable, IEquatable<UnsafeRingBuffer<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     The index of the head.
        /// </summary>
        private int _head;

        /// <summary>
        ///     The index of the tail.
        /// </summary>
        private int _tail;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep the enumerator synchronized with the collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Initializes an instance from its final storage state.
        /// </summary>
        /// <param name="buffer">The allocated element buffer.</param>
        /// <param name="capacity">The buffer capacity.</param>
        /// <param name="head">The index of the head.</param>
        /// <param name="tail">The index of the tail.</param>
        /// <param name="count">The number of stored elements.</param>
        /// <param name="version">The current mutation version.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeRingBuffer(T* buffer, int capacity, int head, int tail, int count, int version)
        {
            _buffer = buffer;
            _capacity = capacity;
            _head = head;
            _tail = tail;
            _count = count;
            _version = version;
        }

        /// <summary>
        ///     Creates a new instance of the class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity. Must be non-negative.</param>
        /// <returns>An unsafe ring buffer with the specified initial capacity.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <remarks>
        ///     The requested capacity is clamped to a minimum of 2; read <c>Capacity</c> for the effective value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeRingBuffer<T> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 2);
            var buffer = NativeMemoryAllocator.AlignedAlloc<T>(Usize.FromI32Unchecked(capacity));
            return new UnsafeRingBuffer<T>(buffer, capacity, 0, 0, 0, 0);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _buffer != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Gets a value that indicates whether this is full.
        /// </summary>
        /// <value><see langword="true" /> if this is full; otherwise, <see langword="false" />.</value>
        public readonly bool IsFull => Count == Capacity;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Gets a reference to the element at the specified index.
        /// </summary>
        /// <param name="index">
        ///     The zero-based index of the element to access, where index <c>0</c> refers
        ///     to the head element, which is the oldest element in the ring buffer.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        public readonly ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
                return ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), RingBufferHelpers.GetElementOffset(Isize.FromI32Unchecked(index), Isize.FromI32Unchecked(_head), Isize.FromI32Unchecked(_capacity)));
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
        public readonly bool Equals(UnsafeRingBuffer<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeRingBuffer<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeRingBuffer<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeRingBuffer<T> left, UnsafeRingBuffer<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeRingBuffer<T> left, UnsafeRingBuffer<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _count = 0;
            _head = 0;
            _tail = 0;
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Adds an item to the head of the ring buffer. If the buffer
        ///     is full, the oldest element at the head is overwritten.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was
        ///     added as <see cref="InsertResult.Success" />, or <see cref="InsertResult.Overwritten" />
        ///     if an existing element was overwritten.
        /// </returns>
        /// <remarks>
        ///     When the buffer is full, the new item takes the slot of the current head,
        ///     so the head and tail indices both stay unchanged and the previously oldest
        ///     element is the one that is discarded.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueHead(T item)
        {
            if (_count == _capacity)
            {
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_head)) = item;
                _version = _version.WrappingAdd(1);
                return InsertResult.Overwritten;
            }

            if ((_head = _head.WrappingSub(1)) == -1)
                _head = _capacity.WrappingSub(1);

            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_head)) = item;
            _count = _count.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
            return InsertResult.Success;
        }

        /// <summary>
        ///     Adds an item to the head of the ring buffer. If the buffer is full, the oldest
        ///     element at the head is overwritten and returned via <paramref name="overwritten" />.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="overwritten">
        ///     When this method returns, contains the element that was overwritten if the
        ///     buffer was full; otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was
        ///     added as <see cref="InsertResult.Success" />, or <see cref="InsertResult.Overwritten" />
        ///     if an existing element was overwritten.
        /// </returns>
        /// <remarks>
        ///     When the buffer is full, the new item takes the slot of the current head,
        ///     so the head and tail indices both stay unchanged and the previously oldest
        ///     element is the one returned through <paramref name="overwritten" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueHead(T item, out T overwritten)
        {
            if (_count == _capacity)
            {
                overwritten = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_head));
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_head)) = item;
                _version = _version.WrappingAdd(1);
                return InsertResult.Overwritten;
            }

            overwritten = default;

            if ((_head = _head.WrappingSub(1)) == -1)
                _head = _capacity.WrappingSub(1);

            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_head)) = item;
            _count = _count.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
            return InsertResult.Success;
        }

        /// <summary>
        ///     Attempts to add an item to the head of the ring buffer.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the ring buffer;
        ///     <see langword="false" /> if the ring buffer is already full and the item could not be enqueued.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueHead(T item)
        {
            if (_count == _capacity)
                return false;

            if ((_head = _head.WrappingSub(1)) == -1)
                _head = _capacity.WrappingSub(1);

            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_head)) = item;
            _count = _count.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Adds an item to the tail of the ring buffer. If the buffer
        ///     is full, the oldest element at the head is overwritten.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was
        ///     added as <see cref="InsertResult.Success" />, or <see cref="InsertResult.Overwritten" />
        ///     if an existing element was overwritten.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueTail(T item)
        {
            InsertResult result;
            if (_count == _capacity)
            {
                if ((_head = _head.WrappingAdd(1)) == _capacity)
                    _head = 0;

                result = InsertResult.Overwritten;
            }
            else
            {
                _count = _count.WrappingAdd(1);
                result = InsertResult.Success;
            }

            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_tail)) = item;
            if ((_tail = _tail.WrappingAdd(1)) == _capacity)
                _tail = 0;

            _version = _version.WrappingAdd(1);
            return result;
        }

        /// <summary>
        ///     Adds an item to the tail of the ring buffer. If the buffer is full, the oldest
        ///     element at the head is overwritten and returned via <paramref name="overwritten" />.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="overwritten">
        ///     When this method returns, contains the element that was overwritten if the
        ///     buffer was full; otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was
        ///     added as <see cref="InsertResult.Success" />, or <see cref="InsertResult.Overwritten" />
        ///     if an existing element was overwritten.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueTail(T item, out T overwritten)
        {
            InsertResult result;
            if (_count == _capacity)
            {
                overwritten = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_head));
                if ((_head = _head.WrappingAdd(1)) == _capacity)
                    _head = 0;

                result = InsertResult.Overwritten;
            }
            else
            {
                overwritten = default;
                _count = _count.WrappingAdd(1);
                result = InsertResult.Success;
            }

            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_tail)) = item;
            if ((_tail = _tail.WrappingAdd(1)) == _capacity)
                _tail = 0;

            _version = _version.WrappingAdd(1);
            return result;
        }

        /// <summary>
        ///     Attempts to add an item to the tail of the ring buffer.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the ring buffer;
        ///     <see langword="false" /> if the ring buffer is already full and the item could not be enqueued.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueTail(T item)
        {
            if (_count == _capacity)
                return false;

            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_tail)) = item;
            if ((_tail = _tail.WrappingAdd(1)) == _capacity)
                _tail = 0;

            _count = _count.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Removes the object at the beginning of this, and copies it to the <paramref name="result" /> parameter.
        /// </summary>
        /// <param name="result">The removed object.</param>
        /// <returns>
        ///     <see langword="true" /> if the object is successfully
        ///     removed; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueHead(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_head));
            if ((_head = _head.WrappingAdd(1)) == _capacity)
                _head = 0;

            _count = _count.WrappingSub(1);
            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Removes the object at the end of this, and copies it to the <paramref name="result" /> parameter.
        /// </summary>
        /// <param name="result">The removed object.</param>
        /// <returns>
        ///     <see langword="true" /> if the object is successfully
        ///     removed; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueTail(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }

            if ((_tail = _tail.WrappingSub(1)) == -1)
                _tail = _capacity.WrappingSub(1);

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_tail));
            _count = _count.WrappingSub(1);
            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Returns a value that indicates whether there is an object at the beginning of this, and if one is
        ///     present, copies it to the <paramref name="result" /> parameter. The object is not removed from this.
        /// </summary>
        /// <param name="result">
        ///     If present, the object at the beginning of this;
        ///     otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if there is an object at the
        ///     beginning of this; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryPeekHead(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_head));
            return true;
        }

        /// <summary>
        ///     Returns a value that indicates whether there is an object at the end of this, and if one is
        ///     present, copies it to the <paramref name="result" /> parameter. The object is not removed from this.
        /// </summary>
        /// <param name="result">
        ///     If present, the object at the end of this; otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if there is an object at the
        ///     end of this; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryPeekTail(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }

            var tail = _tail;
            if ((tail = tail.WrappingSub(1)) == -1)
                tail = _capacity.WrappingSub(1);

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(tail));
            return true;
        }

        /// <summary>
        ///     Copies up to the specified number of elements from this. The actual number of copied elements is
        ///     limited by the span's length, the specified count, and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<T> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var size = Math.Min(buffer.Length, Math.Min(count, _count));
            RingBufferHelpers.Copy(ref reference, ref Unsafe.AsRef<T>(_buffer), size, _capacity, _head);
            return size;
        }

        /// <summary>
        ///     Copies up to the specified number of elements from this into a destination byte span. The span's
        ///     length is measured in bytes, and each element occupies the byte size of <typeparamref name="T" />;
        ///     the actual number of copied elements is limited by the number of elements that fit in the span,
        ///     the specified count, and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination byte span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer), count);

        /// <summary>
        ///     Copies all elements from this into a destination span. The span must
        ///     have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            RingBufferHelpers.Copy(ref reference, ref Unsafe.AsRef<T>(_buffer), _count, _capacity, _head);
        }

        /// <summary>
        ///     Copies all elements from this into a destination byte span. The span's length is measured in bytes
        ///     and must be at least the byte size of all elements in this.
        /// </summary>
        /// <param name="buffer">The destination byte span to which all elements are copied.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="buffer" /> has insufficient byte length to hold all the elements of this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer));

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        [MustBePinned(SR.parameter_this)]
        public Enumerator GetEnumerator() => Enumerator.From(UnsafeHelpers.AsPointer(ref this));

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

        /// <summary>
        ///     Supports a simple iteration over a generic collection.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IIterator<T>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly UnsafeRingBuffer<T>* _handle;

            /// <summary>
            ///     Used to keep the enumerator synchronized with the collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private T _currentElement;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(UnsafeRingBuffer<T>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _index = -1;
                _currentElement = default;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(UnsafeRingBuffer<T>* handle) => new(handle);

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
            ///     <see langword="false" /> if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            ///     Thrown when the collection was modified after the enumerator was created.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _handle;
                ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                if (_index == -2)
                    return false;

                _index = _index.WrappingAdd(1);
                if (_index == handle->_count)
                {
                    _index = -2;
                    _currentElement = default;
                    return false;
                }

                var buffer = handle->_buffer;
                var capacity = U32.FromI32Unchecked(handle->_capacity);
                var index = U32.FromI32Unchecked(handle->_head.WrappingAdd(_index));
                if (index >= capacity)
                    index = index.WrappingSub(capacity);

                _currentElement = Unsafe.Add(ref Unsafe.AsRef<T>(buffer), Isize.FromU32Unchecked(index));
                return true;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                _index = -1;
                _currentElement = default;
            }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _currentElement;
            }
        }
    }
}