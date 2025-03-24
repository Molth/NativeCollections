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
    ///     Native sparseSet
    ///     //https://github.com/bombela/sparseset
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.Community | NativeCollectionType.Rust)]
    public readonly unsafe struct NativeSparseSet<T> : IDisposable, IEquatable<NativeSparseSet<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeSparseSetHandle
        {
            /// <summary>
            ///     Dense
            /// </summary>
            public Entry* Dense;

            /// <summary>
            ///     Sparse
            /// </summary>
            public int* Sparse;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count;

            /// <summary>
            ///     Version
            /// </summary>
            public int Version;

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
                set => Insert(key, in value);
            }

            /// <summary>
            ///     Clear
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                MemoryMarshal.CreateSpan(ref *Sparse, Length).Fill(-1);
                Count = 0;
                ++Version;
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
                if (key > Length)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
                var index = Sparse[key];
                if (index != -1)
                    return false;
                ref var count = ref Count;
                ref var entry = ref Dense[count];
                entry.Key = key;
                entry.Value = value;
                Sparse[key] = count;
                ++count;
                ++Version;
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
                if (key > Length)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
                var index = Sparse[key];
                if (index != -1)
                {
                    Dense[index].Value = value;
                    ++Version;
                    return false;
                }

                ref var count = ref Count;
                ref var entry = ref Dense[count];
                entry.Key = key;
                entry.Value = value;
                Sparse[key] = count;
                ++count;
                ++Version;
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
                if (key < 0)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
                if (key > Length)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
                var index = Sparse[key];
                if (index == -1)
                    return false;
                --Count;
                if (index != Count)
                {
                    ref var lastEntry = ref Dense[Count];
                    Dense[index] = lastEntry;
                    Sparse[lastEntry.Key] = index;
                }

                Sparse[key] = -1;
                ++Version;
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
                if (key < 0)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
                if (key > Length)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
                var index = Sparse[key];
                if (index == -1)
                {
                    value = default;
                    return false;
                }

                ref var entry = ref Dense[index];
                value = entry.Value;
                --Count;
                if (index != Count)
                {
                    ref var lastEntry = ref Dense[Count];
                    entry = lastEntry;
                    Sparse[lastEntry.Key] = index;
                }

                Sparse[key] = -1;
                ++Version;
                return true;
            }

            /// <summary>
            ///     Contains key
            /// </summary>
            /// <param name="key">Key</param>
            /// <returns>Contains key</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ContainsKey(int key)
            {
                if (key < 0)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
                if (key > Length)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
                return Sparse[key] != -1;
            }

            /// <summary>
            ///     Try to get the value
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            /// <returns>Got</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(int key, out T value)
            {
                if (key < 0)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
                if (key > Length)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
                var index = Sparse[key];
                if (index != -1)
                {
                    value = Dense[index].Value;
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
                if (key < 0)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
                if (key > Length)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
                var index = Sparse[key];
                if (index != -1)
                {
                    ref var entry = ref Dense[index];
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
            public int IndexOf(int key)
            {
                if (key < 0)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
                if (key > Length)
                    throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
                return Sparse[key];
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
                if (index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
                return *(KeyValuePair<int, T>*)&Dense[index];
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
                if (index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
                ref var entry = ref Dense[index];
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
                if (index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
                Dense[index].Value = value;
                ++Version;
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
                if (index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
                ref var entry = ref Dense[index];
                var key = entry.Key;
                --Count;
                if (index != Count)
                {
                    ref var lastEntry = ref Dense[Count];
                    entry = lastEntry;
                    Sparse[lastEntry.Key] = index;
                }

                Sparse[key] = -1;
                ++Version;
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
                if (index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
                ref var entry = ref Dense[index];
                var key = entry.Key;
                keyValuePair = *(KeyValuePair<int, T>*)Unsafe.AsPointer(ref entry);
                --Count;
                if (index != Count)
                {
                    ref var lastEntry = ref Dense[Count];
                    entry = lastEntry;
                    Sparse[lastEntry.Key] = index;
                }

                Sparse[key] = -1;
                ++Version;
            }

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref *(KeyValuePair<int, T>*)Dense, Count);

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <param name="start">Start</param>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref *(KeyValuePair<int, T>*)(Dense + start), Count - start);

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <param name="start">Start</param>
            /// <param name="length">Length</param>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref *(KeyValuePair<int, T>*)(Dense + start), length);
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeSparseSetHandle* _handle;

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
        public NativeSparseSet(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            var handle = (NativeSparseSetHandle*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeSparseSetHandle) + capacity * (sizeof(Entry) + sizeof(int))));
            handle->Dense = (Entry*)((byte*)handle + sizeof(NativeSparseSetHandle));
            handle->Sparse = (int*)((byte*)handle->Dense + capacity * sizeof(Entry));
            MemoryMarshal.CreateSpan(ref *handle->Sparse, capacity).Fill(-1);
            handle->Length = capacity;
            handle->Count = 0;
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
        public bool IsEmpty => _handle->Count == 0;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length => _handle->Length;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _handle->Count;

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="key">Key</param>
        public T this[int key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (*_handle)[key];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => (*_handle)[key] = value;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeSparseSet<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeSparseSet<T> nativeSparseSet && nativeSparseSet == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeSparseSet<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeSparseSet<T> left, NativeSparseSet<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeSparseSet<T> left, NativeSparseSet<T> right) => left._handle != right._handle;

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<KeyValuePair<int, T>>(NativeSparseSet<T> nativeSparseSet) => nativeSparseSet.AsReadOnlySpan();

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
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
        public bool Add(int key, in T value) => _handle->Add(key, value);

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
        public bool Insert(int key, in T value) => _handle->Insert(key, value);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key) => _handle->Remove(key);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key, out T value) => _handle->Remove(key, out value);

        /// <summary>
        ///     Contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Contains key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(int key) => _handle->ContainsKey(key);

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(int key, out T value) => _handle->TryGetValue(key, out value);

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReference(int key, out NativeReference<T> value) => _handle->TryGetValueReference(key, out value);

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(int key) => _handle->IndexOf(key);

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>KeyValuePair</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<int, T> GetAt(int index) => _handle->GetAt(index);

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>KeyValuePair</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<int, NativeReference<T>> GetReferenceAt(int index) => _handle->GetReferenceAt(index);

        /// <summary>
        ///     Set at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index, in T value) => _handle->SetAt(index, value);

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index) => _handle->RemoveAt(index);

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out KeyValuePair<int, T> keyValuePair) => _handle->RemoveAt(index, out keyValuePair);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan() => _handle->AsReadOnlySpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan(int start) => _handle->AsReadOnlySpan(start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan(int start, int length) => _handle->AsReadOnlySpan(start, length);

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
        public static NativeSparseSet<T> Empty => new();

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
            ///     NativeSparseSet
            /// </summary>
            private readonly NativeSparseSetHandle* _nativeSparseSet;

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
                var handle = (NativeSparseSetHandle*)nativeSparseSet;
                _nativeSparseSet = handle;
                _version = handle->Version;
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
                if (_version != handle->Version)
                    throw new InvalidOperationException("EnumFailedVersion");
                var num = _index + 1;
                if (num >= handle->Count)
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
                get => *(KeyValuePair<int, T>*)(&_nativeSparseSet->Dense[_index]);
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
            private readonly NativeSparseSetHandle* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(void* nativeSparseSet) => _nativeSparseSet = (NativeSparseSetHandle*)nativeSparseSet;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSparseSet->Count;

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
                    if (index >= handle->Count)
                        throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
                    return handle->Dense[index].Key;
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
                private readonly NativeSparseSetHandle* _nativeSparseSet;

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
                    var handle = (NativeSparseSetHandle*)nativeSparseSet;
                    _nativeSparseSet = handle;
                    _version = handle->Version;
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
                    if (_version != handle->Version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    var num = _index + 1;
                    if (num >= handle->Count)
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
                    get => _nativeSparseSet->Dense[_index].Key;
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
            private readonly NativeSparseSetHandle* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(void* nativeSparseSet) => _nativeSparseSet = (NativeSparseSetHandle*)nativeSparseSet;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSparseSet->Count;

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
                    if (index >= handle->Count)
                        throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
                    return ref handle->Dense[index].Value;
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
                private readonly NativeSparseSetHandle* _nativeSparseSet;

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
                    var handle = (NativeSparseSetHandle*)nativeSparseSet;
                    _nativeSparseSet = handle;
                    _version = handle->Version;
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
                    if (_version != handle->Version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    var num = _index + 1;
                    if (num >= handle->Count)
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
                    get => _nativeSparseSet->Dense[_index].Value;
                }
            }
        }
    }
}