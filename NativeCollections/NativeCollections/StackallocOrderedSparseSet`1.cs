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
    ///     Stackalloc orderedSparseSet
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.None)]
    public unsafe struct StackallocOrderedSparseSet<T> where T : unmanaged
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
        ///     Head
        /// </summary>
        private int _head;

        /// <summary>
        ///     Tail
        /// </summary>
        private int _tail;

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
        ///     Keys
        /// </summary>
        public OrderedKeyCollection OrderedKeys => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Values
        /// </summary>
        public OrderedValueCollection OrderedValues => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     KeyValuePairs
        /// </summary>
        public OrderedKeyValuePairCollection OrderedKeyValuePairs => new(Unsafe.AsPointer(ref this));

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
        ///     Get buffer size
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Buffer size</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBufferSize(int capacity) => capacity * (sizeof(Entry) + sizeof(int));

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocOrderedSparseSet(Span<byte> buffer, int capacity)
        {
            _dense = (Entry*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
            _sparse = (int*)((byte*)_dense + capacity * sizeof(Entry));
            MemoryMarshal.CreateSpan(ref *_sparse, capacity).Fill(-1);
            _length = capacity;
            _head = -1;
            _tail = -1;
            _count = 0;
            _version = 0;
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            MemoryMarshal.CreateSpan(ref *_sparse, _length).Fill(-1);
            _head = -1;
            _tail = -1;
            _count = 0;
            ++_version;
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
                throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeLessOrEqual");
            var index = _sparse[key];
            if (index != -1)
                return false;
            ref var count = ref _count;
            ref var entry = ref _dense[count];
            entry.Key = key;
            entry.Value = value;
            entry.Next = -1;
            _sparse[key] = count;
            ref var tail = ref _tail;
            if (tail != -1)
            {
                _dense[tail].Next = count;
                entry.Previous = tail;
            }
            else
            {
                _head = count;
                entry.Previous = -1;
            }

            tail = count;
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
                throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeLessOrEqual");
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
            entry.Next = -1;
            _sparse[key] = count;
            ref var tail = ref _tail;
            if (tail != -1)
            {
                _dense[tail].Next = count;
                entry.Previous = tail;
            }
            else
            {
                _head = count;
                entry.Previous = -1;
            }

            tail = count;
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
            ref var entry = ref _dense[index];
            if (entry.Next != -1)
                _dense[entry.Next].Previous = entry.Previous;
            else
                _tail = entry.Previous;
            if (entry.Previous != -1)
                _dense[entry.Previous].Next = entry.Next;
            else
                _head = entry.Next;
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref _dense[_count];
                entry = lastEntry;
                if (entry.Next != -1)
                    _dense[entry.Next].Previous = index;
                else
                    _tail = index;
                if (entry.Previous != -1)
                    _dense[entry.Previous].Next = index;
                else
                    _head = index;
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
            if (entry.Next != -1)
                _dense[entry.Next].Previous = entry.Previous;
            else
                _tail = entry.Previous;
            if (entry.Previous != -1)
                _dense[entry.Previous].Next = entry.Next;
            else
                _head = entry.Next;
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref _dense[_count];
                entry = lastEntry;
                if (entry.Next != -1)
                    _dense[entry.Next].Previous = index;
                else
                    _tail = index;
                if (entry.Previous != -1)
                    _dense[entry.Previous].Next = index;
                else
                    _head = index;
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
        /// <returns>Key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKeyAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            return _dense[index].Key;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetValueAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            return ref _dense[index].Value;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <returns>Key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetKeyAt(int index, out int key)
        {
            if (index < 0 || index >= _count)
            {
                key = default;
                return false;
            }

            key = _dense[index].Key;
            return true;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueAt(int index, out T value)
        {
            if (index < 0 || index >= _count)
            {
                value = default;
                return false;
            }

            value = _dense[index].Value;
            return true;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReferenceAt(int index, out NativeReference<T> value)
        {
            if (index < 0 || index >= _count)
            {
                value = default;
                return false;
            }

            value = new NativeReference<T>(Unsafe.AsPointer(ref _dense[index].Value));
            return true;
        }

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
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetAt(int index, out KeyValuePair<int, T> keyValuePair)
        {
            if (index < 0 || index >= _count)
            {
                keyValuePair = default;
                return false;
            }

            keyValuePair = *(KeyValuePair<int, T>*)&_dense[index];
            return true;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetReferenceAt(int index, out KeyValuePair<int, NativeReference<T>> keyValuePair)
        {
            if (index < 0 || index >= _count)
            {
                keyValuePair = default;
                return false;
            }

            ref var entry = ref _dense[index];
            keyValuePair = new KeyValuePair<int, NativeReference<T>>(entry.Key, new NativeReference<T>(Unsafe.AsPointer(ref entry.Value)));
            return true;
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
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index)
        {
            if (index < 0 || index >= _count)
                return false;
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
            return true;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index, out KeyValuePair<int, T> keyValuePair)
        {
            if (index < 0 || index >= _count)
            {
                keyValuePair = default;
                return false;
            }

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
            return true;
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
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<KeyValuePair<int, T>>(in StackallocOrderedSparseSet<T> stackallocOrderedSparseSet) => stackallocOrderedSparseSet.AsReadOnlySpan();

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

            /// <summary>
            ///     Next
            /// </summary>
            public int Next;

            /// <summary>
            ///     Previous
            /// </summary>
            public int Previous;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocOrderedSparseSet<T> Empty => new();

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
            private readonly StackallocOrderedSparseSet<T>* _nativeSparseSet;

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
                var handle = (StackallocOrderedSparseSet<T>*)nativeSparseSet;
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
            private readonly StackallocOrderedSparseSet<T>* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(void* nativeSparseSet) => _nativeSparseSet = (StackallocOrderedSparseSet<T>*)nativeSparseSet;

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
                private readonly StackallocOrderedSparseSet<T>* _nativeSparseSet;

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
                    var handle = (StackallocOrderedSparseSet<T>*)nativeSparseSet;
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
            private readonly StackallocOrderedSparseSet<T>* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(void* nativeSparseSet) => _nativeSparseSet = (StackallocOrderedSparseSet<T>*)nativeSparseSet;

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
                private readonly StackallocOrderedSparseSet<T>* _nativeSparseSet;

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
                    var handle = (StackallocOrderedSparseSet<T>*)nativeSparseSet;
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

        /// <summary>
        ///     Key collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct OrderedKeyCollection
        {
            /// <summary>
            ///     NativeSparseSet
            /// </summary>
            private readonly StackallocOrderedSparseSet<T>* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal OrderedKeyCollection(void* nativeSparseSet) => _nativeSparseSet = (StackallocOrderedSparseSet<T>*)nativeSparseSet;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSparseSet->_count;

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
                private readonly StackallocOrderedSparseSet<T>* _nativeSparseSet;

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
                private Entry* _current;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSparseSet">NativeSparseSet</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSparseSet)
                {
                    var handle = (StackallocOrderedSparseSet<T>*)nativeSparseSet;
                    _nativeSparseSet = handle;
                    _version = handle->_version;
                    _index = -1;
                    _current = handle->_head != -1 ? &handle->_dense[handle->_head] : null;
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
                    if (num != 0)
                        _current = &handle->_dense[_current->Next];
                    return true;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public int Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current->Key;
                }
            }
        }

        /// <summary>
        ///     Value collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct OrderedValueCollection
        {
            /// <summary>
            ///     NativeSparseSet
            /// </summary>
            private readonly StackallocOrderedSparseSet<T>* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal OrderedValueCollection(void* nativeSparseSet) => _nativeSparseSet = (StackallocOrderedSparseSet<T>*)nativeSparseSet;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSparseSet->_count;

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
                private readonly StackallocOrderedSparseSet<T>* _nativeSparseSet;

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
                private Entry* _current;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSparseSet">NativeSparseSet</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSparseSet)
                {
                    var handle = (StackallocOrderedSparseSet<T>*)nativeSparseSet;
                    _nativeSparseSet = handle;
                    _version = handle->_version;
                    _index = -1;
                    _current = handle->_head != -1 ? &handle->_dense[handle->_head] : null;
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
                    if (num != 0)
                        _current = &handle->_dense[_current->Next];
                    return true;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public T Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current->Value;
                }
            }
        }

        /// <summary>
        ///     KeyValuePair collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct OrderedKeyValuePairCollection
        {
            /// <summary>
            ///     NativeSparseSet
            /// </summary>
            private readonly StackallocOrderedSparseSet<T>* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal OrderedKeyValuePairCollection(void* nativeSparseSet) => _nativeSparseSet = (StackallocOrderedSparseSet<T>*)nativeSparseSet;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSparseSet->_count;

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
                private readonly StackallocOrderedSparseSet<T>* _nativeSparseSet;

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
                private Entry* _current;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSparseSet">NativeSparseSet</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSparseSet)
                {
                    var handle = (StackallocOrderedSparseSet<T>*)nativeSparseSet;
                    _nativeSparseSet = handle;
                    _version = handle->_version;
                    _index = -1;
                    _current = handle->_head != -1 ? &handle->_dense[handle->_head] : null;
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
                    if (num != 0)
                        _current = &handle->_dense[_current->Next];
                    return true;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public KeyValuePair<int, T> Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => *(KeyValuePair<int, T>*)_current;
                }
            }
        }
    }
}