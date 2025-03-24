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
    [NativeCollection(NativeCollectionType.Standard)]
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

            /// <summary>
            ///     Get or set value
            /// </summary>
            /// <param name="key">Key</param>
            public TValue this[TKey key]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var index = BinarySearchHelpers.Find(Keys, Size, key);
                    return index >= 0 ? Values[index] : throw new KeyNotFoundException(key.ToString());
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    var index = BinarySearchHelpers.Find(Keys, Size, key);
                    if (index >= 0)
                    {
                        Values[index] = value;
                        ++Version;
                    }
                    else
                    {
                        Insert(~index, key, value);
                    }
                }
            }

            /// <summary>
            ///     Clear
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                ++Version;
                Size = 0;
            }

            /// <summary>
            ///     Add
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(in TKey key, in TValue value)
            {
                var num = BinarySearchHelpers.Find(Keys, Size, key);
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
                var index = BinarySearchHelpers.Find(Keys, Size, key);
                if (index >= 0)
                {
                    --Size;
                    if (index < Size)
                    {
                        Unsafe.CopyBlockUnaligned(Keys + index, Keys + index + 1, (uint)((Size - index) * sizeof(TKey)));
                        Unsafe.CopyBlockUnaligned(Values + index, Values + index + 1, (uint)((Size - index) * sizeof(TValue)));
                    }

                    ++Version;
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
                var index = BinarySearchHelpers.Find(Keys, Size, key);
                if (index >= 0)
                {
                    value = Values[index];
                    --Size;
                    if (index < Size)
                    {
                        Unsafe.CopyBlockUnaligned(Keys + index, Keys + index + 1, (uint)((Size - index) * sizeof(TKey)));
                        Unsafe.CopyBlockUnaligned(Values + index, Values + index + 1, (uint)((Size - index) * sizeof(TValue)));
                    }

                    ++Version;
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
                if (index >= Size)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
                --Size;
                if (index < Size)
                {
                    Unsafe.CopyBlockUnaligned(Keys + index, Keys + index + 1, (uint)((Size - index) * sizeof(TKey)));
                    Unsafe.CopyBlockUnaligned(Values + index, Values + index + 1, (uint)((Size - index) * sizeof(TValue)));
                }

                ++Version;
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
                if (index + count > Size)
                    throw new ArgumentOutOfRangeException(nameof(count), count, "MustBeLess");
                Size -= count;
                if (index < Size)
                {
                    Unsafe.CopyBlockUnaligned(Keys + index, Keys + index + count, (uint)((Size - index) * sizeof(TKey)));
                    Unsafe.CopyBlockUnaligned(Values + index, Values + index + count, (uint)((Size - index) * sizeof(TValue)));
                }

                ++Version;
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
                if (index >= Size)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
                return Keys[index];
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
                if (index >= Size)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
                return Values[index];
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
                if (index >= Size)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
                Values[index] = value;
                ++Version;
            }

            /// <summary>
            ///     Contains key
            /// </summary>
            /// <param name="key">Key</param>
            /// <returns>Contains key</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ContainsKey(in TKey key) => BinarySearchHelpers.Find(Keys, Size, key) >= 0;

            /// <summary>
            ///     Try to get the value
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            /// <returns>Got</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(in TKey key, out TValue value)
            {
                var index = BinarySearchHelpers.Find(Keys, Size, key);
                if (index >= 0)
                {
                    value = Values[index];
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
                var index = BinarySearchHelpers.Find(Keys, Size, key);
                if (index >= 0)
                {
                    value = new NativeReference<TValue>(Unsafe.AsPointer(ref Values[index]));
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
                if (Capacity < capacity)
                {
                    var newCapacity = 2 * Capacity;
                    if ((uint)newCapacity > 2147483591)
                        newCapacity = 2147483591;
                    var expected = Capacity + 4;
                    newCapacity = newCapacity > expected ? newCapacity : expected;
                    if (newCapacity < capacity)
                        newCapacity = capacity;
                    SetCapacity(newCapacity);
                }

                return Capacity;
            }

            /// <summary>
            ///     Trim excess
            /// </summary>
            /// <returns>New capacity</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int TrimExcess()
            {
                var threshold = (int)(Capacity * 0.9);
                if (Size < threshold)
                    SetCapacity(Size);
                return Capacity;
            }

            /// <summary>
            ///     Set capacity
            /// </summary>
            /// <param name="capacity">Capacity</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetCapacity(int capacity)
            {
                if (capacity < Size)
                    throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Small_Capacity");
                if (capacity != Capacity)
                {
                    if (capacity > 0)
                    {
                        var keys = (TKey*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(TKey)));
                        var values = (TValue*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(TValue)));
                        if (Size > 0)
                        {
                            Unsafe.CopyBlockUnaligned(keys, Keys, (uint)(Size * sizeof(TKey)));
                            Unsafe.CopyBlockUnaligned(values, Values, (uint)(Size * sizeof(TValue)));
                        }

                        NativeMemoryAllocator.Free(Keys);
                        NativeMemoryAllocator.Free(Values);
                        Keys = keys;
                        Values = values;
                    }
                    else
                    {
                        NativeMemoryAllocator.Free(Keys);
                        NativeMemoryAllocator.Free(Values);
                        Keys = (TKey*)NativeMemoryAllocator.Alloc(0);
                        Values = (TValue*)NativeMemoryAllocator.Alloc(0);
                    }

                    Capacity = capacity;
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
                if (Size == Capacity)
                    EnsureCapacity(Size + 1);
                if (index < Size)
                {
                    Unsafe.CopyBlockUnaligned(Keys + index + 1, Keys + index, (uint)((Size - index) * sizeof(TKey)));
                    Unsafe.CopyBlockUnaligned(Values + index + 1, Values + index, (uint)((Size - index) * sizeof(TValue)));
                }

                Keys[index] = key;
                Values[index] = value;
                ++Size;
                ++Version;
            }
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeSortedListHandle* _handle;

        /// <summary>
        ///     Keys
        /// </summary>
        public KeyCollection Keys => new(_handle);

        /// <summary>
        ///     Values
        /// </summary>
        public ValueCollection Values => new(_handle);

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
            get => (*_handle)[key];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => (*_handle)[key] = value;
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
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in TKey key, in TValue value) => _handle->Add(key, value);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key) => _handle->Remove(key);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key, out TValue value) => _handle->Remove(key, out value);

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index) => _handle->RemoveAt(index);

        /// <summary>
        ///     Remove range
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRange(int index, int count) => _handle->RemoveRange(index, count);

        /// <summary>
        ///     Get key at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TKey GetKeyAtIndex(int index) => _handle->GetKeyAtIndex(index);

        /// <summary>
        ///     Get value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetValueAtIndex(int index) => _handle->GetValueAtIndex(index);

        /// <summary>
        ///     Set value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValueAtIndex(int index, in TValue value) => _handle->SetValueAtIndex(index, value);

        /// <summary>
        ///     Contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Contains key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in TKey key) => _handle->ContainsKey(key);

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in TKey key, out TValue value) => _handle->TryGetValue(key, out value);

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReference(in TKey key, out NativeReference<TValue> value) => _handle->TryGetValueReference(key, out value);

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
        ///     Set capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity) => _handle->SetCapacity(capacity);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeSortedList<TKey, TValue> Empty => new();

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
            ///     NativeSortedList
            /// </summary>
            private readonly NativeSortedListHandle* _nativeSortedList;

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
                var handle = (NativeSortedListHandle*)nativeSortedList;
                _nativeSortedList = handle;
                _current = default;
                _index = 0;
                _version = handle->Version;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _nativeSortedList;
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
            private readonly NativeSortedListHandle* _nativeSortedList;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedList">NativeSortedList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(void* nativeSortedList) => _nativeSortedList = (NativeSortedListHandle*)nativeSortedList;

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TKey> AsReadOnlySpan()
            {
                var handle = _nativeSortedList;
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
                private readonly NativeSortedListHandle* _nativeSortedList;

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
                    var handle = (NativeSortedListHandle*)nativeSortedList;
                    _nativeSortedList = handle;
                    _current = default;
                    _index = 0;
                    _version = handle->Version;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativeSortedList;
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
            private readonly NativeSortedListHandle* _nativeSortedList;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedList">NativeSortedList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(void* nativeSortedList) => _nativeSortedList = (NativeSortedListHandle*)nativeSortedList;

            /// <summary>
            ///     As span
            /// </summary>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<TValue> AsSpan()
            {
                var handle = _nativeSortedList;
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
                private readonly NativeSortedListHandle* _nativeSortedList;

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
                    var handle = (NativeSortedListHandle*)nativeSortedList;
                    _nativeSortedList = handle;
                    _current = default;
                    _index = 0;
                    _version = handle->Version;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativeSortedList;
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