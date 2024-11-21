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
    [NativeCollection]
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
        public void Clear()
        {
            var handle = _handle;
            handle->Size = 0;
            handle->Head = 0;
            handle->Tail = 0;
            handle->Version++;
        }

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
            var handle = _handle;
            if (handle->Size == handle->Length)
                Grow(handle->Size + 1);
            handle->Array[handle->Tail] = item;
            if (++handle->Tail == handle->Length)
                handle->Tail = 0;
            ++handle->Size;
            ++handle->Version;
        }

        /// <summary>
        ///     Try add
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Added</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(in T item)
        {
            var handle = _handle;
            if (handle->Size == handle->Length)
                return false;
            handle->Array[handle->Tail] = item;
            if (++handle->Tail == handle->Length)
                handle->Tail = 0;
            ++handle->Size;
            ++handle->Version;
            return true;
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result)
        {
            var handle = _handle;
            if (handle->Size == 0)
            {
                result = default;
                return false;
            }

            result = handle->Array[handle->Head];
            if (++handle->Head == handle->Length)
                handle->Head = 0;
            --handle->Size;
            ++handle->Version;
            return true;
        }

        /// <summary>
        ///     Try pop
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Popped</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result)
        {
            var handle = _handle;
            if (handle->Size == 0)
            {
                result = default;
                return false;
            }

            if (--handle->Tail == -1)
                handle->Tail = handle->Length - 1;
            result = handle->Array[handle->Tail];
            --handle->Size;
            ++handle->Version;
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
            var handle = _handle;
            if (handle->Length < capacity)
                Grow(capacity);
            return handle->Length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var handle = _handle;
            var threshold = (int)(handle->Length * 0.9);
            if (handle->Size < threshold)
                SetCapacity(handle->Size);
            return handle->Length;
        }

        /// <summary>
        ///     Set capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCapacity(int capacity)
        {
            var handle = _handle;
            var destinationArray = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(T)));
            if (handle->Head == 0)
            {
                Unsafe.CopyBlockUnaligned(destinationArray, handle->Array, (uint)(handle->Size * sizeof(T)));
            }
            else
            {
                Unsafe.CopyBlockUnaligned(destinationArray, handle->Array + handle->Head, (uint)((handle->Length - handle->Head) * sizeof(T)));
                Unsafe.CopyBlockUnaligned(destinationArray + (handle->Length - handle->Head), handle->Array, (uint)(handle->Tail * sizeof(T)));
            }

            NativeMemoryAllocator.Free(handle->Array);
            handle->Array = destinationArray;
            handle->Length = capacity;
            handle->Head = 0;
            handle->Tail = handle->Size;
        }

        /// <summary>
        ///     Grow
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var handle = _handle;
            var newCapacity = 2 * handle->Length;
            if ((uint)newCapacity > 2147483591)
                newCapacity = 2147483591;
            var expected = handle->Length + 4;
            newCapacity = newCapacity > expected ? newCapacity : expected;
            if (newCapacity < capacity)
                newCapacity = capacity;
            SetCapacity(newCapacity);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeDeque<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(this);

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     NativeDeque
            /// </summary>
            private readonly NativeDeque<T> _nativeDeque;

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
            internal Enumerator(in NativeDeque<T> nativeDeque)
            {
                _nativeDeque = nativeDeque;
                _version = nativeDeque._handle->Version;
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
                var handle = _nativeDeque._handle;
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