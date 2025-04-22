using System;
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
    ///     Unsafe ringBuffer
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeRingBuffer<T> : IDisposable, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Length
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
        ///     Size
        /// </summary>
        private int _size;

        /// <summary>
        ///     Version
        /// </summary>
        private int _version;

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
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeRingBuffer(int capacity)
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
        ///     Enqueue head
        /// </summary>
        /// <param name="item">Item</param>
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
            _buffer[_head] = item;
            ++_version;
            return result;
        }

        /// <summary>
        ///     Enqueue head
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="overwritten">Overwritten</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueHead(in T item, out T overwritten)
        {
            InsertResult result;
            if (_size == _length)
            {
                if (--_tail == -1)
                    _tail = _length - 1;
                overwritten = _buffer[_tail];
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
            _buffer[_head] = item;
            ++_version;
            return result;
        }

        /// <summary>
        ///     Try enqueue head
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueHead(in T item)
        {
            if (_size == _length)
                return false;
            if (--_head == -1)
                _head = _length - 1;
            _buffer[_head] = item;
            ++_size;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Enqueue tail
        /// </summary>
        /// <param name="item">Item</param>
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

            _buffer[_tail] = item;
            if (++_tail == _length)
                _tail = 0;
            ++_version;
            return result;
        }

        /// <summary>
        ///     Enqueue tail
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="overwritten">Overwritten</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueTail(in T item, out T overwritten)
        {
            InsertResult result;
            if (_size == _length)
            {
                overwritten = _buffer[_head];
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

            _buffer[_tail] = item;
            if (++_tail == _length)
                _tail = 0;
            ++_version;
            return result;
        }

        /// <summary>
        ///     Try enqueue tail
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueTail(in T item)
        {
            if (_size == _length)
                return false;
            _buffer[_tail] = item;
            if (++_tail == _length)
                _tail = 0;
            ++_size;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueHead(out T result)
        {
            if (_size == 0)
            {
                result = default;
                return false;
            }

            result = _buffer[_head];
            if (++_head == _length)
                _head = 0;
            --_size;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
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
            result = _buffer[_tail];
            --_size;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Try peek head
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekHead(out T result)
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
        ///     Try peek tail
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekTail(out T result)
        {
            var size = _size - 1;
            if ((uint)size >= (uint)_length)
            {
                result = default;
                return false;
            }

            result = _buffer[size];
            return true;
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
        public static UnsafeRingBuffer<T> Empty => new();

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
            ///     NativeDeque
            /// </summary>
            private readonly UnsafeRingBuffer<T>* _nativeDeque;

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
            /// <param name="nativeDeque">NativeDeque</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeDeque)
            {
                var handle = (UnsafeRingBuffer<T>*)nativeDeque;
                _nativeDeque = handle;
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
                var handle = _nativeDeque;
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