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
    /// <typeparam name="T">The type of elements in the stack.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeStack<T> : IIsCreated, IDisposable, IEquatable<UnsafeStack<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private T* _buffer;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private int _capacity;

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
        /// <param name="count">The number of stored elements.</param>
        /// <param name="version">The current mutation version.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeStack(T* buffer, int capacity, int count, int version)
        {
            _buffer = buffer;
            _capacity = capacity;
            _count = count;
            _version = version;
        }

        /// <summary>
        ///     Creates a new instance of the class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity. Must be non-negative.</param>
        /// <returns>An unsafe stack with the specified initial capacity.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <remarks>
        ///     The requested capacity is clamped to a minimum of 2; read <c>Capacity</c> for the effective value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeStack<T> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 2);
            var buffer = NativeMemoryAllocator.AlignedAlloc<T>(Usize.FromI32Unchecked(capacity));
            return new UnsafeStack<T>(buffer, capacity, 0, 0);
        }

        /// <summary>
        ///     Gets a reference to the element at the specified index.
        /// </summary>
        /// <param name="index">
        ///     The zero-based index of the element to access, where index
        ///     <c>0</c> refers to the element at the bottom of the stack.
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
                return ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index));
            }
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
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeStack<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeStack<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeStack<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeStack<T> left, UnsafeStack<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeStack<T> left, UnsafeStack<T> right) => !left.Equals(right);

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
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Pushes an item to the top of the stack.
        /// </summary>
        /// <param name="item">The item to add to the top of the stack.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T item)
        {
            var size = _count;
            if (U32.FromI32Unchecked(size) < U32.FromI32Unchecked(_capacity))
            {
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(size)) = item;
                _version = _version.WrappingAdd(1);
                _count = size.WrappingAdd(1);
            }
            else
            {
                Grow(_count.WrappingAdd(1));
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_count)) = item;
                _version = _version.WrappingAdd(1);
                _count = _count.WrappingAdd(1);
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
        public bool TryPush(T item)
        {
            var size = _count;
            if (U32.FromI32Unchecked(size) < U32.FromI32Unchecked(_capacity))
            {
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(size)) = item;
                _version = _version.WrappingAdd(1);
                _count = size.WrappingAdd(1);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Removes and returns the object at the top of this.
        /// </summary>
        /// <returns>The object removed from the top of this.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
            var size = _count.WrappingSub(1);
            ThrowHelpers.ThrowIfEmptyStack(U32.FromI32Unchecked(size), U32.FromI32Unchecked(_capacity));
            _version = _version.WrappingAdd(1);
            _count = size;
            var item = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(size));
            return item;
        }

        /// <summary>
        ///     Removes the object at the top of this, and copies it to the <paramref name="result" /> parameter.
        /// </summary>
        /// <param name="result">The removed object.</param>
        /// <returns>
        ///     <see langword="true" /> if the object is successfully
        ///     removed; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result)
        {
            var size = _count.WrappingSub(1);
            if (U32.FromI32Unchecked(size) >= U32.FromI32Unchecked(_capacity))
            {
                result = default;
                return false;
            }

            _version = _version.WrappingAdd(1);
            _count = size;
            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(size));
            return true;
        }

        /// <summary>
        ///     Returns the object at the top of this without removing it.
        /// </summary>
        /// <returns>The object at the top of this.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T Peek()
        {
            var size = _count.WrappingSub(1);
            ThrowHelpers.ThrowIfEmptyStack(U32.FromI32Unchecked(size), U32.FromI32Unchecked(_capacity));
            return Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(size));
        }

        /// <summary>
        ///     Returns a value that indicates whether there is an object at the top of this, and if one is
        ///     present, copies it to the <paramref name="result" /> parameter. The object is not removed from this.
        /// </summary>
        /// <param name="result">
        ///     If present, the object at the top of this; otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if there is an object at the
        ///     top of this; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryPeek(out T result)
        {
            var size = _count.WrappingSub(1);
            if (U32.FromI32Unchecked(size) >= U32.FromI32Unchecked(_capacity))
            {
                result = default;
                return false;
            }

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(size));
            return true;
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
            if (_capacity < capacity)
                Grow(capacity);

            return _capacity;
        }

        /// <summary>
        ///     Trims the excess capacity of this based on the current count.
        /// </summary>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = I32.FromF64Unchecked(F64.FromI32Unchecked(_capacity) * 0.9);
            if (_count < threshold)
                SetCapacity(_count);

            return _capacity;
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
            if (capacity < _count || capacity >= _capacity)
                return _capacity;

            SetCapacity(capacity);
            return _capacity;
        }

        /// <summary>
        ///     Sets the capacity of this to the specified number of elements.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCapacity(int capacity)
        {
            var newBuffer = NativeMemoryAllocator.AlignedAlloc<T>(Usize.FromI32Unchecked(capacity));
            if (_count > 0)
                SpanHelpers.Copy(ref Unsafe.AsRef<byte>(newBuffer), ref Unsafe.AsRef<byte>(_buffer), Usize.FromI32Unchecked(_count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

            NativeMemoryAllocator.AlignedFree(_buffer);
            _buffer = newBuffer;
            _capacity = capacity;
        }

        /// <summary>
        ///     Increases the capacity of this to a new size that is at least the specified minimum capacity.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var newCapacity = CollectionHelpers.GrowCapacity(_capacity, capacity);
            SetCapacity(newCapacity);
        }

        /// <summary>
        ///     Copies up to the specified number of elements from this, starting at the top of the stack and
        ///     proceeding in LIFO order, so the first element of the destination span is the current top of the
        ///     stack. The actual number of copied elements is limited by the span's length, the specified count,
        ///     and the current number of elements in this.
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
            StackHelpers.Copy(ref reference, ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_count.WrappingSub(size))), size);
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
        ///     Copies all elements from this into a destination span in LIFO order, starting at the top of the
        ///     stack, so the first element of the destination span is the current top of the stack. The span
        ///     must have a length at least equal to the current number of elements in this.
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
            StackHelpers.Copy(ref reference, ref Unsafe.AsRef<T>(_buffer), _count);
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
            private readonly UnsafeStack<T>* _handle;

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
            private T _current;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(UnsafeStack<T>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _index = -2;
                _current = default;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(UnsafeStack<T>* handle) => new(handle);

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
                bool returned;
                if (_index == -2)
                {
                    _index = handle->_count.WrappingSub(1);
                    returned = _index >= 0;
                    if (returned)
                        _current = Unsafe.Add(ref Unsafe.AsRef<T>(handle->_buffer), Isize.FromI32Unchecked(_index));

                    return returned;
                }

                if (_index == -1)
                    return false;

                returned = (_index = _index.WrappingSub(1)) >= 0;
                _current = returned ? Unsafe.Add(ref Unsafe.AsRef<T>(handle->_buffer), Isize.FromI32Unchecked(_index)) : default;
                return returned;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                _index = -2;
                _current = default;
            }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}