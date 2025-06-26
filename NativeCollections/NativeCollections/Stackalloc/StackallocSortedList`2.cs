using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Stackalloc sortedList
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocSortedList<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IComparable<TKey> where TValue : unmanaged
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
        ///     Get byte count
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Byte count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity)
        {
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<TKey>(), NativeMemoryAllocator.AlignOf<TValue>());
            var keysByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * sizeof(TKey)), alignment);
            return (int)(keysByteCount + capacity * sizeof(TValue) + alignment - 1);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocSortedList(Span<byte> buffer, int capacity)
        {
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<TKey>(), NativeMemoryAllocator.AlignOf<TValue>());
            var keysByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * sizeof(TKey)), alignment);
            _keys = (TKey*)NativeArray<byte>.Create(buffer, alignment).Buffer;
            _values = UnsafeHelpers.AddByteOffset<TValue>(_keys, (nint)keysByteCount);
            _size = 0;
            _version = 0;
            _capacity = capacity;
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
        ///     Index of
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in TKey key)
        {
            var num = BinarySearchHelpers.IndexOf(_keys, _size, key);
            return num >= 0 ? num : -1;
        }

        /// <summary>
        ///     Try add
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAdd(in TKey key, in TValue value)
        {
            var num = IndexOf(key);
            return num >= 0 ? InsertResult.AlreadyExists : Insert(~num, key, value);
        }

        /// <summary>
        ///     Try insert
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryInsert(in TKey key, in TValue value)
        {
            var num = IndexOf(key);
            if (num >= 0)
            {
                Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)num) = value;
                ++_version;
                return InsertResult.Overwritten;
            }

            return Insert(~num, key, value);
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                --_size;
                if (index < _size)
                {
                    Unsafe.CopyBlockUnaligned(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), (uint)((_size - index) * sizeof(TKey)));
                    Unsafe.CopyBlockUnaligned(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), (uint)((_size - index) * sizeof(TValue)));
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
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index);
                --_size;
                if (index < _size)
                {
                    Unsafe.CopyBlockUnaligned(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), (uint)((_size - index) * sizeof(TKey)));
                    Unsafe.CopyBlockUnaligned(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), (uint)((_size - index) * sizeof(TValue)));
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
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), (uint)((_size - index) * sizeof(TKey)));
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), (uint)((_size - index) * sizeof(TValue)));
            }

            ++_version;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">Key value pair</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            keyValuePair = new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index), Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index));
            --_size;
            if (index < _size)
            {
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), (uint)((_size - index) * sizeof(TKey)));
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), (uint)((_size - index) * sizeof(TValue)));
            }

            ++_version;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
                return false;
            --_size;
            if (index < _size)
            {
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), (uint)((_size - index) * sizeof(TKey)));
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), (uint)((_size - index) * sizeof(TValue)));
            }

            ++_version;
            return true;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">Key value pair</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
        {
            if ((uint)index >= (uint)_size)
            {
                keyValuePair = default;
                return false;
            }

            keyValuePair = new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index), Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index));
            --_size;
            if (index < _size)
            {
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), (uint)((_size - index) * sizeof(TKey)));
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), (uint)((_size - index) * sizeof(TValue)));
            }

            ++_version;
            return true;
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
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + count))), (uint)((_size - index) * sizeof(TKey)));
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + count))), (uint)((_size - index) * sizeof(TValue)));
            }

            ++_version;
        }

        /// <summary>
        ///     Get key at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TKey GetKeyAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index);
        }

        /// <summary>
        ///     Get value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index);
        }

        /// <summary>
        ///     Set value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValueAt(int index, in TValue value)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index) = value;
            ++_version;
        }

        /// <summary>
        ///     Contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Contains key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in TKey key) => IndexOf(key) >= 0;

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in TKey key, out TValue value)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index);
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
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = new NativeReference<TValue>(Unsafe.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Get key at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <returns>Key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetKeyAt(int index, out TKey key)
        {
            if ((uint)index >= (uint)_size)
            {
                key = default;
                return false;
            }

            key = Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index);
            return true;
        }

        /// <summary>
        ///     Get value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueAt(int index, out TValue value)
        {
            if ((uint)index >= (uint)_size)
            {
                value = default;
                return false;
            }

            value = Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index);
            return true;
        }

        /// <summary>
        ///     Get value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReferenceAt(int index, out NativeReference<TValue> value)
        {
            if ((uint)index >= (uint)_size)
            {
                value = default;
                return false;
            }

            value = new NativeReference<TValue>(Unsafe.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)));
            return true;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>KeyValuePair</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<TKey, TValue> GetAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index), Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index));
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>KeyValuePair</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<TKey, NativeReference<TValue>> GetReferenceAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return new KeyValuePair<TKey, NativeReference<TValue>>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index), new NativeReference<TValue>(Unsafe.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index))));
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
        {
            if ((uint)index >= (uint)_size)
            {
                keyValuePair = default;
                return false;
            }

            keyValuePair = new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index), Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index));
            return true;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetReferenceAt(int index, out KeyValuePair<TKey, NativeReference<TValue>> keyValuePair)
        {
            if ((uint)index >= (uint)_size)
            {
                keyValuePair = default;
                return false;
            }

            keyValuePair = new KeyValuePair<TKey, NativeReference<TValue>>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index), new NativeReference<TValue>(Unsafe.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index))));
            return true;
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private InsertResult Insert(int index, in TKey key, in TValue value)
        {
            if (_size == _capacity)
                return InsertResult.InsufficientCapacity;
            if (index < _size)
            {
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), (uint)((_size - index) * sizeof(TKey)));
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), (uint)((_size - index) * sizeof(TValue)));
            }

            Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index) = key;
            Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index) = value;
            ++_size;
            ++_version;
            return InsertResult.Success;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocSortedList<TKey, TValue> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     NativeSortedList
            /// </summary>
            private readonly StackallocSortedList<TKey, TValue>* _nativeSortedList;

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
                var handle = (StackallocSortedList<TKey, TValue>*)nativeSortedList;
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
                    _current = new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(handle->_keys), (nint)_index), Unsafe.Add(ref Unsafe.AsRef<TValue>(handle->_values), (nint)_index));
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
        public readonly struct KeyCollection : IReadOnlyCollection<TKey>
        {
            /// <summary>
            ///     NativeSortedList
            /// </summary>
            private readonly StackallocSortedList<TKey, TValue>* _nativeSortedList;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSortedList->Count;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedList">NativeSortedList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(void* nativeSortedList) => _nativeSortedList = (StackallocSortedList<TKey, TValue>*)nativeSortedList;

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TKey> AsReadOnlySpan()
            {
                var handle = _nativeSortedList;
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<TKey>(handle->_keys), handle->_size);
            }

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TKey> AsReadOnlySpan(int start)
            {
                var handle = _nativeSortedList;
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(handle->_keys), (nint)start), handle->_size - start);
            }

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TKey> AsReadOnlySpan(int start, int length)
            {
                var handle = _nativeSortedList;
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(handle->_keys), (nint)start), length);
            }

            /// <summary>
            ///     As span
            /// </summary>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnlySpan<TKey>(in KeyCollection keyCollection) => keyCollection.AsReadOnlySpan();

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSortedList);

            /// <summary>
            ///     Get enumerator
            /// </summary>
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

            /// <summary>
            ///     Get enumerator
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator
            {
                /// <summary>
                ///     NativeSortedList
                /// </summary>
                private readonly StackallocSortedList<TKey, TValue>* _nativeSortedList;

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
                    var handle = (StackallocSortedList<TKey, TValue>*)nativeSortedList;
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
                        _current = Unsafe.Add(ref Unsafe.AsRef<TKey>(handle->_keys), (nint)_index);
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
        public readonly struct ValueCollection : IReadOnlyCollection<TValue>
        {
            /// <summary>
            ///     NativeSortedList
            /// </summary>
            private readonly StackallocSortedList<TKey, TValue>* _nativeSortedList;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSortedList->Count;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedList">NativeSortedList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(void* nativeSortedList) => _nativeSortedList = (StackallocSortedList<TKey, TValue>*)nativeSortedList;

            /// <summary>
            ///     As span
            /// </summary>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<TValue> AsSpan()
            {
                var handle = _nativeSortedList;
                return MemoryMarshal.CreateSpan(ref Unsafe.AsRef<TValue>(handle->_values), handle->_size);
            }

            /// <summary>
            ///     As span
            /// </summary>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<TValue> AsSpan(int start)
            {
                var handle = _nativeSortedList;
                return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(handle->_values), (nint)start), handle->_size - start);
            }

            /// <summary>
            ///     As span
            /// </summary>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<TValue> AsSpan(int start, int length)
            {
                var handle = _nativeSortedList;
                return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(handle->_values), (nint)start), length);
            }

            /// <summary>
            ///     As span
            /// </summary>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Span<TValue>(in ValueCollection valueCollection) => valueCollection.AsSpan();

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSortedList);

            /// <summary>
            ///     Get enumerator
            /// </summary>
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

            /// <summary>
            ///     Get enumerator
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator
            {
                /// <summary>
                ///     NativeSortedList
                /// </summary>
                private readonly StackallocSortedList<TKey, TValue>* _nativeSortedList;

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
                    var handle = (StackallocSortedList<TKey, TValue>*)nativeSortedList;
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
                        _current = Unsafe.Add(ref Unsafe.AsRef<TValue>(handle->_values), (nint)_index);
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