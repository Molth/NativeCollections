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
    ///     Unsafe chunked deque
    ///     (Slower than Deque)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeChunkedDeque<T> : IIsCreated, IDisposable, IEquatable<UnsafeChunkedDeque<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Head
        /// </summary>
        private MemoryChunk* _head;

        /// <summary>
        ///     Tail
        /// </summary>
        private MemoryChunk* _tail;

        /// <summary>
        ///     Free list
        /// </summary>
        private MemoryChunk* _freeList;

        /// <summary>
        ///     Chunks
        /// </summary>
        private int _chunks;

        /// <summary>
        ///     Free chunks
        /// </summary>
        private int _freeChunks;

        /// <summary>
        ///     Max free chunks
        /// </summary>
        private readonly int _maxFreeChunks;

        /// <summary>
        ///     Size
        /// </summary>
        private readonly int _size;

        /// <summary>
        ///     Read offset
        /// </summary>
        private int _readOffset;

        /// <summary>
        ///     Write offset
        /// </summary>
        private int _writeOffset;

        /// <summary>
        ///     Count
        /// </summary>
        private int _count;

        /// <summary>
        ///     Version
        /// </summary>
        private int _version;

        /// <summary>
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_head);

        /// <summary>
        ///     Is empty
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Chunks
        /// </summary>
        public readonly int Chunks => _chunks;

        /// <summary>
        ///     Free chunks
        /// </summary>
        public readonly int FreeChunks => _freeChunks;

        /// <summary>
        ///     Max free chunks
        /// </summary>
        public readonly int MaxFreeChunks => _maxFreeChunks;

        /// <summary>
        ///     Size
        /// </summary>
        public readonly int Size => _size;

        /// <summary>
        ///     Count
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeChunks">Max free chunks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeChunkedDeque(int size, int maxFreeChunks)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(size, ExceptionArgument.size);
            ThrowHelpers.ThrowIfNegative(maxFreeChunks, ExceptionArgument.maxFreeChunks);
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<MemoryChunk>(), NativeMemoryAllocator.AlignOf<T>());
            var chunkByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<MemoryChunk>(), alignment);
            var chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc((uint)(chunkByteCount + size * Unsafe.SizeOf<T>()), alignment);
            chunk->Buffer = UnsafeHelpers.AddByteOffset<T>(chunk, (nint)chunkByteCount);
            _head = chunk;
            _tail = chunk;
            _freeList = null;
            _chunks = 1;
            _freeChunks = 0;
            _maxFreeChunks = maxFreeChunks;
            _size = size;
            _readOffset = 0;
            _writeOffset = 0;
            _count = 0;
            _version = 0;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeChunkedDeque<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeChunkedDeque<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => SR.Format("UnsafeChunkedDeque<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeChunkedDeque<T> left, UnsafeChunkedDeque<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeChunkedDeque<T> left, UnsafeChunkedDeque<T> right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var node = _head;
            while (_chunks > 0)
            {
                _chunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            node = _freeList;
            while (_freeChunks > 0)
            {
                _freeChunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_chunks != 1)
            {
                _freeChunks += _chunks - 1;
                _chunks = 1;
                var chunk = _head->Next;
                chunk->Next = _freeList;
                _freeList = chunk;
                TrimExcess(_maxFreeChunks);
                _tail = _head;
            }

            _readOffset = 0;
            _writeOffset = 0;
            _count = 0;
            ++_version;
        }

        /// <summary>
        ///     Enqueue head
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnqueueHead(in T item)
        {
            if (_readOffset == 0)
            {
                _readOffset = _size;
                if (_count != 0)
                {
                    MemoryChunk* chunk;
                    if (_freeChunks == 0)
                    {
                        var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<MemoryChunk>(), NativeMemoryAllocator.AlignOf<T>());
                        var chunkByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<MemoryChunk>(), alignment);
                        chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc((uint)(chunkByteCount + _size * Unsafe.SizeOf<T>()), alignment);
                        chunk->Buffer = UnsafeHelpers.AddByteOffset<T>(chunk, (nint)chunkByteCount);
                    }
                    else
                    {
                        chunk = _freeList;
                        _freeList = chunk->Next;
                        --_freeChunks;
                    }

                    chunk->Next = _head;
                    _head->Previous = chunk;
                    _head = chunk;
                    ++_chunks;
                }
                else
                {
                    _writeOffset = _size;
                }
            }

            ++_count;
            Unsafe.Add(ref Unsafe.AsRef<T>(_head->Buffer), (nint)(--_readOffset)) = item;
            ++_version;
        }

        /// <summary>
        ///     Enqueue tail
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnqueueTail(in T item)
        {
            if (_writeOffset == _size)
            {
                _writeOffset = 0;
                MemoryChunk* chunk;
                if (_freeChunks == 0)
                {
                    var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<MemoryChunk>(), NativeMemoryAllocator.AlignOf<T>());
                    var chunkByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<MemoryChunk>(), alignment);
                    chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc((uint)(chunkByteCount + _size * Unsafe.SizeOf<T>()), alignment);
                    chunk->Buffer = UnsafeHelpers.AddByteOffset<T>(chunk, (nint)chunkByteCount);
                }
                else
                {
                    chunk = _freeList;
                    _freeList = chunk->Next;
                    --_freeChunks;
                }

                chunk->Previous = _tail;
                _tail->Next = chunk;
                _tail = chunk;
                ++_chunks;
            }

            ++_count;
            Unsafe.Add(ref Unsafe.AsRef<T>(_tail->Buffer), (nint)_writeOffset++) = item;
            ++_version;
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueHead(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }

            --_count;
            result = Unsafe.Add(ref Unsafe.AsRef<T>(_head->Buffer), (nint)_readOffset++);
            if (_readOffset == _size)
            {
                _readOffset = 0;
                if (_chunks != 1)
                {
                    var chunk = _head;
                    _head = chunk->Next;
                    if (_freeChunks == _maxFreeChunks)
                    {
                        NativeMemoryAllocator.AlignedFree(chunk);
                    }
                    else
                    {
                        chunk->Next = _freeList;
                        _freeList = chunk;
                        ++_freeChunks;
                    }

                    --_chunks;
                }
                else
                {
                    _writeOffset = 0;
                }
            }

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
            if (_count == 0)
            {
                result = default;
                return false;
            }

            --_count;
            result = Unsafe.Add(ref Unsafe.AsRef<T>(_tail->Buffer), (nint)(--_writeOffset));
            if (_writeOffset == 0 && _chunks != 1)
            {
                _writeOffset = _size;
                var chunk = _tail;
                _tail = chunk->Previous;
                if (_freeChunks == _maxFreeChunks)
                {
                    NativeMemoryAllocator.AlignedFree(chunk);
                }
                else
                {
                    chunk->Next = _freeList;
                    _freeList = chunk;
                    ++_freeChunks;
                }

                --_chunks;
            }

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
            if (_count == 0)
            {
                result = default;
                return false;
            }

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_head->Buffer), (nint)_readOffset);
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
            if (_count == 0)
            {
                result = default;
                return false;
            }

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_tail->Buffer), (nint)(_writeOffset - 1));
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
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Min(capacity, _maxFreeChunks);
            while (_freeChunks < capacity)
            {
                _freeChunks++;
                var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<MemoryChunk>(), NativeMemoryAllocator.AlignOf<T>());
                var chunkByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<MemoryChunk>(), alignment);
                var chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc((uint)(chunkByteCount + _size * Unsafe.SizeOf<T>()), alignment);
                chunk->Buffer = UnsafeHelpers.AddByteOffset<T>(chunk, (nint)chunkByteCount);
                chunk->Next = _freeList;
                _freeList = chunk;
            }

            return _freeChunks;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            var node = _freeList;
            while (_freeChunks > 0)
            {
                _freeChunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            _freeList = node;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            var node = _freeList;
            while (_freeChunks > capacity)
            {
                _freeChunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            _freeList = node;
            return _freeChunks;
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<T> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var result = count = Math.Min(buffer.Length, Math.Min(count, _count));
            if (count == 0)
                return 0;
            var node = _head;
            var elementCount = Math.Min(_size - _readOffset, count);
            if (elementCount > 0)
            {
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(node->Buffer), (nint)_readOffset)), (uint)(elementCount * Unsafe.SizeOf<T>()));
                count -= elementCount;
            }

            if (count == 0)
                return elementCount;
            reference = ref Unsafe.Add(ref reference, (nint)elementCount);
            elementCount = _size;
            var (chunks, remaining) = MathHelpers.DivRem(count, elementCount);
            for (var i = 0; i < chunks; ++i)
            {
                node = node->Next;
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.AsRef<byte>(node->Buffer), (uint)(elementCount * Unsafe.SizeOf<T>()));
                reference = ref Unsafe.Add(ref reference, (nint)elementCount);
            }

            if (remaining > 0)
            {
                elementCount = remaining;
                node = node->Next;
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.AsRef<byte>(node->Buffer), (uint)(elementCount * Unsafe.SizeOf<T>()));
            }

            return result;
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
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var count = _count;
            if (count == 0)
                return;
            var node = _head;
            var elementCount = Math.Min(_size - _readOffset, count);
            if (elementCount > 0)
            {
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(node->Buffer), (nint)_readOffset)), (uint)(elementCount * Unsafe.SizeOf<T>()));
                count -= elementCount;
            }

            if (count == 0)
                return;
            reference = ref Unsafe.Add(ref reference, (nint)elementCount);
            elementCount = _size;
            var (chunks, remaining) = MathHelpers.DivRem(count, elementCount);
            for (var i = 0; i < chunks; ++i)
            {
                node = node->Next;
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.AsRef<byte>(node->Buffer), (uint)(elementCount * Unsafe.SizeOf<T>()));
                reference = ref Unsafe.Add(ref reference, (nint)elementCount);
            }

            if (remaining > 0)
            {
                elementCount = remaining;
                node = node->Next;
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.AsRef<byte>(node->Buffer), (uint)(elementCount * Unsafe.SizeOf<T>()));
            }
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer));

        /// <summary>
        ///     Chunk
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryChunk
        {
            /// <summary>
            ///     Next
            /// </summary>
            public MemoryChunk* Next;

            /// <summary>
            ///     Previous
            /// </summary>
            public MemoryChunk* Previous;

            /// <summary>
            ///     Buffer
            /// </summary>
            public T* Buffer;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeChunkedDeque<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        [MustBePinned(SR.parameter_this)]
        public Enumerator GetEnumerator() => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Get enumerator
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
            ///     Unsafe chunked deque
            /// </summary>
            private readonly UnsafeChunkedDeque<T>* _handle;

            /// <summary>
            ///     Version
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Memory chunk
            /// </summary>
            private MemoryChunk* _currentChunk;

            /// <summary>
            ///     Read offset
            /// </summary>
            private int _readOffset;

            /// <summary>
            ///     Count
            /// </summary>
            private int _count;

            /// <summary>
            ///     Current
            /// </summary>
            private T _current;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UnsafeChunkedDeque<T>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _currentChunk = handle->_head;
                _readOffset = handle->_readOffset;
                _count = handle->_count;
                _current = default;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _handle;
                ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                if (_count == 0)
                    return false;
                --_count;
                _current = Unsafe.Add(ref Unsafe.AsRef<T>(_currentChunk->Buffer), (nint)_readOffset++);
                if (_readOffset == handle->_size && _count > 0)
                {
                    _readOffset = 0;
                    _currentChunk = _currentChunk->Next;
                }

                return true;
            }

            /// <summary>
            ///     Reset
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                var handle = _handle;
                _currentChunk = handle->_head;
                _readOffset = handle->_readOffset;
                _count = handle->_count;
                _current = default;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}