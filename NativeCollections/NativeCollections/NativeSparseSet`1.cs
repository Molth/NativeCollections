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
    [NativeCollection]
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
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeSparseSetHandle* _handle;

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
        public void Clear()
        {
            var handle = _handle;
            MemoryMarshal.CreateSpan(ref *handle->Sparse, handle->Length).Fill(-1);
            handle->Count = 0;
            ++handle->Version;
        }

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(int key, in T value)
        {
            var handle = _handle;
            if (key < 0)
                throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
            if (key > handle->Length)
                throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
            var index = handle->Sparse[key];
            if (index != -1)
                return false;
            ref var count = ref handle->Count;
            ref var entry = ref handle->Dense[count];
            entry.Key = key;
            entry.Value = value;
            handle->Sparse[key] = count;
            ++count;
            ++handle->Version;
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
            var handle = _handle;
            if (key < 0)
                throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
            if (key > handle->Length)
                throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
            var index = handle->Sparse[key];
            if (index != -1)
            {
                handle->Dense[index].Value = value;
                ++handle->Version;
                return false;
            }

            ref var count = ref handle->Count;
            ref var entry = ref handle->Dense[count];
            entry.Key = key;
            entry.Value = value;
            handle->Sparse[key] = count;
            ++count;
            ++handle->Version;
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
            var handle = _handle;
            if (key < 0)
                throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
            if (key > handle->Length)
                throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
            var index = handle->Sparse[key];
            if (index == -1)
                return false;
            --handle->Count;
            if (index != handle->Count)
            {
                ref var lastEntry = ref handle->Dense[handle->Count];
                handle->Dense[index] = lastEntry;
                handle->Sparse[lastEntry.Key] = index;
            }

            handle->Sparse[key] = -1;
            ++handle->Version;
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
            var handle = _handle;
            if (key < 0)
                throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
            if (key > handle->Length)
                throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
            var index = handle->Sparse[key];
            if (index == -1)
            {
                value = default;
                return false;
            }

            ref var entry = ref handle->Dense[index];
            value = entry.Value;
            --handle->Count;
            if (index != handle->Count)
            {
                ref var lastEntry = ref handle->Dense[handle->Count];
                entry = lastEntry;
                handle->Sparse[lastEntry.Key] = index;
            }

            handle->Sparse[key] = -1;
            ++handle->Version;
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
            var handle = _handle;
            if (key < 0)
                throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
            if (key > handle->Length)
                throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
            return handle->Sparse[key] != -1;
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
            var handle = _handle;
            if (key < 0)
                throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
            if (key > handle->Length)
                throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
            var index = handle->Sparse[key];
            if (index != -1)
            {
                value = handle->Dense[index].Value;
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
            var handle = _handle;
            if (key < 0)
                throw new ArgumentOutOfRangeException(nameof(key), key, "MustBeNonNegative");
            if (key > handle->Length)
                throw new ArgumentOutOfRangeException(nameof(key), key, "IndexMustBeLessOrEqual");
            var index = handle->Sparse[key];
            if (index != -1)
            {
                ref var entry = ref handle->Dense[index];
                value = new NativeReference<T>(Unsafe.AsPointer(ref entry.Value));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>KeyValuePair</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<int, T> GetAt(int index)
        {
            var handle = _handle;
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= handle->Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            return *(KeyValuePair<int, T>*)&handle->Dense[index];
        }

        /// <summary>
        ///     Set at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index, in T value)
        {
            var handle = _handle;
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= handle->Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            handle->Dense[index].Value = value;
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan()
        {
            var handle = _handle;
            return MemoryMarshal.CreateReadOnlySpan(ref *(KeyValuePair<int, T>*)handle->Dense, handle->Count);
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan(int start)
        {
            var handle = _handle;
            return MemoryMarshal.CreateReadOnlySpan(ref *(KeyValuePair<int, T>*)(handle->Dense + start), handle->Count - start);
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<KeyValuePair<int, T>> AsReadOnlySpan(int start, int length)
        {
            var handle = _handle;
            return MemoryMarshal.CreateReadOnlySpan(ref *(KeyValuePair<int, T>*)(handle->Dense + start), length);
        }

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
        public Enumerator GetEnumerator() => new(this);

        /// <summary>
        ///     Enumerator
        /// </summary>
        public ref struct Enumerator
        {
            /// <summary>
            ///     NativeSparseSet
            /// </summary>
            private readonly NativeSparseSet<T> _nativeSparseSet;

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
            internal Enumerator(NativeSparseSet<T> nativeSparseSet)
            {
                _nativeSparseSet = nativeSparseSet;
                _version = nativeSparseSet._handle->Version;
                _index = -1;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _nativeSparseSet._handle;
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
                get => *(KeyValuePair<int, T>*)(&_nativeSparseSet._handle->Dense[_index]);
            }
        }
    }
}