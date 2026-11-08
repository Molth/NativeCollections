using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a stream that stores its data in a chain of fixed-size unmanaged memory chunks.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeChunkedStream : IIsCreated, IDisposable, IEnumerable<NativeArray<byte>>, IEquatable<UnsafeChunkedStream>
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
        ///     Gets the total number of chunks currently allocated in the stream.
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
        ///     Gets the number of bytes each chunk can hold.
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
        ///     Gets the length in bytes of the stream.
        /// </summary>
        private int _length;

        /// <summary>
        ///     Used to keep the enumerator synchronized with the collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="head">Pointer to the first chunk.</param>
        /// <param name="tail">Pointer to the last chunk.</param>
        /// <param name="freeList">Head of the free list of chunks available for reuse.</param>
        /// <param name="chunks">The total number of chunks currently allocated in the stream.</param>
        /// <param name="freeChunks">The number of chunks that are currently free and available for reuse.</param>
        /// <param name="maxFreeChunks">
        ///     The maximum number of free chunks that can be retained before excess chunks are freed.
        /// </param>
        /// <param name="size">The number of bytes each chunk can hold.</param>
        /// <param name="readOffset">Current read position offset within the head chunk.</param>
        /// <param name="writeOffset">Current write position offset within the tail chunk.</param>
        /// <param name="length">The length in bytes of the stream.</param>
        /// <param name="version">The version used to keep the enumerator synchronized with the collection.</param>
        private UnsafeChunkedStream(MemoryChunk* head, MemoryChunk* tail, MemoryChunk* freeList, int chunks, int freeChunks, int maxFreeChunks, int size, int readOffset, int writeOffset, int length, int version)
        {
            _head = head;
            _tail = tail;
            _freeList = freeList;
            _chunks = chunks;
            _freeChunks = freeChunks;
            _maxFreeChunks = maxFreeChunks;
            _size = size;
            _readOffset = readOffset;
            _writeOffset = writeOffset;
            _length = length;
            _version = version;
        }

        /// <summary>
        ///     Creates a chunked stream with the specified chunk size and free-list limit.
        /// </summary>
        /// <param name="size">The number of bytes each chunk can hold. Must be greater than zero.</param>
        /// <param name="maxFreeChunks">The maximum number of free chunks to retain. Must be non-negative.</param>
        /// <returns>A new chunked stream.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="size" /> is not positive or <paramref name="maxFreeChunks" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeChunkedStream Create(int size, int maxFreeChunks)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(size, ExceptionArgument.size);
            ThrowHelpers.ThrowIfNegative(maxFreeChunks, ExceptionArgument.maxFreeChunks);
            var chunk = CreateChunk(size);
            return new UnsafeChunkedStream(chunk, chunk, null, 1, 0, maxFreeChunks, size, 0, 0, 0, 0);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _head != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _length == 0;

        /// <summary>
        ///     Gets the total number of chunks currently allocated in the stream.
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
        ///     Gets the number of bytes each chunk can hold.
        /// </summary>
        public readonly int Size => _size;

        /// <summary>
        ///     Gets the length in bytes of the stream.
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeChunkedStream other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeChunkedStream other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeChunkedStream";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeChunkedStream left, UnsafeChunkedStream right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeChunkedStream left, UnsafeChunkedStream right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var node = _head;
            while (_chunks > 0)
            {
                _chunks = _chunks.WrappingSub(1);
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            node = _freeList;
            while (_freeChunks > 0)
            {
                _freeChunks = _freeChunks.WrappingSub(1);
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream, and advances
        ///     the position within the memory stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">A pointer to the buffer that receives the bytes read from the stream.</param>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            return Read(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(buffer), length));
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">A pointer to the buffer containing the bytes to write to the stream.</param>
        /// <param name="length">The number of bytes to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write([MustNotSelfAlias] byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            Write(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(buffer), length));
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream, and advances
        ///     the position within the memory stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">The buffer that receives the bytes read from the stream.</param>
        /// <returns>The number of bytes read.</returns>
        public int Read(Span<byte> buffer)
        {
            var length = buffer.Length;
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            if (length >= _length)
            {
                length = _length;
                if (length == 0)
                    return 0;

                _version = _version.WrappingAdd(1);
                var size = _size;
                var byteCount = size.WrappingSub(_readOffset);
                if (byteCount >= length)
                {
                    SpanHelpers.Copy(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), Usize.FromI32Unchecked(length));
                }
                else
                {
                    if (byteCount != 0)
                        SpanHelpers.Copy(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), Usize.FromI32Unchecked(byteCount));

                    MemoryChunk* chunk;
                    var count = length.WrappingSub(byteCount);
                    var (chunks, remaining) = MathHelpers.DivRem(count, size);
                    for (var i = 0; i < chunks; i = i.WrappingAdd(1))
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
                            _freeChunks = _freeChunks.WrappingAdd(1);
                        }

                        _chunks = _chunks.WrappingSub(1);
                        SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref reference, new IntPtr(byteCount)), ref Unsafe.AsRef<byte>(_head->Buffer), Usize.FromI32Unchecked(size));
                        byteCount = byteCount.WrappingAdd(size);
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
                            _freeChunks = _freeChunks.WrappingAdd(1);
                        }

                        _chunks = _chunks.WrappingSub(1);
                        SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref reference, new IntPtr(byteCount)), ref Unsafe.AsRef<byte>(_head->Buffer), Usize.FromI32Unchecked(remaining));
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

                _version = _version.WrappingAdd(1);
                var size = _size;
                var byteCount = size.WrappingSub(_readOffset);
                if (byteCount >= length)
                {
                    SpanHelpers.Copy(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), Usize.FromI32Unchecked(length));
                    _readOffset = _readOffset.WrappingAdd(length);
                }
                else
                {
                    if (byteCount != 0)
                        SpanHelpers.Copy(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), Usize.FromI32Unchecked(byteCount));

                    MemoryChunk* chunk;
                    var count = length.WrappingSub(byteCount);
                    var (chunks, remaining) = MathHelpers.DivRem(count, size);
                    for (var i = 0; i < chunks; i = i.WrappingAdd(1))
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
                            _freeChunks = _freeChunks.WrappingAdd(1);
                        }

                        _chunks = _chunks.WrappingSub(1);
                        SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref reference, new IntPtr(byteCount)), ref Unsafe.AsRef<byte>(_head->Buffer), Usize.FromI32Unchecked(size));
                        byteCount = byteCount.WrappingAdd(size);
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
                            _freeChunks = _freeChunks.WrappingAdd(1);
                        }

                        _chunks = _chunks.WrappingSub(1);
                        SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref reference, new IntPtr(byteCount)), ref Unsafe.AsRef<byte>(_head->Buffer), Usize.FromI32Unchecked(remaining));
                    }

                    _readOffset = remaining == 0 ? _size : remaining;
                }

                _length = _length.WrappingSub(length);
            }

            return length;
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">The buffer containing the bytes to write to the stream.</param>
        /// <exception cref="IOException">
        ///     Thrown when the resulting stream length exceeds <see cref="int.MaxValue" /> bytes.
        /// </exception>
        public void Write([MustNotSelfAlias] ReadOnlySpan<byte> buffer)
        {
            var length = buffer.Length;
            if (length == 0)
                return;

            if (U32.FromI32Unchecked(length) > U32.FromI32Unchecked(int.MaxValue.WrappingSub(_length)))
                ThrowHelpers.ThrowStreamTooLongException();

            _version = _version.WrappingAdd(1);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var size = _size;
            var byteCount = size.WrappingSub(_writeOffset);
            if (byteCount >= length)
            {
                SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_tail->Buffer), new IntPtr(_writeOffset)), ref reference, Usize.FromI32Unchecked(length));
                _writeOffset = _writeOffset.WrappingAdd(length);
            }
            else
            {
                if (byteCount != 0)
                    SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_tail->Buffer), new IntPtr(_writeOffset)), ref reference, Usize.FromI32Unchecked(byteCount));

                MemoryChunk* chunk;
                var count = length.WrappingSub(byteCount);
                var (chunks, remaining) = MathHelpers.DivRem(count, size);
                for (var i = 0; i < chunks; i = i.WrappingAdd(1))
                {
                    if (_freeChunks == 0)
                    {
                        chunk = CreateChunk(_size);
                    }
                    else
                    {
                        chunk = _freeList;
                        _freeList = chunk->Next;
                        _freeChunks = _freeChunks.WrappingSub(1);
                    }

                    _tail->Next = chunk;
                    _tail = chunk;
                    _chunks = _chunks.WrappingAdd(1);
                    SpanHelpers.Copy(ref Unsafe.AsRef<byte>(_tail->Buffer), ref Unsafe.AddByteOffset(ref reference, new IntPtr(byteCount)), Usize.FromI32Unchecked(size));
                    byteCount = byteCount.WrappingAdd(size);
                }

                if (remaining != 0)
                {
                    if (_freeChunks == 0)
                    {
                        chunk = CreateChunk(_size);
                    }
                    else
                    {
                        chunk = _freeList;
                        _freeList = chunk->Next;
                        _freeChunks = _freeChunks.WrappingSub(1);
                    }

                    _tail->Next = chunk;
                    _tail = chunk;
                    _chunks = _chunks.WrappingAdd(1);
                    SpanHelpers.Copy(ref Unsafe.AsRef<byte>(_tail->Buffer), ref Unsafe.AddByteOffset(ref reference, new IntPtr(byteCount)), Usize.FromI32Unchecked(remaining));
                }

                _writeOffset = remaining == 0 ? _size : remaining;
            }

            _length = _length.WrappingAdd(length);
        }

        /// <summary>
        ///     Advances the position within the memory stream by the number of bytes read.
        /// </summary>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        public int Read(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            if (length >= _length)
            {
                length = _length;
                if (length == 0)
                    return 0;

                _version = _version.WrappingAdd(1);
                var size = _size;
                var byteCount = size.WrappingSub(_readOffset);
                if (byteCount < length)
                {
                    MemoryChunk* chunk;
                    var count = length.WrappingSub(byteCount);
                    var (chunks, remaining) = MathHelpers.DivRem(count, size);
                    for (var i = 0; i < chunks; i = i.WrappingAdd(1))
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
                            _freeChunks = _freeChunks.WrappingAdd(1);
                        }

                        _chunks = _chunks.WrappingSub(1);
                        byteCount = byteCount.WrappingAdd(size);
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
                            _freeChunks = _freeChunks.WrappingAdd(1);
                        }

                        _chunks = _chunks.WrappingSub(1);
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

                _version = _version.WrappingAdd(1);
                var size = _size;
                var byteCount = size.WrappingSub(_readOffset);
                if (byteCount >= length)
                {
                    _readOffset = _readOffset.WrappingAdd(length);
                }
                else
                {
                    MemoryChunk* chunk;
                    var count = length.WrappingSub(byteCount);
                    var (chunks, remaining) = MathHelpers.DivRem(count, size);
                    for (var i = 0; i < chunks; i = i.WrappingAdd(1))
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
                            _freeChunks = _freeChunks.WrappingAdd(1);
                        }

                        _chunks = _chunks.WrappingSub(1);
                        byteCount = byteCount.WrappingAdd(size);
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
                            _freeChunks = _freeChunks.WrappingAdd(1);
                        }

                        _chunks = _chunks.WrappingSub(1);
                    }

                    _readOffset = remaining == 0 ? _size : remaining;
                }

                _length = _length.WrappingSub(length);
            }

            return length;
        }

        /// <summary>
        ///     Advances the current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="length">The number of bytes by which to advance the position.</param>
        /// <exception cref="IOException">
        ///     Thrown when the resulting stream length exceeds <see cref="int.MaxValue" /> bytes.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        /// <remarks>
        ///     This method advances the current position without writing any data. The newly exposed
        ///     range is left untouched, so it may contain data previously written to recycled chunks
        ///     instead of zero bytes.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            if (length == 0)
                return;

            if (U32.FromI32Unchecked(length) > U32.FromI32Unchecked(int.MaxValue.WrappingSub(_length)))
                ThrowHelpers.ThrowStreamTooLongException();

            _version = _version.WrappingAdd(1);
            var size = _size;
            var byteCount = size.WrappingSub(_writeOffset);
            if (byteCount >= length)
            {
                _writeOffset = _writeOffset.WrappingAdd(length);
            }
            else
            {
                MemoryChunk* chunk;
                var count = length.WrappingSub(byteCount);
                var (chunks, remaining) = MathHelpers.DivRem(count, size);
                for (var i = 0; i < chunks; i = i.WrappingAdd(1))
                {
                    if (_freeChunks == 0)
                    {
                        chunk = CreateChunk(_size);
                    }
                    else
                    {
                        chunk = _freeList;
                        _freeList = chunk->Next;
                        _freeChunks = _freeChunks.WrappingSub(1);
                    }

                    _tail->Next = chunk;
                    _tail = chunk;
                    _chunks = _chunks.WrappingAdd(1);
                    byteCount = byteCount.WrappingAdd(size);
                }

                if (remaining != 0)
                {
                    if (_freeChunks == 0)
                    {
                        chunk = CreateChunk(_size);
                    }
                    else
                    {
                        chunk = _freeList;
                        _freeList = chunk->Next;
                        _freeChunks = _freeChunks.WrappingSub(1);
                    }

                    _tail->Next = chunk;
                    _tail = chunk;
                    _chunks = _chunks.WrappingAdd(1);
                }

                _writeOffset = remaining == 0 ? _size : remaining;
            }

            _length = _length.WrappingAdd(length);
        }

        /// <summary>
        ///     Returns a span over the unread bytes of the first chunk of this.
        /// </summary>
        /// <returns>A span over the unread bytes of the first chunk of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> GetBuffer()
        {
            var byteCount = Math.Min(_size.WrappingSub(_readOffset), _length);
            return MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), byteCount);
        }

        /// <summary>
        ///     Ensures that this retains at least the specified number of free chunks, up to the configured maximum.
        /// </summary>
        /// <param name="capacity">The minimum number of free chunks to retain.</param>
        /// <returns>The number of free chunks retained by this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Min(capacity, _maxFreeChunks);
            while (_freeChunks < capacity)
            {
                _freeChunks = _freeChunks.WrappingAdd(1);
                var chunk = CreateChunk(_size);
                chunk->Next = _freeList;
                _freeList = chunk;
            }

            return _freeChunks;
        }

        /// <summary>
        ///     Releases all free chunks from this.
        /// </summary>
        /// <returns>The number of free chunks retained.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var node = _freeList;
            while (_freeChunks > 0)
            {
                _freeChunks = _freeChunks.WrappingSub(1);
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            _freeList = node;
            return 0;
        }

        /// <summary>
        ///     Trims the retained free-chunk capacity of this to the specified number of chunks.
        /// </summary>
        /// <param name="capacity">The number of free chunks to retain.</param>
        /// <returns>The number of free chunks retained.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            var node = _freeList;
            while (_freeChunks > capacity)
            {
                _freeChunks = _freeChunks.WrappingSub(1);
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            _freeList = node;
            return _freeChunks;
        }

        /// <summary>
        ///     Copies up to the specified number of bytes from this. The actual number of copied bytes
        ///     is limited by the span's length, the specified count, and the current length of this.
        /// </summary>
        /// <param name="buffer">The destination span to which bytes are copied.</param>
        /// <param name="count">The maximum number of bytes to copy. Must be non-negative.</param>
        /// <returns>The actual number of bytes copied from this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        public readonly int CopyTo(Span<byte> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var result = count = Math.Min(buffer.Length, Math.Min(count, _length));
            if (count == 0)
                return 0;

            var node = _head;
            var elementCount = Math.Min(_size.WrappingSub(_readOffset), count);
            if (elementCount > 0)
            {
                SpanHelpers.Copy(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(node->Buffer), new IntPtr(_readOffset)), Usize.FromI32Unchecked(elementCount));
                count = count.WrappingSub(elementCount);
            }

            if (count == 0)
                return elementCount;

            reference = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(elementCount));
            elementCount = _size;
            var (chunks, remaining) = MathHelpers.DivRem(count, elementCount);
            for (var i = 0; i < chunks; i = i.WrappingAdd(1))
            {
                node = node->Next;
                SpanHelpers.Copy(ref reference, ref Unsafe.AsRef<byte>(node->Buffer), Usize.FromI32Unchecked(elementCount));
                reference = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(elementCount));
            }

            if (remaining > 0)
            {
                elementCount = remaining;
                node = node->Next;
                SpanHelpers.Copy(ref reference, ref Unsafe.AsRef<byte>(node->Buffer), Usize.FromI32Unchecked(elementCount));
            }

            return result;
        }

        /// <summary>
        ///     Copies all bytes from this into a destination span. The span
        ///     must have a length at least equal to the current length of this.
        /// </summary>
        /// <param name="buffer">The destination span to which all bytes are copied.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the bytes of this.
        /// </exception>
        public readonly void CopyTo(Span<byte> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, _length, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var count = _length;
            if (count == 0)
                return;

            var node = _head;
            var elementCount = Math.Min(_size.WrappingSub(_readOffset), count);
            if (elementCount > 0)
            {
                SpanHelpers.Copy(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(node->Buffer), new IntPtr(_readOffset)), Usize.FromI32Unchecked(elementCount));
                count = count.WrappingSub(elementCount);
            }

            if (count == 0)
                return;

            reference = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(elementCount));
            elementCount = _size;
            var (chunks, remaining) = MathHelpers.DivRem(count, elementCount);
            for (var i = 0; i < chunks; i = i.WrappingAdd(1))
            {
                node = node->Next;
                SpanHelpers.Copy(ref reference, ref Unsafe.AsRef<byte>(node->Buffer), Usize.FromI32Unchecked(elementCount));
                reference = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(elementCount));
            }

            if (remaining > 0)
            {
                elementCount = remaining;
                node = node->Next;
                SpanHelpers.Copy(ref reference, ref Unsafe.AsRef<byte>(node->Buffer), Usize.FromI32Unchecked(elementCount));
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="size">The number of bytes to store in the chunk.</param>
        /// <returns>
        ///     A pointer to the newly allocated chunk whose data region holds <paramref name="size" /> bytes.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MemoryChunk* CreateChunk(int size) => (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc(Usize.FromI32Unchecked(Unsafe.SizeOf<MemoryChunk>()).WrappingAdd(Usize.FromI32Unchecked(size)), NativeMemoryAllocator.AlignOf<MemoryChunk>());

        /// <summary>
        ///     Chunk.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryChunk
        {
            /// <summary>
            ///     The next chunk in the linked list.
            /// </summary>
            public MemoryChunk* Next;

            /// <summary>
            ///     Represents a contiguous region of arbitrary memory.
            /// </summary>
            public fixed byte Buffer[1];
        }

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
        readonly IEnumerator<NativeArray<byte>> IEnumerable<NativeArray<byte>>.GetEnumerator()
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
        /// <remarks>
        ///     The enumerator can yield segments whose length is 0, for example when the
        ///     first chunk of the stream has no bytes written yet.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IIterator<NativeArray<byte>>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly UnsafeChunkedStream* _handle;

            /// <summary>
            ///     Used to keep the enumerator synchronized with the collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     The chunk currently being enumerated.
            /// </summary>
            private MemoryChunk* _currentChunk;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private NativeArray<byte> _current;

            /// <summary>
            ///     Indicates whether the enumerator has begun iterating.
            /// </summary>
            private bool _started;

            /// <summary>
            ///     Indicates whether the enumerator has reached the end of the stream.
            /// </summary>
            private bool _ended;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(UnsafeChunkedStream* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _currentChunk = handle->_head;
                _current = default;
                _started = false;
                _ended = false;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(UnsafeChunkedStream* handle) => new(handle);

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
                if (handle->_length == 0)
                    return false;

                if (!_started)
                {
                    _started = true;
                    _current = NativeArray<byte>.FromBytes(handle->GetBuffer());
                    if (_currentChunk != handle->_tail)
                        _currentChunk = _currentChunk->Next;
                    else
                        _ended = true;

                    return true;
                }

                if (_currentChunk != handle->_tail)
                {
                    _current = NativeArray<byte>.FromBytes(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(_currentChunk->Buffer), handle->_size));
                    _currentChunk = _currentChunk->Next;
                    return true;
                }

                if (!_ended)
                {
                    _ended = true;
                    _current = NativeArray<byte>.FromBytes(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(_currentChunk->Buffer), handle->_writeOffset));
                    return true;
                }

                return false;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
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
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly NativeArray<byte> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}