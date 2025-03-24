using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native queue
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.Standard)]
    public readonly unsafe struct NativeQueue<T> : IDisposable, IEquatable<NativeQueue<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeQueueHandle
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
            ///     Enqueue
            /// </summary>
            /// <param name="item">Item</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Enqueue(in T item)
            {
                if (Size == Length)
                    Grow(Size + 1);
                Array[Tail] = item;
                MoveNext(ref Tail);
                Size++;
                Version++;
            }

            /// <summary>
            ///     Try enqueue
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Enqueued</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryEnqueue(in T item)
            {
                if (Size != Length)
                {
                    Array[Tail] = item;
                    MoveNext(ref Tail);
                    Size++;
                    Version++;
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
                if (Size == 0)
                    throw new InvalidOperationException("EmptyQueue");
                var removed = Array[Head];
                MoveNext(ref Head);
                Size--;
                Version++;
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
                if (Size == 0)
                {
                    result = default;
                    return false;
                }

                result = Array[Head];
                MoveNext(ref Head);
                Size--;
                Version++;
                return true;
            }

            /// <summary>
            ///     Peek
            /// </summary>
            /// <returns>Item</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Peek() => Size == 0 ? throw new InvalidOperationException("EmptyQueue") : Array[Head];

            /// <summary>
            ///     Try peek
            /// </summary>
            /// <param name="result">Item</param>
            /// <returns>Peeked</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPeek(out T result)
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

            /// <summary>
            ///     Move next
            /// </summary>
            /// <param name="index">Index</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void MoveNext(ref int index)
            {
                var tmp = index + 1;
                if (tmp == Length)
                    tmp = 0;
                index = tmp;
            }
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeQueueHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeQueue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            var handle = (NativeQueueHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeQueueHandle));
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
        public bool Equals(NativeQueue<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeQueue<T> nativeQueue && nativeQueue == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeQueue<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeQueue<T> left, NativeQueue<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeQueue<T> left, NativeQueue<T> right) => left._handle != right._handle;

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
        ///     Enqueue
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in T item) => _handle->Enqueue(item);

        /// <summary>
        ///     Try enqueue
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(in T item) => _handle->TryEnqueue(item);

        /// <summary>
        ///     Dequeue
        /// </summary>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue() => _handle->Dequeue();

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result) => _handle->TryDequeue(out result);

        /// <summary>
        ///     Peek
        /// </summary>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek() => _handle->Peek();

        /// <summary>
        ///     Try peek
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T result) => _handle->TryPeek(out result);

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
        public static NativeQueue<T> Empty => new();

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
            ///     NativeQueue
            /// </summary>
            private readonly NativeQueueHandle* _nativeQueue;

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
                var handle = (NativeQueueHandle*)nativeQueue;
                _nativeQueue = handle;
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
                var handle = _nativeQueue;
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