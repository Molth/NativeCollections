﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe queue
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeQueue<T> : IDisposable, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private T* _buffer;

        /// <summary>
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Head
        /// </summary>
        private int _head;

        /// <summary>
        ///     Tail
        /// </summary>
        private int _tail;

        /// <summary>
        ///     Size
        /// </summary>
        private int _size;

        /// <summary>
        ///     Version
        /// </summary>
        private int _version;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[(_head + index) % _length];
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[(_head + index) % _length];
        }

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _size == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _size;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity => _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeQueue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            _buffer = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(T)));
            _length = capacity;
            _head = 0;
            _tail = 0;
            _size = 0;
            _version = 0;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => NativeMemoryAllocator.Free(_buffer);

        /// <summary>
        ///     Clear
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
        ///     Enqueue
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in T item)
        {
            if (_size == _length)
                Grow(_size + 1);
            _buffer[_tail] = item;
            MoveNext(ref _tail);
            _size++;
            _version++;
        }

        /// <summary>
        ///     Try enqueue
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(in T item)
        {
            if (_size != _length)
            {
                _buffer[_tail] = item;
                MoveNext(ref _tail);
                _size++;
                _version++;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Dequeue
        /// </summary>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue()
        {
            if (_size == 0)
                throw new InvalidOperationException("EmptyQueue");
            var removed = _buffer[_head];
            MoveNext(ref _head);
            _size--;
            _version++;
            return removed;
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result)
        {
            if (_size == 0)
            {
                result = default;
                return false;
            }

            result = _buffer[_head];
            MoveNext(ref _head);
            _size--;
            _version++;
            return true;
        }

        /// <summary>
        ///     Peek
        /// </summary>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek() => _size == 0 ? throw new InvalidOperationException("EmptyQueue") : _buffer[_head];

        /// <summary>
        ///     Try peek
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T result)
        {
            if (_size == 0)
            {
                result = default;
                return false;
            }

            result = _buffer[_head];
            return true;
        }

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (_length < capacity)
                Grow(capacity);
            return _length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = (int)(_length * 0.9);
            if (_size < threshold)
                SetCapacity(_size);
            return _length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < _size || capacity >= _length)
                return _length;
            SetCapacity(capacity);
            return _length;
        }

        /// <summary>
        ///     Set capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCapacity(int capacity)
        {
            var newBuffer = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(T)));
            if (_size > 0)
            {
                if (_head < _tail)
                {
                    Unsafe.CopyBlockUnaligned(newBuffer, _buffer + _head, (uint)(_size * sizeof(T)));
                }
                else
                {
                    Unsafe.CopyBlockUnaligned(newBuffer, _buffer + _head, (uint)((_length - _head) * sizeof(T)));
                    Unsafe.CopyBlockUnaligned(newBuffer + _length - _head, _buffer, (uint)(_tail * sizeof(T)));
                }
            }

            NativeMemoryAllocator.Free(_buffer);
            _buffer = newBuffer;
            _length = capacity;
            _head = 0;
            _tail = _size == capacity ? 0 : _size;
            _version++;
        }

        /// <summary>
        ///     Grow
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var newCapacity = 2 * _length;
            if ((uint)newCapacity > 2147483591)
                newCapacity = 2147483591;
            var expected = _length + 4;
            newCapacity = newCapacity > expected ? newCapacity : expected;
            if (newCapacity < capacity)
                newCapacity = capacity;
            SetCapacity(newCapacity);
        }

        /// <summary>
        ///     Move next
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveNext(ref int index)
        {
            var tmp = index + 1;
            if (tmp == _length)
                tmp = 0;
            index = tmp;
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> buffer) => CopyTo(MemoryMarshal.Cast<T, byte>(buffer));

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<byte> buffer)
        {
            var size = _size;
            if (size == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var length1 = _length - _head;
            var length2 = length1 > size ? size : length1;
            Unsafe.CopyBlockUnaligned(ref reference, ref *(byte*)(_buffer + _head), (uint)(length2 * sizeof(T)));
            var length3 = size - length2;
            if (length3 <= 0)
                return;
            nint offset = length1 * sizeof(T);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref reference, offset), ref *(byte*)_buffer, (uint)(length2 * sizeof(T)));
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeQueue<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     NativeQueue
            /// </summary>
            private readonly UnsafeQueue<T>* _nativeQueue;

            /// <summary>
            ///     Version
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Current
            /// </summary>
            private T _currentElement;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeQueue">NativeQueue</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeQueue)
            {
                var handle = (UnsafeQueue<T>*)nativeQueue;
                _nativeQueue = handle;
                _version = handle->_version;
                _index = -1;
                _currentElement = default;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _nativeQueue;
                if (_version != handle->_version)
                    throw new InvalidOperationException("EnumFailedVersion");
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
                _currentElement = buffer[index];
                return true;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _index < 0 ? throw new InvalidOperationException(_index == -1 ? "EnumNotStarted" : "EnumEnded") : _currentElement;
            }
        }
    }
}