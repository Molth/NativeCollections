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
    ///     Native sortedList
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly unsafe struct NativeSortedList<TKey, TValue> where TKey : unmanaged, IComparable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeSortedListHandle
        {
            /// <summary>
            ///     Keys
            /// </summary>
            public TKey* Keys;

            /// <summary>
            ///     Values
            /// </summary>
            public TValue* Values;

            /// <summary>
            ///     Size
            /// </summary>
            public int Size;

            /// <summary>
            ///     Version
            /// </summary>
            public int Version;

            /// <summary>
            ///     Capacity
            /// </summary>
            public int Capacity;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeSortedListHandle* _handle;

        /// <summary>
        ///     Keys
        /// </summary>
        public KeyCollection Keys => new(this);

        /// <summary>
        ///     Values
        /// </summary>
        public ValueCollection Values => new(this);

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSortedList(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            var handle = (NativeSortedListHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeSortedListHandle));
            handle->Keys = (TKey*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(TKey)));
            handle->Values = (TValue*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(TValue)));
            handle->Size = 0;
            handle->Version = 0;
            handle->Capacity = capacity;
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
        ///     Get or set value
        /// </summary>
        /// <param name="key">Key</param>
        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var handle = _handle;
                var index = BinarySearch(handle->Keys, handle->Size, key);
                return index >= 0 ? handle->Values[index] : throw new KeyNotFoundException(key.ToString());
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var handle = _handle;
                var index = BinarySearch(handle->Keys, handle->Size, key);
                if (index >= 0)
                {
                    handle->Values[index] = value;
                    ++handle->Version;
                }
                else
                {
                    Insert(~index, key, value);
                }
            }
        }

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _handle->Size;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var handle = _handle;
                if (value < handle->Size)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "SmallCapacity");
                if (value != handle->Capacity)
                {
                    if (value > 0)
                    {
                        var keys = (TKey*)NativeMemoryAllocator.Alloc((uint)(value * sizeof(TKey)));
                        var values = (TValue*)NativeMemoryAllocator.Alloc((uint)(value * sizeof(TValue)));
                        if (handle->Size > 0)
                        {
                            Unsafe.CopyBlockUnaligned(keys, handle->Keys, (uint)(handle->Size * sizeof(TKey)));
                            Unsafe.CopyBlockUnaligned(values, handle->Values, (uint)(handle->Size * sizeof(TValue)));
                        }

                        NativeMemoryAllocator.Free(handle->Keys);
                        NativeMemoryAllocator.Free(handle->Values);
                        handle->Keys = keys;
                        handle->Values = values;
                    }
                    else
                    {
                        NativeMemoryAllocator.Free(handle->Keys);
                        NativeMemoryAllocator.Free(handle->Values);
                        handle->Keys = (TKey*)NativeMemoryAllocator.Alloc(0);
                        handle->Values = (TValue*)NativeMemoryAllocator.Alloc(0);
                    }

                    handle->Capacity = value;
                }
            }
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeSortedList<TKey, TValue> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeSortedList<TKey, TValue> nativeSortedList && nativeSortedList == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeSortedList<{typeof(TKey).Name}, {typeof(TValue).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeSortedList<TKey, TValue> left, NativeSortedList<TKey, TValue> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeSortedList<TKey, TValue> left, NativeSortedList<TKey, TValue> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            NativeMemoryAllocator.Free(handle->Keys);
            NativeMemoryAllocator.Free(handle->Values);
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var handle = _handle;
            ++handle->Version;
            handle->Size = 0;
        }

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in TKey key, in TValue value)
        {
            var handle = _handle;
            var num = BinarySearch(handle->Keys, handle->Size, key);
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
            var handle = _handle;
            var index = BinarySearch(handle->Keys, handle->Size, key);
            if (index >= 0)
            {
                --handle->Size;
                if (index < handle->Size)
                {
                    Unsafe.CopyBlockUnaligned(handle->Keys + index, handle->Keys + index + 1, (uint)((handle->Size - index) * sizeof(TKey)));
                    Unsafe.CopyBlockUnaligned(handle->Values + index, handle->Values + index + 1, (uint)((handle->Size - index) * sizeof(TValue)));
                }

                ++handle->Version;
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
            var handle = _handle;
            var index = BinarySearch(handle->Keys, handle->Size, key);
            if (index >= 0)
            {
                value = handle->Values[index];
                --handle->Size;
                if (index < handle->Size)
                {
                    Unsafe.CopyBlockUnaligned(handle->Keys + index, handle->Keys + index + 1, (uint)((handle->Size - index) * sizeof(TKey)));
                    Unsafe.CopyBlockUnaligned(handle->Values + index, handle->Values + index + 1, (uint)((handle->Size - index) * sizeof(TValue)));
                }

                ++handle->Version;
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
            var handle = _handle;
            if (index >= handle->Size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            --handle->Size;
            if (index < handle->Size)
            {
                Unsafe.CopyBlockUnaligned(handle->Keys + index, handle->Keys + index + 1, (uint)((handle->Size - index) * sizeof(TKey)));
                Unsafe.CopyBlockUnaligned(handle->Values + index, handle->Values + index + 1, (uint)((handle->Size - index) * sizeof(TValue)));
            }

            ++handle->Version;
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
            var handle = _handle;
            if (index >= handle->Size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return handle->Keys[index];
        }

        /// <summary>
        ///     Get value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetValueAtIndex(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            var handle = _handle;
            if (index >= handle->Size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return handle->Values[index];
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
            var handle = _handle;
            if (index >= handle->Size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            handle->Values[index] = value;
            ++handle->Version;
        }

        /// <summary>
        ///     Contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Contains key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in TKey key)
        {
            var handle = _handle;
            return BinarySearch(handle->Keys, handle->Size, key) >= 0;
        }

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in TKey key, out TValue value)
        {
            var handle = _handle;
            var index = BinarySearch(handle->Keys, handle->Size, key);
            if (index >= 0)
            {
                value = handle->Values[index];
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
            var handle = _handle;
            var index = BinarySearch(handle->Keys, handle->Size, key);
            if (index >= 0)
            {
                value = new NativeReference<TValue>(Unsafe.AsPointer(ref handle->Values[index]));
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
            var handle = _handle;
            if (handle->Capacity < capacity)
            {
                var newCapacity = 2 * handle->Capacity;
                if ((uint)newCapacity > 2147483591)
                    newCapacity = 2147483591;
                var expected = handle->Capacity + 4;
                newCapacity = newCapacity > expected ? newCapacity : expected;
                if (newCapacity < capacity)
                    newCapacity = capacity;
                Capacity = newCapacity;
            }

            return handle->Capacity;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var handle = _handle;
            var threshold = (int)(handle->Capacity * 0.9);
            if (handle->Size < threshold)
                Capacity = handle->Size;
            return handle->Capacity;
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
            var handle = _handle;
            if (handle->Size == handle->Capacity)
                EnsureCapacity(handle->Size + 1);
            if (index < handle->Size)
            {
                Unsafe.CopyBlockUnaligned(handle->Keys + index + 1, handle->Keys + index, (uint)((handle->Size - index) * sizeof(TKey)));
                Unsafe.CopyBlockUnaligned(handle->Values + index + 1, handle->Values + index, (uint)((handle->Size - index) * sizeof(TValue)));
            }

            handle->Keys[index] = key;
            handle->Values[index] = value;
            ++handle->Size;
            ++handle->Version;
        }

        /// <summary>
        ///     Binary search
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <param name="comparable">Comparable</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int BinarySearch(TKey* start, int length, in TKey comparable)
        {
            var low = 0;
            var high = length - 1;
            while (low <= high)
            {
                var i = (int)(((uint)high + (uint)low) >> 1);
                var c = comparable.CompareTo(*(start + i));
                if (c == 0)
                    return i;
                if (c > 0)
                    low = i + 1;
                else
                    high = i - 1;
            }

            return ~low;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeSortedList<TKey, TValue> Empty => new();

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
            ///     NativeSortedList
            /// </summary>
            private readonly NativeSortedList<TKey, TValue> _nativeSortedList;

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
            internal Enumerator(NativeSortedList<TKey, TValue> nativeSortedList)
            {
                _nativeSortedList = nativeSortedList;
                _current = default;
                _index = 0;
                _version = _nativeSortedList._handle->Version;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _nativeSortedList._handle;
                if (_version != handle->Version)
                    throw new InvalidOperationException("EnumFailedVersion");
                if ((uint)_index < (uint)handle->Size)
                {
                    _current = new KeyValuePair<TKey, TValue>(handle->Keys[_index], handle->Values[_index]);
                    ++_index;
                    return true;
                }

                _index = handle->Size + 1;
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
            private readonly NativeSortedList<TKey, TValue> _nativeSortedList;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedList">NativeSortedList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(NativeSortedList<TKey, TValue> nativeSortedList) => _nativeSortedList = nativeSortedList;

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TKey> AsReadOnlySpan()
            {
                var handle = _nativeSortedList._handle;
                return MemoryMarshal.CreateReadOnlySpan(ref *handle->Keys, handle->Size);
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
                private readonly NativeSortedList<TKey, TValue> _nativeSortedList;

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
                internal Enumerator(NativeSortedList<TKey, TValue> nativeSortedList)
                {
                    _nativeSortedList = nativeSortedList;
                    _current = default;
                    _index = 0;
                    _version = _nativeSortedList._handle->Version;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativeSortedList._handle;
                    if (_version != handle->Version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    if ((uint)_index < (uint)handle->Size)
                    {
                        _current = handle->Keys[_index];
                        ++_index;
                        return true;
                    }

                    _index = handle->Size + 1;
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
            private readonly NativeSortedList<TKey, TValue> _nativeSortedList;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedList">NativeSortedList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(NativeSortedList<TKey, TValue> nativeSortedList) => _nativeSortedList = nativeSortedList;

            /// <summary>
            ///     As span
            /// </summary>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<TValue> AsSpan()
            {
                var handle = _nativeSortedList._handle;
                return MemoryMarshal.CreateSpan(ref *handle->Values, handle->Size);
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
                private readonly NativeSortedList<TKey, TValue> _nativeSortedList;

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
                internal Enumerator(NativeSortedList<TKey, TValue> nativeSortedList)
                {
                    _nativeSortedList = nativeSortedList;
                    _current = default;
                    _index = 0;
                    _version = _nativeSortedList._handle->Version;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativeSortedList._handle;
                    if (_version != handle->Version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    if ((uint)_index < (uint)handle->Size)
                    {
                        _current = handle->Values[_index];
                        ++_index;
                        return true;
                    }

                    _index = handle->Size + 1;
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