using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe stack
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeStack<T> : IDisposable where T : unmanaged
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
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[index];
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[index];
        }

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _size == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _size;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity => _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeStack(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            _buffer = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(T)));
            _length = capacity;
            _size = 0;
            _version = 0;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => NativeMemoryAllocator.Free(_buffer);

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _size = 0;
            _version++;
        }

        /// <summary>
        ///     Push
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(in T item)
        {
            var size = _size;
            if ((uint)size < (uint)_length)
            {
                _buffer[size] = item;
                _version++;
                _size = size + 1;
            }
            else
            {
                Grow(_size + 1);
                _buffer[_size] = item;
                _version++;
                _size++;
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
            var size = _size;
            if ((uint)size < (uint)_length)
            {
                _buffer[size] = item;
                _version++;
                _size = size + 1;
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
            var size = _size - 1;
            if ((uint)size >= (uint)_length)
                throw new InvalidOperationException("EmptyStack");
            _version++;
            _size = size;
            var item = _buffer[size];
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
            var size = _size - 1;
            if ((uint)size >= (uint)_length)
            {
                result = default;
                return false;
            }

            _version++;
            _size = size;
            result = _buffer[size];
            return true;
        }

        /// <summary>
        ///     Peek
        /// </summary>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek()
        {
            var size = _size - 1;
            return (uint)size >= (uint)_length ? throw new InvalidOperationException("EmptyStack") : _buffer[size];
        }

        /// <summary>
        ///     Try peek
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T result)
        {
            var size = _size - 1;
            if ((uint)size >= (uint)_length)
            {
                result = default;
                return false;
            }

            result = _buffer[size];
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
            if (_length < capacity)
                Grow(capacity);
            return _length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = (int)(_length * 0.9);
            if (_size < threshold)
                SetCapacity(_size);
            return _length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < _size || capacity >= _length)
                return _length;
            SetCapacity(capacity);
            return _length;
        }

        /// <summary>
        ///     Set capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCapacity(int capacity)
        {
            var newBuffer = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(T)));
            if (_size > 0)
                Unsafe.CopyBlockUnaligned(newBuffer, _buffer, (uint)(_length * sizeof(T)));
            NativeMemoryAllocator.Free(_buffer);
            _buffer = newBuffer;
            _length = capacity;
        }

        /// <summary>
        ///     Grow
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var newCapacity = 2 * _length;
            if ((uint)newCapacity > 2147483591)
                newCapacity = 2147483591;
            var expected = _length + 4;
            newCapacity = newCapacity > expected ? newCapacity : expected;
            if (newCapacity < capacity)
                newCapacity = capacity;
            SetCapacity(newCapacity);
        }

        /// <summary>
        ///     Get byte count
        /// </summary>
        /// <returns>Byte count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetByteCount() => _size * sizeof(T);

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<byte> buffer)
        {
            ref var reference = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(buffer));
            var num1 = 0;
            var num2 = _size;
            while (num1 < _size)
                Unsafe.Add(ref reference, --num2) = _buffer[num1++];
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeStack<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     NativeStack
            /// </summary>
            private readonly UnsafeStack<T>* _nativeStack;

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
                var handle = (UnsafeStack<T>*)nativeStack;
                _nativeStack = handle;
                _version = handle->_version;
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
                if (_version != handle->_version)
                    throw new InvalidOperationException("EnumFailedVersion");
                bool returned;
                if (_index == -2)
                {
                    _index = handle->_size - 1;
                    returned = _index >= 0;
                    if (returned)
                        _currentElement = handle->_buffer[_index];
                    return returned;
                }

                if (_index == -1)
                    return false;
                returned = --_index >= 0;
                _currentElement = returned ? handle->_buffer[_index] : default;
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