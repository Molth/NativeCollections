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
    ///     Represents a double-ended collection of objects that supports insertion and removal from both ends.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeChunkedDeque<T> : IIsCreated, IDisposable, IEquatable<UnsafeChunkedDeque<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Pointer to the first chunk.
        /// </summary>
        private MemoryChunk* _head;

        /// <summary>
        ///     Pointer to the last chunk.
        /// </summary>
        private MemoryChunk* _tail;

        /// <summary>
        ///     Head of the free list of chunks available for reuse.
        /// </summary>
        private MemoryChunk* _freeList;

        /// <summary>
        ///     Gets the total number of chunks currently allocated in the stack.
        /// </summary>
        private int _chunks;

        /// <summary>
        ///     Gets the number of chunks that are currently free and available for reuse.
        /// </summary>
        private int _freeChunks;

        /// <summary>
        ///     Gets the maximum number of free chunks that can be retained before excess chunks are freed.
        /// </summary>
        private readonly int _maxFreeChunks;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private readonly int _size;

        /// <summary>
        ///     Current read position offset within the head chunk.
        /// </summary>
        private int _readOffset;

        /// <summary>
        ///     Current write position offset within the tail chunk.
        /// </summary>
        private int _writeOffset;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep enumerator in sync w/ collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_head);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Gets the total number of chunks currently allocated in the stack.
        /// </summary>
        public readonly int Chunks => _chunks;

        /// <summary>
        ///     Gets the number of chunks that are currently free and available for reuse.
        /// </summary>
        public readonly int FreeChunks => _freeChunks;

        /// <summary>
        ///     Gets the maximum number of free chunks that can be retained before excess chunks are freed.
        /// </summary>
        public readonly int MaxFreeChunks => _maxFreeChunks;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        public readonly int Size => _size;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified chunk size and maximum number of free chunks to retain.
        /// </summary>
        /// <param name="size">
        ///     The number of elements each chunk can hold.
        ///     Must be greater than zero.
        /// </param>
        /// <param name="maxFreeChunks">
        ///     The maximum number of free chunks to keep in the free list.
        ///     Must be non-negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="size" /> is less than or equal to zero, or if
        ///     <paramref name="maxFreeChunks" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeChunkedDeque(int size, int maxFreeChunks)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(size, ExceptionArgument.size);
            ThrowHelpers.ThrowIfNegative(maxFreeChunks, ExceptionArgument.maxFreeChunks);
            var chunk = Create(size);
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
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeChunkedDeque<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeChunkedDeque<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeChunkedDeque<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeChunkedDeque<T> left, UnsafeChunkedDeque<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeChunkedDeque<T> left, UnsafeChunkedDeque<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
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
        ///     Clears the contents of this.
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
        ///     Adds item to the head of the queue.
        /// </summary>
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
                        chunk = Create(_size);
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
        ///     Adds item to the tail of the queue.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnqueueTail(in T item)
        {
            if (_writeOffset == _size)
            {
                _writeOffset = 0;
                MemoryChunk* chunk;
                if (_freeChunks == 0)
                {
                    chunk = Create(_size);
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
            capacity = Math.Min(capacity, _maxFreeChunks);
            while (_freeChunks < capacity)
            {
                _freeChunks++;
                var chunk = Create(_size);
                chunk->Next = _freeList;
                _freeList = chunk;
            }

            return _freeChunks;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
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
            return 0;
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
            var result = count = Math.Min(buffer.Length, Math.Min(count, _count));
            if (count == 0)
                return 0;
            var node = _head;
            var elementCount = Math.Min(_size - _readOffset, count);
            if (elementCount > 0)
            {
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(node->Buffer), (nint)_readOffset)), (uint)(elementCount * Unsafe.SizeOf<T>()));
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
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.AsRef<byte>(node->Buffer), (uint)(elementCount * Unsafe.SizeOf<T>()));
                reference = ref Unsafe.Add(ref reference, (nint)elementCount);
            }

            if (remaining > 0)
            {
                elementCount = remaining;
                node = node->Next;
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.AsRef<byte>(node->Buffer), (uint)(elementCount * Unsafe.SizeOf<T>()));
            }

            return result;
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
            var count = _count;
            if (count == 0)
                return;
            var node = _head;
            var elementCount = Math.Min(_size - _readOffset, count);
            if (elementCount > 0)
            {
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(node->Buffer), (nint)_readOffset)), (uint)(elementCount * Unsafe.SizeOf<T>()));
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
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.AsRef<byte>(node->Buffer), (uint)(elementCount * Unsafe.SizeOf<T>()));
                reference = ref Unsafe.Add(ref reference, (nint)elementCount);
            }

            if (remaining > 0)
            {
                elementCount = remaining;
                node = node->Next;
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.AsRef<byte>(node->Buffer), (uint)(elementCount * Unsafe.SizeOf<T>()));
            }
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
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MemoryChunk* Create(int size)
        {
            var alignment = Math.Max(NativeMemoryAllocator.AlignOf<MemoryChunk>(), NativeMemoryAllocator.AlignOf<T>());
            var chunkByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<MemoryChunk>(), alignment);
            var chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc(chunkByteCount + (uint)size * (uint)Unsafe.SizeOf<T>(), alignment);
            chunk->Buffer = UnsafeHelpers.AddByteOffset<T>(chunk, (nint)chunkByteCount);
            return chunk;
        }

        /// <summary>
        ///     Chunk
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryChunk
        {
            /// <summary>
            ///     The next chunk in the linked list.
            /// </summary>
            public MemoryChunk* Next;

            /// <summary>
            ///     The previous chunk in the linked list.
            /// </summary>
            public MemoryChunk* Previous;

            /// <summary>
            ///     Represents a contiguous region of arbitrary memory.
            /// </summary>
            public T* Buffer;
        }

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeChunkedDeque<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public Enumerator GetEnumerator() => new(UnsafeHelpers.AsPointer(ref this));

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
            private readonly UnsafeChunkedDeque<T>* _handle;

            /// <summary>
            ///     Used to keep enumerator in sync w/ collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     The chunk currently being enumerated.
            /// </summary>
            private MemoryChunk* _currentChunk;

            /// <summary>
            ///     Current read position offset within the head chunk.
            /// </summary>
            private int _readOffset;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            private int _count;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private T _current;

            /// <summary>
            ///     Initializes a new instance of this class.
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
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
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
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}