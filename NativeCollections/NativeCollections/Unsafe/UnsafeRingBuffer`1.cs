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
        public readonly bool IsEmpty => _size == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public readonly int Count => _size;

        /// <summary>
        ///     Capacity
        /// </summary>
        public readonly int Capacity => _length;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public readonly ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)((_head + index) % _length));
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public readonly ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)((_head + index) % _length));
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeRingBuffer(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            if (capacity < 4)
                capacity = 4;
            _buffer = NativeMemoryAllocator.AlignedAlloc<T>((uint)capacity);
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
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

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
            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head) = item;
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
            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head) = item;
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

            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_tail) = item;
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
        ///     Try enqueue tail
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Enqueued</returns>
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

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head);
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
            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_tail);
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
        ///     Try peek tail
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
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
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<T> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, nameof(count));
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var size = Math.Min(buffer.Length, Math.Min(count, _size));
            if (size == 0)
                return 0;
            var length1 = _length - _head;
            var length2 = length1 > size ? size : length1;
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head)), (uint)(length2 * sizeof(T)));
            var length3 = size - length2;
            if (length3 > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, length1)), ref Unsafe.AsRef<byte>(_buffer), (uint)(length2 * sizeof(T)));
            return size;
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer), count);

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, nameof(buffer));
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var size = _size;
            if (size == 0)
                return;
            var length1 = _length - _head;
            var length2 = length1 > size ? size : length1;
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head)), (uint)(length2 * sizeof(T)));
            var length3 = size - length2;
            if (length3 <= 0)
                return;
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, length1)), ref Unsafe.AsRef<byte>(_buffer), (uint)(length2 * sizeof(T)));
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer));

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
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

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
            ///     Current
            /// </summary>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _currentElement;
            }
        }
    }
}