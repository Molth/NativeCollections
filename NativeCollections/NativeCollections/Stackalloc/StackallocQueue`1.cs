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
    ///     Stackalloc queue
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocQueue<T> : IReadOnlyCollection<T> where T : unmanaged
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
        ///     Get byte count
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Byte count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity) => capacity * sizeof(T) + (int)NativeMemoryAllocator.AlignOf<T>() - 1;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocQueue(Span<byte> buffer, int capacity)
        {
            _buffer = NativeArray<T>.Create(buffer).Buffer;
            _length = capacity;
            _head = 0;
            _tail = 0;
            _size = 0;
            _version = 0;
        }

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
        ///     Try enqueue
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(in T item)
        {
            if (_size != _length)
            {
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_tail) = item;
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
            ThrowHelpers.ThrowIfEmptyQueue(_size);
            var removed = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head);
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

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head);
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
        public readonly T Peek()
        {
            ThrowHelpers.ThrowIfEmptyQueue(_size);
            return Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_head);
        }

        /// <summary>
        ///     Try peek
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryPeek(out T result)
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
        ///     Move next
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void MoveNext(ref int index)
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
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)length1)), ref Unsafe.AsRef<byte>(_buffer), (uint)(length2 * sizeof(T)));
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
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)length1)), ref Unsafe.AsRef<byte>(_buffer), (uint)(length2 * sizeof(T)));
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
        public static StackallocQueue<T> Empty => new();

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
            ///     NativeQueue
            /// </summary>
            private readonly StackallocQueue<T>* _nativeQueue;

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
                var handle = (StackallocQueue<T>*)nativeQueue;
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