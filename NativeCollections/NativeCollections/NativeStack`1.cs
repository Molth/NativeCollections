using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native stack
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.Standard)]
    public readonly unsafe struct NativeStack<T> : IDisposable, IEquatable<NativeStack<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeStackHandle
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
                get => ref Array[index];
            }

            /// <summary>
            ///     Get reference
            /// </summary>
            /// <param name="index">Index</param>
            public ref T this[uint index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref Array[index];
            }

            /// <summary>
            ///     Clear
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                Size = 0;
                Version++;
            }

            /// <summary>
            ///     Push
            /// </summary>
            /// <param name="item">Item</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Push(in T item)
            {
                var size = Size;
                if ((uint)size < (uint)Length)
                {
                    Array[size] = item;
                    Version++;
                    Size = size + 1;
                }
                else
                {
                    Grow(Size + 1);
                    Array[Size] = item;
                    Version++;
                    Size++;
                }
            }

            /// <summary>
            ///     Try push
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Pushed</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPush(in T item)
            {
                var size = Size;
                if ((uint)size < (uint)Length)
                {
                    Array[size] = item;
                    Version++;
                    Size = size + 1;
                    return true;
                }

                return false;
            }

            /// <summary>
            ///     Pop
            /// </summary>
            /// <returns>Item</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Pop()
            {
                var size = Size - 1;
                if ((uint)size >= (uint)Length)
                    throw new InvalidOperationException("EmptyStack");
                Version++;
                Size = size;
                var item = Array[size];
                return item;
            }

            /// <summary>
            ///     Try pop
            /// </summary>
            /// <param name="result">Item</param>
            /// <returns>Popped</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPop(out T result)
            {
                var size = Size - 1;
                if ((uint)size >= (uint)Length)
                {
                    result = default;
                    return false;
                }

                Version++;
                Size = size;
                result = Array[size];
                return true;
            }

            /// <summary>
            ///     Peek
            /// </summary>
            /// <returns>Item</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Peek()
            {
                var size = Size - 1;
                return (uint)size >= (uint)Length ? throw new InvalidOperationException("EmptyStack") : Array[size];
            }

            /// <summary>
            ///     Try peek
            /// </summary>
            /// <param name="result">Item</param>
            /// <returns>Peeked</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPeek(out T result)
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
                    Unsafe.CopyBlockUnaligned(newArray, Array, (uint)(Length * sizeof(T)));
                NativeMemoryAllocator.Free(Array);
                Array = newArray;
                Length = capacity;
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
        private readonly NativeStackHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeStack(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            var handle = (NativeStackHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeStackHandle));
            handle->Array = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(T)));
            handle->Length = capacity;
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
        public bool Equals(NativeStack<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeStack<T> nativeStack && nativeStack == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeStack<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeStack<T> left, NativeStack<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeStack<T> left, NativeStack<T> right) => left._handle != right._handle;

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
        ///     Push
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(in T item) => _handle->Push(item);

        /// <summary>
        ///     Try push
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Pushed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPush(in T item) => _handle->TryPush(item);

        /// <summary>
        ///     Pop
        /// </summary>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop() => _handle->Pop();

        /// <summary>
        ///     Try pop
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Popped</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result) => _handle->TryPop(out result);

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
        public static NativeStack<T> Empty => new();

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
            ///     NativeStack
            /// </summary>
            private readonly NativeStackHandle* _nativeStack;

            /// <summary>
            ///     Version
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Current element
            /// </summary>
            private T _currentElement;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeStack">NativeStack</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeStack)
            {
                var handle = (NativeStackHandle*)nativeStack;
                _nativeStack = handle;
                _version = handle->Version;
                _index = -2;
                _currentElement = default;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _nativeStack;
                if (_version != handle->Version)
                    throw new InvalidOperationException("EnumFailedVersion");
                bool returned;
                if (_index == -2)
                {
                    _index = handle->Size - 1;
                    returned = _index >= 0;
                    if (returned)
                        _currentElement = handle->Array[_index];
                    return returned;
                }

                if (_index == -1)
                    return false;
                returned = --_index >= 0;
                _currentElement = returned ? handle->Array[_index] : default;
                return returned;
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