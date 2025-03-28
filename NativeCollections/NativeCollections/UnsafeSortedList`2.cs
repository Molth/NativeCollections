﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe sortedList
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeSortedList<TKey, TValue> : IDisposable where TKey : unmanaged, IComparable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Keys
        /// </summary>
        private TKey* _keys;

        /// <summary>
        ///     Values
        /// </summary>
        private TValue* _values;

        /// <summary>
        ///     Size
        /// </summary>
        private int _size;

        /// <summary>
        ///     Version
        /// </summary>
        private int _version;

        /// <summary>
        ///     Capacity
        /// </summary>
        private int _capacity;

        /// <summary>
        ///     Keys
        /// </summary>
        public KeyCollection Keys => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Values
        /// </summary>
        public ValueCollection Values => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="key">Key</param>
        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var index = BinarySearchHelpers.Find(_keys, _size, key);
                return index >= 0 ? _values[index] : throw new KeyNotFoundException(key.ToString());
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var index = BinarySearchHelpers.Find(_keys, _size, key);
                if (index >= 0)
                {
                    _values[index] = value;
                    ++_version;
                }
                else
                {
                    Insert(~index, key, value);
                }
            }
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
        public int Capacity => _capacity;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeSortedList(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            _keys = (TKey*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(TKey)));
            _values = (TValue*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(TValue)));
            _size = 0;
            _version = 0;
            _capacity = capacity;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            NativeMemoryAllocator.Free(_keys);
            NativeMemoryAllocator.Free(_values);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            ++_version;
            _size = 0;
        }

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in TKey key, in TValue value)
        {
            var num = BinarySearchHelpers.Find(_keys, _size, key);
            if (num >= 0)
                throw new ArgumentException($"AddingDuplicate, {key}", nameof(key));
            Insert(~num, key, value);
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key)
        {
            var index = BinarySearchHelpers.Find(_keys, _size, key);
            if (index >= 0)
            {
                --_size;
                if (index < _size)
                {
                    Unsafe.CopyBlockUnaligned(_keys + index, _keys + index + 1, (uint)((_size - index) * sizeof(TKey)));
                    Unsafe.CopyBlockUnaligned(_values + index, _values + index + 1, (uint)((_size - index) * sizeof(TValue)));
                }

                ++_version;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key, out TValue value)
        {
            var index = BinarySearchHelpers.Find(_keys, _size, key);
            if (index >= 0)
            {
                value = _values[index];
                --_size;
                if (index < _size)
                {
                    Unsafe.CopyBlockUnaligned(_keys + index, _keys + index + 1, (uint)((_size - index) * sizeof(TKey)));
                    Unsafe.CopyBlockUnaligned(_values + index, _values + index + 1, (uint)((_size - index) * sizeof(TValue)));
                }

                ++_version;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            --_size;
            if (index < _size)
            {
                Unsafe.CopyBlockUnaligned(_keys + index, _keys + index + 1, (uint)((_size - index) * sizeof(TKey)));
                Unsafe.CopyBlockUnaligned(_values + index, _values + index + 1, (uint)((_size - index) * sizeof(TValue)));
            }

            ++_version;
        }

        /// <summary>
        ///     Remove range
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRange(int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "MustBeNonNegative");
            if (count == 0)
                return;
            if (index + count > _size)
                throw new ArgumentOutOfRangeException(nameof(count), count, "MustBeLess");
            _size -= count;
            if (index < _size)
            {
                Unsafe.CopyBlockUnaligned(_keys + index, _keys + index + count, (uint)((_size - index) * sizeof(TKey)));
                Unsafe.CopyBlockUnaligned(_values + index, _values + index + count, (uint)((_size - index) * sizeof(TValue)));
            }

            ++_version;
        }

        /// <summary>
        ///     Get key at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TKey GetKeyAtIndex(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return _keys[index];
        }

        /// <summary>
        ///     Get value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueAtIndex(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return ref _values[index];
        }

        /// <summary>
        ///     Set value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValueAtIndex(int index, in TValue value)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            _values[index] = value;
            ++_version;
        }

        /// <summary>
        ///     Contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Contains key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in TKey key) => BinarySearchHelpers.Find(_keys, _size, key) >= 0;

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in TKey key, out TValue value)
        {
            var index = BinarySearchHelpers.Find(_keys, _size, key);
            if (index >= 0)
            {
                value = _values[index];
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReference(in TKey key, out NativeReference<TValue> value)
        {
            var index = BinarySearchHelpers.Find(_keys, _size, key);
            if (index >= 0)
            {
                value = new NativeReference<TValue>(Unsafe.AsPointer(ref _values[index]));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            if (_capacity < capacity)
            {
                var newCapacity = 2 * _capacity;
                if ((uint)newCapacity > 2147483591)
                    newCapacity = 2147483591;
                var expected = _capacity + 4;
                newCapacity = newCapacity > expected ? newCapacity : expected;
                if (newCapacity < capacity)
                    newCapacity = capacity;
                SetCapacity(newCapacity);
            }

            return _capacity;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = (int)(_capacity * 0.9);
            if (_size < threshold)
                SetCapacity(_size);
            return _capacity;
        }

        /// <summary>
        ///     Set capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            if (capacity < _size)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Small_Capacity");
            if (capacity != _capacity)
            {
                if (capacity > 0)
                {
                    var keys = (TKey*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(TKey)));
                    var values = (TValue*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(TValue)));
                    if (_size > 0)
                    {
                        Unsafe.CopyBlockUnaligned(keys, _keys, (uint)(_size * sizeof(TKey)));
                        Unsafe.CopyBlockUnaligned(values, _values, (uint)(_size * sizeof(TValue)));
                    }

                    NativeMemoryAllocator.Free(_keys);
                    NativeMemoryAllocator.Free(_values);
                    _keys = keys;
                    _values = values;
                }
                else
                {
                    NativeMemoryAllocator.Free(_keys);
                    NativeMemoryAllocator.Free(_values);
                    _keys = (TKey*)NativeMemoryAllocator.Alloc(0);
                    _values = (TValue*)NativeMemoryAllocator.Alloc(0);
                }

                _capacity = capacity;
            }
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Insert(int index, in TKey key, in TValue value)
        {
            if (_size == _capacity)
                EnsureCapacity(_size + 1);
            if (index < _size)
            {
                Unsafe.CopyBlockUnaligned(_keys + index + 1, _keys + index, (uint)((_size - index) * sizeof(TKey)));
                Unsafe.CopyBlockUnaligned(_values + index + 1, _values + index, (uint)((_size - index) * sizeof(TValue)));
            }

            _keys[index] = key;
            _values[index] = value;
            ++_size;
            ++_version;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeSortedList<TKey, TValue> Empty => new();

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
            ///     NativeSortedList
            /// </summary>
            private readonly UnsafeSortedList<TKey, TValue>* _nativeSortedList;

            /// <summary>
            ///     Current
            /// </summary>
            private KeyValuePair<TKey, TValue> _current;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Version
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedList">NativeSortedList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeSortedList)
            {
                var handle = (UnsafeSortedList<TKey, TValue>*)nativeSortedList;
                _nativeSortedList = handle;
                _current = default;
                _index = 0;
                _version = handle->_version;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _nativeSortedList;
                if (_version != handle->_version)
                    throw new InvalidOperationException("EnumFailedVersion");
                if ((uint)_index < (uint)handle->_size)
                {
                    _current = new KeyValuePair<TKey, TValue>(handle->_keys[_index], handle->_values[_index]);
                    ++_index;
                    return true;
                }

                _index = handle->_size + 1;
                return false;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }

        /// <summary>
        ///     Key collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct KeyCollection
        {
            /// <summary>
            ///     NativeSortedList
            /// </summary>
            private readonly UnsafeSortedList<TKey, TValue>* _nativeSortedList;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedList">NativeSortedList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(void* nativeSortedList) => _nativeSortedList = (UnsafeSortedList<TKey, TValue>*)nativeSortedList;

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TKey> AsReadOnlySpan()
            {
                var handle = _nativeSortedList;
                return MemoryMarshal.CreateReadOnlySpan(ref *handle->_keys, handle->_size);
            }

            /// <summary>
            ///     As span
            /// </summary>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnlySpan<TKey>(KeyCollection keyCollection) => keyCollection.AsReadOnlySpan();

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSortedList);

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator
            {
                /// <summary>
                ///     NativeSortedList
                /// </summary>
                private readonly UnsafeSortedList<TKey, TValue>* _nativeSortedList;

                /// <summary>
                ///     Current
                /// </summary>
                private TKey _current;

                /// <summary>
                ///     Index
                /// </summary>
                private int _index;

                /// <summary>
                ///     Version
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSortedList">NativeSortedList</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSortedList)
                {
                    var handle = (UnsafeSortedList<TKey, TValue>*)nativeSortedList;
                    _nativeSortedList = handle;
                    _current = default;
                    _index = 0;
                    _version = handle->_version;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativeSortedList;
                    if (_version != handle->_version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    if ((uint)_index < (uint)handle->_size)
                    {
                        _current = handle->_keys[_index];
                        ++_index;
                        return true;
                    }

                    _index = handle->_size + 1;
                    return false;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public TKey Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }
            }
        }

        /// <summary>
        ///     Value collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct ValueCollection
        {
            /// <summary>
            ///     NativeSortedList
            /// </summary>
            private readonly UnsafeSortedList<TKey, TValue>* _nativeSortedList;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedList">NativeSortedList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(void* nativeSortedList) => _nativeSortedList = (UnsafeSortedList<TKey, TValue>*)nativeSortedList;

            /// <summary>
            ///     As span
            /// </summary>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<TValue> AsSpan()
            {
                var handle = _nativeSortedList;
                return MemoryMarshal.CreateSpan(ref *handle->_values, handle->_size);
            }

            /// <summary>
            ///     As span
            /// </summary>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Span<TValue>(ValueCollection valueCollection) => valueCollection.AsSpan();

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSortedList);

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator
            {
                /// <summary>
                ///     NativeSortedList
                /// </summary>
                private readonly UnsafeSortedList<TKey, TValue>* _nativeSortedList;

                /// <summary>
                ///     Current
                /// </summary>
                private TValue _current;

                /// <summary>
                ///     Index
                /// </summary>
                private int _index;

                /// <summary>
                ///     Version
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSortedList">NativeSortedList</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSortedList)
                {
                    var handle = (UnsafeSortedList<TKey, TValue>*)nativeSortedList;
                    _nativeSortedList = handle;
                    _current = default;
                    _index = 0;
                    _version = handle->_version;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativeSortedList;
                    if (_version != handle->_version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    if ((uint)_index < (uint)handle->_size)
                    {
                        _current = handle->_values[_index];
                        ++_index;
                        return true;
                    }

                    _index = handle->_size + 1;
                    return false;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }
            }
        }
    }
}