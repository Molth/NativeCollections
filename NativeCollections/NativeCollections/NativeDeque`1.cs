using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native deque
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.Standard)]
    public readonly unsafe struct NativeDeque<T> : IDisposable, IEquatable<NativeDeque<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeDequeHandle
        {
            /// <summary>
            ///     Array
            /// </summary>
            public T* Array;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;

            /// <summary>
            ///     Head
            /// </summary>
            public int Head;

            /// <summary>
            ///     Tail
            /// </summary>
            public int Tail;

            /// <summary>
            ///     Size
            /// </summary>
            public int Size;

            /// <summary>
            ///     Version
            /// </summary>
            public int Version;

            /// <summary>
            ///     Get reference
            /// </summary>
            /// <param name="index">Index</param>
            public ref T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref Array[(Head + index) % Length];
            }

            /// <summary>
            ///     Get reference
            /// </summary>
            /// <param name="index">Index</param>
            public ref T this[uint index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref Array[(Head + index) % Length];
            }

            /// <summary>
            ///     Clear
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                Size = 0;
                Head = 0;
                Tail = 0;
                Version++;
            }

            /// <summary>
            ///     Enqueue head
            /// </summary>
            /// <param name="item">Item</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void EnqueueHead(in T item)
            {
                if (Size == Length)
                    Grow(Size + 1);
                if (--Head == -1)
                    Head = Length - 1;
                Array[Head] = item;
                ++Size;
                ++Version;
            }

            /// <summary>
            ///     Try enqueue head
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Enqueued</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryEnqueueHead(in T item)
            {
                if (Size == Length)
                    return false;
                if (--Head == -1)
                    Head = Length - 1;
                Array[Head] = item;
                ++Size;
                ++Version;
                return true;
            }

            /// <summary>
            ///     Enqueue tail
            /// </summary>
            /// <param name="item">Item</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void EnqueueTail(in T item)
            {
                if (Size == Length)
                    Grow(Size + 1);
                Array[Tail] = item;
                if (++Tail == Length)
                    Tail = 0;
                ++Size;
                ++Version;
            }

            /// <summary>
            ///     Try enqueue tail
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Enqueued</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryEnqueueTail(in T item)
            {
                if (Size == Length)
                    return false;
                Array[Tail] = item;
                if (++Tail == Length)
                    Tail = 0;
                ++Size;
                ++Version;
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
                if (Size == 0)
                {
                    result = default;
                    return false;
                }

                result = Array[Head];
                if (++Head == Length)
                    Head = 0;
                --Size;
                ++Version;
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
                if (Size == 0)
                {
                    result = default;
                    return false;
                }

                if (--Tail == -1)
                    Tail = Length - 1;
                result = Array[Tail];
                --Size;
                ++Version;
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
                if (Size == 0)
                {
                    result = default;
                    return false;
                }

                result = Array[Head];
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
                var size = Size - 1;
                if ((uint)size >= (uint)Length)
                {
                    result = default;
                    return false;
                }

                result = Array[size];
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
                if (Length < capacity)
                    Grow(capacity);
                return Length;
            }

            /// <summary>
            ///     Trim excess
            /// </summary>
            /// <returns>New capacity</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int TrimExcess()
            {
                var threshold = (int)(Length * 0.9);
                if (Size < threshold)
                    SetCapacity(Size);
                return Length;
            }

            /// <summary>
            ///     Set capacity
            /// </summary>
            /// <param name="capacity">Capacity</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void SetCapacity(int capacity)
            {
                var newArray = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(T)));
                if (Size > 0)
                {
                    if (Head < Tail)
                    {
                        Unsafe.CopyBlockUnaligned(newArray, Array + Head, (uint)(Size * sizeof(T)));
                    }
                    else
                    {
                        Unsafe.CopyBlockUnaligned(newArray, Array + Head, (uint)((Length - Head) * sizeof(T)));
                        Unsafe.CopyBlockUnaligned(newArray + Length - Head, Array, (uint)(Tail * sizeof(T)));
                    }
                }

                NativeMemoryAllocator.Free(Array);
                Array = newArray;
                Length = capacity;
                Head = 0;
                Tail = Size == capacity ? 0 : Size;
                Version++;
            }

            /// <summary>
            ///     Grow
            /// </summary>
            /// <param name="capacity">Capacity</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Grow(int capacity)
            {
                var newCapacity = 2 * Length;
                if ((uint)newCapacity > 2147483591)
                    newCapacity = 2147483591;
                var expected = Length + 4;
                newCapacity = newCapacity > expected ? newCapacity : expected;
                if (newCapacity < capacity)
                    newCapacity = capacity;
                SetCapacity(newCapacity);
            }
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeDequeHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDeque(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            var handle = (NativeDequeHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeDequeHandle));
            handle->Array = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(T)));
            handle->Length = capacity;
            handle->Head = 0;
            handle->Tail = 0;
            handle->Size = 0;
            handle->Version = 0;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _handle->Size == 0;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref (*_handle)[index];
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref (*_handle)[index];
        }

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _handle->Size;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeDeque<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeDeque<T> nativeDeque && nativeDeque == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeDeque<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeDeque<T> left, NativeDeque<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeDeque<T> left, NativeDeque<T> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            NativeMemoryAllocator.Free(handle->Array);
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Enqueue head
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnqueueHead(in T item) => _handle->EnqueueHead(item);

        /// <summary>
        ///     Try enqueue head
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueHead(in T item) => _handle->TryEnqueueHead(item);

        /// <summary>
        ///     Enqueue tail
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnqueueTail(in T item) => _handle->EnqueueTail(item);

        /// <summary>
        ///     Try enqueue tail
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueTail(in T item) => _handle->TryEnqueueTail(item);

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueHead(out T result) => _handle->TryDequeueHead(out result);

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueTail(out T result) => _handle->TryDequeueTail(out result);

        /// <summary>
        ///     Try peek head
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekHead(out T result) => _handle->TryPeekHead(out result);

        /// <summary>
        ///     Try peek tail
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekTail(out T result) => _handle->TryPeekTail(out result);

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeDeque<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(_handle);

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     NativeDeque
            /// </summary>
            private readonly NativeDequeHandle* _nativeDeque;

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
                var handle = (NativeDequeHandle*)nativeDeque;
                _nativeDeque = handle;
                _version = handle->Version;
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
                if (_version != handle->Version)
                    throw new InvalidOperationException("EnumFailedVersion");
                if (_index == -2)
                    return false;
                _index++;
                if (_index == handle->Size)
                {
                    _index = -2;
                    _currentElement = default;
                    return false;
                }

                var array = handle->Array;
                var capacity = (uint)handle->Length;
                var arrayIndex = (uint)(handle->Head + _index);
                if (arrayIndex >= capacity)
                    arrayIndex -= capacity;
                _currentElement = array[arrayIndex];
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