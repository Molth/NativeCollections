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
    /// <typeparam name="T">The type of elements in the chunked stack.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeChunkedStack<T> : IIsCreated, IDisposable, IEquatable<UnsafeChunkedStack<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Sentinel node of the linked list of active chunks.
        /// </summary>
        private MemoryChunk* _sentinel;

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
        ///     The number of elements each chunk can hold.
        /// </summary>
        private readonly int _size;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        private int _count;

        /// <summary>
        ///     The number of elements from the start.
        /// </summary>
        private int _offset;

        /// <summary>
        ///     Used to keep the enumerator synchronized with the collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="sentinel">Sentinel node of the linked list of active chunks.</param>
        /// <param name="freeList">Head of the free list of chunks available for reuse.</param>
        /// <param name="chunks">The total number of chunks currently allocated in the stack.</param>
        /// <param name="freeChunks">The number of chunks that are currently free and available for reuse.</param>
        /// <param name="maxFreeChunks">
        ///     The maximum number of free chunks that can be retained before excess chunks are freed.
        /// </param>
        /// <param name="size">The number of elements each chunk can hold.</param>
        /// <param name="count">The number of elements contained in this.</param>
        /// <param name="offset">The number of elements from the start.</param>
        /// <param name="version">The version used to keep the enumerator synchronized with the collection.</param>
        private UnsafeChunkedStack(MemoryChunk* sentinel, MemoryChunk* freeList, int chunks, int freeChunks, int maxFreeChunks, int size, int count, int offset, int version)
        {
            _sentinel = sentinel;
            _freeList = freeList;
            _chunks = chunks;
            _freeChunks = freeChunks;
            _maxFreeChunks = maxFreeChunks;
            _size = size;
            _count = count;
            _offset = offset;
            _version = version;
        }

        /// <summary>
        ///     Creates a chunked stack with the specified chunk size and free-list limit.
        /// </summary>
        /// <param name="size">The number of elements each chunk can hold. Must be greater than zero.</param>
        /// <param name="maxFreeChunks">The maximum number of free chunks to retain. Must be non-negative.</param>
        /// <returns>A new chunked stack.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="size" /> is not positive or <paramref name="maxFreeChunks" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeChunkedStack<T> Create(int size, int maxFreeChunks)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(size, ExceptionArgument.size);
            ThrowHelpers.ThrowIfNegative(maxFreeChunks, ExceptionArgument.maxFreeChunks);
            var chunk = CreateChunk(size);
            return new UnsafeChunkedStack<T>(chunk, null, 1, 0, maxFreeChunks, size, 0, 0, 0);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _sentinel != null;

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
        ///     Gets the number of elements each chunk can hold.
        /// </summary>
        public readonly int Size => _size;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeChunkedStack<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeChunkedStack<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeChunkedStack<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeChunkedStack<T> left, UnsafeChunkedStack<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeChunkedStack<T> left, UnsafeChunkedStack<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var node = _sentinel;
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
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_chunks != 1)
            {
                var chunkCount = _chunks.WrappingSub(1);
                _freeChunks = _freeChunks.WrappingAdd(chunkCount);
                _chunks = 1;
                var chunk = _sentinel->Next;
                var tail = chunk;
                for (var index = 1; index < chunkCount; index = index.WrappingAdd(1))
                    tail = tail->Next;

                tail->Next = _freeList;
                _freeList = chunk;
                TrimExcess(_maxFreeChunks);
            }

            _count = 0;
            _offset = 0;
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Pushes an item to the top of the stack.
        /// </summary>
        /// <param name="item">The item to add to the top of the stack.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T item)
        {
            if (_offset == _size)
            {
                _offset = 0;
                MemoryChunk* chunk;
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

                chunk->Next = _sentinel;
                _sentinel = chunk;
                _chunks = _chunks.WrappingAdd(1);
            }

            _count = _count.WrappingAdd(1);
            Unsafe.Add(ref Unsafe.AsRef<T>(_sentinel->Buffer), Isize.FromI32Unchecked((_offset = _offset.WrappingAdd(1)).WrappingSub(1))) = item;
            _version = _version.WrappingAdd(1);
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
            if (_count == 0)
            {
                result = default;
                return false;
            }

            _count = _count.WrappingSub(1);
            result = Unsafe.Add(ref Unsafe.AsRef<T>(_sentinel->Buffer), Isize.FromI32Unchecked(_offset = _offset.WrappingSub(1)));
            if (_offset == 0 && _chunks != 1)
            {
                _offset = _size;
                var chunk = _sentinel;
                _sentinel = chunk->Next;
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

            _version = _version.WrappingAdd(1);
            return true;
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
        public bool TryPeek(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_sentinel->Buffer), Isize.FromI32Unchecked(_offset.WrappingSub(1)));
            return true;
        }

        /// <summary>
        ///     Ensures that at least the specified number of free chunks are available for
        ///     reuse. The value is capped by the maximum number of free chunks retained by this.
        /// </summary>
        /// <param name="capacity">The minimum number of free chunks to retain.</param>
        /// <returns>The number of free chunks retained.</returns>
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
        ///     Frees all chunks that are currently unused.
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
        ///     Trims the pool of free chunks to the specified number of free chunks.
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
        ///     Copies up to the specified number of elements from this. The actual number of copied elements is
        ///     limited by the span's length, the specified count, and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        public readonly int CopyTo(Span<T> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var result = count = Math.Min(buffer.Length, Math.Min(count, _count));
            if (count == 0)
                return 0;

            var node = _sentinel;
            var elementCount = Math.Min(_offset, count);
            if (elementCount > 0)
            {
                ref var source = ref Unsafe.Add(ref Unsafe.AsRef<T>(node->Buffer), Isize.FromI32Unchecked(_offset.WrappingSub(elementCount)));
                StackHelpers.Copy(ref reference, ref source, elementCount);
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
                StackHelpers.Copy(ref reference, ref Unsafe.AsRef<T>(node->Buffer), elementCount);
                reference = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(elementCount));
            }

            if (remaining > 0)
            {
                elementCount = remaining;
                var elementOffset = _size.WrappingSub(elementCount);
                node = node->Next;
                StackHelpers.Copy(ref reference, ref Unsafe.Add(ref Unsafe.AsRef<T>(node->Buffer), Isize.FromI32Unchecked(elementOffset)), elementCount);
            }

            return result;
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
        public readonly int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer), count);

        /// <summary>
        ///     Copies all elements from this into a destination span. The span must
        ///     have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
        /// </exception>
        public readonly void CopyTo(Span<T> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var count = _count;
            if (count == 0)
                return;

            var node = _sentinel;
            var elementCount = Math.Min(_offset, count);
            if (elementCount > 0)
            {
                StackHelpers.Copy(ref reference, ref Unsafe.AsRef<T>(node->Buffer), elementCount);
                count = count.WrappingSub(elementCount);
            }

            if (count == 0)
                return;

            reference = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(elementCount));
            elementCount = _size;
            var chunks = count / elementCount;
            for (var i = 0; i < chunks; i = i.WrappingAdd(1))
            {
                node = node->Next;
                StackHelpers.Copy(ref reference, ref Unsafe.AsRef<T>(node->Buffer), elementCount);
                reference = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(elementCount));
            }
        }

        /// <summary>
        ///     Copies all elements from this into a destination span. The span must
        ///     have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer));

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="size">The number of elements to store in the chunk.</param>
        /// <returns>
        ///     A pointer to the newly allocated chunk whose data region holds <paramref name="size" /> elements.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MemoryChunk* CreateChunk(int size)
        {
            var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<MemoryChunk>(), NativeMemoryAllocator.AlignOf<T>());
            var chunkByteCount = NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(Unsafe.SizeOf<MemoryChunk>()), alignment);
            var chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc(chunkByteCount + Usize.FromI32Unchecked(size) * Usize.FromI32Unchecked(Unsafe.SizeOf<T>()), alignment);
            chunk->Buffer = UnsafeHelpers.AddByteOffset<T>(chunk, Isize.FromUsizeUnchecked(chunkByteCount));
            return chunk;
        }

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
            public T* Buffer;
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
            private readonly UnsafeChunkedStack<T>* _handle;

            /// <summary>
            ///     Used to keep the enumerator synchronized with the collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     The chunk currently being enumerated.
            /// </summary>
            private MemoryChunk* _currentChunk;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            private int _count;

            /// <summary>
            ///     The number of elements from the start.
            /// </summary>
            private int _offset;

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
            private Enumerator(UnsafeChunkedStack<T>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _currentChunk = handle->_sentinel;
                _count = handle->_count;
                _offset = handle->_offset;
                _current = default;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(UnsafeChunkedStack<T>* handle) => new(handle);

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
                if (_count == 0)
                    return false;

                _count = _count.WrappingSub(1);
                _current = Unsafe.Add(ref Unsafe.AsRef<T>(_currentChunk->Buffer), Isize.FromI32Unchecked(_offset = _offset.WrappingSub(1)));
                if (_offset == 0 && _count > 0)
                {
                    _offset = handle->_size;
                    _currentChunk = _currentChunk->Next;
                }

                return true;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                var handle = _handle;
                _currentChunk = handle->_sentinel;
                _count = handle->_count;
                _offset = handle->_offset;
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