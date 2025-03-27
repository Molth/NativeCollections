using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe sparseSet
    ///     //https://github.com/bombela/sparseset
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community | FromType.Rust)]
    public unsafe struct UnsafeSparseSet<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        ///     Dense
        /// </summary>
        private Entry* _dense;

        /// <summary>
        ///     Sparse
        /// </summary>
        private int* _sparse;

        /// <summary>
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Count
        /// </summary>
        private int _count;

        /// <summary>
        ///     Version
        /// </summary>
        private int _version;

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
        public T this[int key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (TryGetValue(key, out var obj))
                    return obj;
                throw new KeyNotFoundException(key.ToString());
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Insert(key, value);
        }

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length => _length;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _count;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeSparseSet(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            _dense = (Entry*)NativeMemoryAllocator.Alloc((uint)(capacity * (sizeof(Entry) + sizeof(int))));
            _sparse = (int*)((byte*)_dense + capacity * sizeof(Entry));
            MemoryMarshal.CreateSpan(ref *_sparse, capacity).Fill(-1);
            _length = capacity;
            _count = 0;
            _version = 0;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => NativeMemoryAllocator.Free(_dense);

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            MemoryMarshal.CreateSpan(ref *_sparse, _length).Fill(-1);
            _count = 0;
            ++_version;
        }

        /// <summary>
        ///     Set capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            if (capacity < _count)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "SmallCapacity");
            if (capacity != _length)
            {
                var dense = (Entry*)NativeMemoryAllocator.Alloc((uint)(capacity * (sizeof(Entry) + sizeof(int))));
                var sparse = (int*)((byte*)dense + capacity * sizeof(Entry));
                Unsafe.CopyBlockUnaligned(dense, _dense, (uint)(_count * sizeof(Entry)));
                Unsafe.CopyBlockUnaligned(sparse, _sparse, (uint)(_count * sizeof(int)));
                if (capacity > _length)
                    MemoryMarshal.CreateSpan(ref *(sparse + _length), capacity - _length).Fill(-1);
                _dense = dense;
                _sparse = sparse;
                _length = capacity;
                ++_version;
            }
        }

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(int key, in T value)
        {
            if (key < 0)
                throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
            if (key >= _length)
                throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
            var index = _sparse[key];
            if (index != -1)
                return false;
            ref var count = ref _count;
            ref var entry = ref _dense[count];
            entry.Key = key;
            entry.Value = value;
            _sparse[key] = count;
            ++count;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>
        ///     True if the key was newly added to the collection.
        ///     False if an existing key's value was replaced.
        ///     If the key was already set, the previous value is overridden.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int key, in T value)
        {
            if (key < 0)
                throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
            if (key >= _length)
                throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
            var index = _sparse[key];
            if (index != -1)
            {
                _dense[index].Value = value;
                ++_version;
                return false;
            }

            ref var count = ref _count;
            ref var entry = ref _dense[count];
            entry.Key = key;
            entry.Value = value;
            _sparse[key] = count;
            ++count;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key)
        {
            if (key < 0 || key >= _length)
                return false;
            var index = _sparse[key];
            if (index == -1)
                return false;
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref _dense[_count];
                _dense[index] = lastEntry;
                _sparse[lastEntry.Key] = index;
            }

            _sparse[key] = -1;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key, out T value)
        {
            if (key < 0 || key >= _length)
            {
                value = default;
                return false;
            }

            var index = _sparse[key];
            if (index == -1)
            {
                value = default;
                return false;
            }

            ref var entry = ref _dense[index];
            value = entry.Value;
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref _dense[_count];
                entry = lastEntry;
                _sparse[lastEntry.Key] = index;
            }

            _sparse[key] = -1;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Contains key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(int key) => key >= 0 && key < _length && _sparse[key] != -1;

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(int key, out T value)
        {
            if (key < 0 || key >= _length)
            {
                value = default;
                return false;
            }

            var index = _sparse[key];
            if (index != -1)
            {
                value = _dense[index].Value;
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
        public bool TryGetValueReference(int key, out NativeReference<T> value)
        {
            if (key < 0 || key >= _length)
            {
                value = default;
                return false;
            }

            var index = _sparse[key];
            if (index != -1)
            {
                ref var entry = ref _dense[index];
                value = new NativeReference<T>(Unsafe.AsPointer(ref entry.Value));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(int key) => key < 0 || key >= _length ? -1 : _sparse[key];

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>KeyValuePair</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<int, T> GetAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            return *(KeyValuePair<int, T>*)&_dense[index];
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>KeyValuePair</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<int, NativeReference<T>> GetReferenceAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            ref var entry = ref _dense[index];
            return new KeyValuePair<int, NativeReference<T>>(entry.Key, new NativeReference<T>(Unsafe.AsPointer(ref entry.Value)));
        }

        /// <summary>
        ///     Set at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index, in T value)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            _dense[index].Value = value;
            ++_version;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            ref var entry = ref _dense[index];
            var key = entry.Key;
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref _dense[_count];
                entry = lastEntry;
                _sparse[lastEntry.Key] = index;
            }

            _sparse[key] = -1;
            ++_version;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out KeyValuePair<int, T> keyValuePair)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            ref var entry = ref _dense[index];
            var key = entry.Key;
            keyValuePair = *(KeyValuePair<int, T>*)Unsafe.AsPointer(ref entry);
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref _dense[_count];
                entry = lastEntry;
                _sparse[lastEntry.Key] = index;
            }

            _sparse[key] = -1;
            ++_version;
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref *(KeyValuePair<int, T>*)_dense, _count);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref *(KeyValuePair<int, T>*)(_dense + start), _count - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref *(KeyValuePair<int, T>*)(_dense + start), length);

        /// <summary>
        ///     Entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            /// <summary>
            ///     Key
            /// </summary>
            public int Key;

            /// <summary>
            ///     Value
            /// </summary>
            public T Value;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeSparseSet<T> Empty => new();

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
            ///     NativeSparseSet
            /// </summary>
            private readonly UnsafeSparseSet<T>* _nativeSparseSet;

            /// <summary>
            ///     Version
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeSparseSet)
            {
                var handle = (UnsafeSparseSet<T>*)nativeSparseSet;
                _nativeSparseSet = handle;
                _version = handle->_version;
                _index = -1;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _nativeSparseSet;
                if (_version != handle->_version)
                    throw new InvalidOperationException("EnumFailedVersion");
                var num = _index + 1;
                if (num >= handle->_count)
                    return false;
                _index = num;
                return true;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public KeyValuePair<int, T> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => *(KeyValuePair<int, T>*)(&_nativeSparseSet->_dense[_index]);
            }
        }

        /// <summary>
        ///     Key collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct KeyCollection
        {
            /// <summary>
            ///     NativeSparseSet
            /// </summary>
            private readonly UnsafeSparseSet<T>* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(void* nativeSparseSet) => _nativeSparseSet = (UnsafeSparseSet<T>*)nativeSparseSet;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSparseSet->_count;

            /// <summary>
            ///     Get key
            /// </summary>
            /// <param name="index">Index</param>
            public int this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var handle = _nativeSparseSet;
                    if (index < 0)
                        throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
                    if (index >= handle->_count)
                        throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
                    return handle->_dense[index].Key;
                }
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSparseSet);

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator
            {
                /// <summary>
                ///     NativeSparseSet
                /// </summary>
                private readonly UnsafeSparseSet<T>* _nativeSparseSet;

                /// <summary>
                ///     Version
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Index
                /// </summary>
                private int _index;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSparseSet">NativeSparseSet</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSparseSet)
                {
                    var handle = (UnsafeSparseSet<T>*)nativeSparseSet;
                    _nativeSparseSet = handle;
                    _version = handle->_version;
                    _index = -1;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativeSparseSet;
                    if (_version != handle->_version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    var num = _index + 1;
                    if (num >= handle->_count)
                        return false;
                    _index = num;
                    return true;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public int Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _nativeSparseSet->_dense[_index].Key;
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
            ///     NativeSparseSet
            /// </summary>
            private readonly UnsafeSparseSet<T>* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(void* nativeSparseSet) => _nativeSparseSet = (UnsafeSparseSet<T>*)nativeSparseSet;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSparseSet->_count;

            /// <summary>
            ///     Get reference
            /// </summary>
            /// <param name="index">Index</param>
            public ref T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var handle = _nativeSparseSet;
                    if (index < 0)
                        throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
                    if (index >= handle->_count)
                        throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
                    return ref handle->_dense[index].Value;
                }
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSparseSet);

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator
            {
                /// <summary>
                ///     NativeSparseSet
                /// </summary>
                private readonly UnsafeSparseSet<T>* _nativeSparseSet;

                /// <summary>
                ///     Version
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Index
                /// </summary>
                private int _index;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSparseSet">NativeSparseSet</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSparseSet)
                {
                    var handle = (UnsafeSparseSet<T>*)nativeSparseSet;
                    _nativeSparseSet = handle;
                    _version = handle->_version;
                    _index = -1;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativeSparseSet;
                    if (_version != handle->_version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    var num = _index + 1;
                    if (num >= handle->_count)
                        return false;
                    _index = num;
                    return true;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public T Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _nativeSparseSet->_dense[_index].Value;
                }
            }
        }
    }
}