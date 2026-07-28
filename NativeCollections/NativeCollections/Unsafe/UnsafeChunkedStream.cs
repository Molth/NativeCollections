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
    ///     Unsafe chunked stream
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeChunkedStream : IIsCreated, IDisposable, IEnumerable<NativeArray<byte>>, IEquatable<UnsafeChunkedStream>
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
        ///     Read offset
        /// </summary>
        private int _readOffset;

        /// <summary>
        ///     Write offset
        /// </summary>
        private int _writeOffset;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        private int _length;

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
        /// <value>
        ///     true if this is empty;
        ///     otherwise, false.
        /// </value>
        public readonly bool IsEmpty => _length == 0;

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
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeChunks">Max free chunks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeChunkedStream(int size, int maxFreeChunks)
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
            _length = 0;
            _version = 0;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeChunkedStream other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeChunkedStream other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeChunkedStream";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeChunkedStream left, UnsafeChunkedStream right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeChunkedStream left, UnsafeChunkedStream right) => !left.Equals(right);

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
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.buffer);
            return Read(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(buffer), length));
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream,
        ///     and advances the current position within this memory stream by the number of bytes written.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.buffer);
            Write(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(buffer), length));
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<byte> buffer)
        {
            var length = buffer.Length;
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            if (length >= _length)
            {
                length = _length;
                if (length == 0)
                    return 0;
                ++_version;
                var size = _size;
                var byteCount = size - _readOffset;
                if (byteCount >= length)
                {
                    SpanHelpers.Copy(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), (uint)length);
                }
                else
                {
                    if (byteCount != 0)
                        SpanHelpers.Copy(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), (uint)byteCount);
                    MemoryChunk* chunk;
                    var count = length - byteCount;
                    var (chunks, remaining) = MathHelpers.DivRem(count, size);
                    for (var i = 0; i < chunks; ++i)
                    {
                        chunk = _head;
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
                        SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref reference, new IntPtr(byteCount)), ref Unsafe.AsRef<byte>(_head->Buffer), (uint)size);
                        byteCount += size;
                    }

                    if (remaining != 0)
                    {
                        chunk = _head;
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
                        SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref reference, new IntPtr(byteCount)), ref Unsafe.AsRef<byte>(_head->Buffer), (uint)remaining);
                    }
                }

                _readOffset = 0;
                _writeOffset = 0;
                _length = 0;
            }
            else
            {
                if (length == 0)
                    return 0;
                ++_version;
                var size = _size;
                var byteCount = size - _readOffset;
                if (byteCount >= length)
                {
                    SpanHelpers.Copy(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), (uint)length);
                    _readOffset += length;
                }
                else
                {
                    if (byteCount != 0)
                        SpanHelpers.Copy(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), (uint)byteCount);
                    MemoryChunk* chunk;
                    var count = length - byteCount;
                    var (chunks, remaining) = MathHelpers.DivRem(count, size);
                    for (var i = 0; i < chunks; ++i)
                    {
                        chunk = _head;
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
                        SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref reference, new IntPtr(byteCount)), ref Unsafe.AsRef<byte>(_head->Buffer), (uint)size);
                        byteCount += size;
                    }

                    if (remaining != 0)
                    {
                        chunk = _head;
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
                        SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref reference, new IntPtr(byteCount)), ref Unsafe.AsRef<byte>(_head->Buffer), (uint)remaining);
                    }

                    _readOffset = remaining == 0 ? _size : remaining;
                }

                _length -= length;
            }

            return length;
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream,
        ///     and advances the current position within this memory stream by the number of bytes written.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> buffer)
        {
            var length = buffer.Length;
            if (length == 0)
                return;
            ++_version;
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var size = _size;
            var byteCount = size - _writeOffset;
            if (byteCount >= length)
            {
                SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_tail->Buffer), new IntPtr(_writeOffset)), ref reference, (uint)length);
                _writeOffset += length;
            }
            else
            {
                if (byteCount != 0)
                    SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_tail->Buffer), new IntPtr(_writeOffset)), ref reference, (uint)byteCount);
                MemoryChunk* chunk;
                var count = length - byteCount;
                var (chunks, remaining) = MathHelpers.DivRem(count, size);
                for (var i = 0; i < chunks; ++i)
                {
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

                    _tail->Next = chunk;
                    _tail = chunk;
                    ++_chunks;
                    SpanHelpers.Copy(ref Unsafe.AsRef<byte>(_tail->Buffer), ref Unsafe.AddByteOffset(ref reference, new IntPtr(byteCount)), (uint)size);
                    byteCount += size;
                }

                if (remaining != 0)
                {
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

                    _tail->Next = chunk;
                    _tail = chunk;
                    ++_chunks;
                    SpanHelpers.Copy(ref Unsafe.AsRef<byte>(_tail->Buffer), ref Unsafe.AddByteOffset(ref reference, new IntPtr(byteCount)), (uint)remaining);
                }

                _writeOffset = remaining == 0 ? _size : remaining;
            }

            _length += length;
        }

        /// <summary>
        ///     Advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            if (length >= _length)
            {
                length = _length;
                if (length == 0)
                    return 0;
                ++_version;
                var size = _size;
                var byteCount = size - _readOffset;
                if (byteCount < length)
                {
                    MemoryChunk* chunk;
                    var count = length - byteCount;
                    var (chunks, remaining) = MathHelpers.DivRem(count, size);
                    for (var i = 0; i < chunks; ++i)
                    {
                        chunk = _head;
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
                        byteCount += size;
                    }

                    if (remaining != 0)
                    {
                        chunk = _head;
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
                }

                _readOffset = 0;
                _writeOffset = 0;
                _length = 0;
            }
            else
            {
                if (length == 0)
                    return 0;
                ++_version;
                var size = _size;
                var byteCount = size - _readOffset;
                if (byteCount >= length)
                {
                    _readOffset += length;
                }
                else
                {
                    MemoryChunk* chunk;
                    var count = length - byteCount;
                    var (chunks, remaining) = MathHelpers.DivRem(count, size);
                    for (var i = 0; i < chunks; ++i)
                    {
                        chunk = _head;
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
                        byteCount += size;
                    }

                    if (remaining != 0)
                    {
                        chunk = _head;
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

                    _readOffset = remaining == 0 ? _size : remaining;
                }

                _length -= length;
            }

            return length;
        }

        /// <summary>
        ///     Advances the current position within this memory stream by the number of bytes written.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            if (length == 0)
                return;
            ++_version;
            var size = _size;
            var byteCount = size - _writeOffset;
            if (byteCount >= length)
            {
                _writeOffset += length;
            }
            else
            {
                MemoryChunk* chunk;
                var count = length - byteCount;
                var (chunks, remaining) = MathHelpers.DivRem(count, size);
                for (var i = 0; i < chunks; ++i)
                {
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

                    _tail->Next = chunk;
                    _tail = chunk;
                    ++_chunks;
                    byteCount += size;
                }

                if (remaining != 0)
                {
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

                    _tail->Next = chunk;
                    _tail = chunk;
                    ++_chunks;
                }

                _writeOffset = remaining == 0 ? _size : remaining;
            }

            _length += length;
        }

        /// <summary>
        ///     Returns the first array of unsigned bytes from which this stream was created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> GetBuffer()
        {
            var byteCount = Math.Min(_size - _readOffset, _length);
            return MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), byteCount);
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
        public readonly int CopyTo(Span<byte> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var result = count = Math.Min(buffer.Length, Math.Min(count, _length));
            if (count == 0)
                return 0;
            var node = _head;
            var elementCount = Math.Min(_size - _readOffset, count);
            if (elementCount > 0)
            {
                SpanHelpers.Copy(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(node->Buffer), new IntPtr(_readOffset)), (uint)elementCount);
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
                SpanHelpers.Copy(ref reference, ref Unsafe.AsRef<byte>(node->Buffer), (uint)elementCount);
                reference = ref Unsafe.Add(ref reference, (nint)elementCount);
            }

            if (remaining > 0)
            {
                elementCount = remaining;
                node = node->Next;
                SpanHelpers.Copy(ref reference, ref Unsafe.AsRef<byte>(node->Buffer), (uint)elementCount);
            }

            return result;
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
        public readonly void CopyTo(Span<byte> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, _length, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var count = _length;
            if (count == 0)
                return;
            var node = _head;
            var elementCount = Math.Min(_size - _readOffset, count);
            if (elementCount > 0)
            {
                SpanHelpers.Copy(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(node->Buffer), new IntPtr(_readOffset)), (uint)elementCount);
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
                SpanHelpers.Copy(ref reference, ref Unsafe.AsRef<byte>(node->Buffer), (uint)elementCount);
                reference = ref Unsafe.Add(ref reference, (nint)elementCount);
            }

            if (remaining > 0)
            {
                elementCount = remaining;
                node = node->Next;
                SpanHelpers.Copy(ref reference, ref Unsafe.AsRef<byte>(node->Buffer), (uint)elementCount);
            }
        }

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MemoryChunk* Create(int size) => (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc((uint)(Unsafe.SizeOf<MemoryChunk>() + size), NativeMemoryAllocator.AlignOf<MemoryChunk>());

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
            ///     Buffer
            /// </summary>
            public fixed byte Buffer[1];
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeChunkedStream Empty => default;

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
        readonly IEnumerator<NativeArray<byte>> IEnumerable<NativeArray<byte>>.GetEnumerator()
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
        public struct Enumerator : IIterator<NativeArray<byte>>
        {
            /// <summary>
            ///     Unsafe chunked stream
            /// </summary>
            private readonly UnsafeChunkedStream* _handle;

            /// <summary>
            ///     Used to keep enumerator in sync w/ collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Memory chunk
            /// </summary>
            private MemoryChunk* _currentChunk;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private NativeArray<byte> _current;

            /// <summary>
            ///     Started
            /// </summary>
            private bool _started;

            /// <summary>
            ///     Ended
            /// </summary>
            private bool _ended;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UnsafeChunkedStream* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _currentChunk = handle->_head;
                _current = default;
                _started = false;
                _ended = false;
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
                if (handle->_length == 0)
                    return false;

                if (!_started)
                {
                    _started = true;
                    _current = handle->GetBuffer();
                    if (_currentChunk != handle->_tail)
                        _currentChunk = _currentChunk->Next;
                    else
                        _ended = true;
                    return true;
                }

                if (_currentChunk != handle->_tail)
                {
                    _current = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(_currentChunk->Buffer), handle->_size);
                    _currentChunk = _currentChunk->Next;
                    return true;
                }

                if (!_ended)
                {
                    _ended = true;
                    _current = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(_currentChunk->Buffer), handle->_writeOffset);
                    return true;
                }

                return false;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                var handle = _handle;
                _currentChunk = handle->_head;
                _current = default;
                _started = false;
                _ended = false;
            }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly NativeArray<byte> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}