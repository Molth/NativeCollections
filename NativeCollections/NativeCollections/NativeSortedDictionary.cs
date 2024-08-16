using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY_2021_3_OR_NEWER || GODOT
using System;
using System.Collections.Generic;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable PossibleNullReferenceException
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Native.Collections
{
    /// <summary>
    ///     Native sortedDictionary
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct NativeSortedDictionary<TKey, TValue> : IDisposable, IEquatable<NativeSortedDictionary<TKey, TValue>> where TKey : unmanaged, IComparable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeSortedDictionaryHandle
        {
            /// <summary>
            ///     SortedSet
            /// </summary>
            public NativeSortedSet<SortedKeyValuePair> SortedSet;

            /// <summary>
            ///     Keys
            /// </summary>
            public KeyCollection Keys;

            /// <summary>
            ///     Values
            /// </summary>
            public ValueCollection Values;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeSortedDictionaryHandle* _handle;

        /// <summary>
        ///     Keys
        /// </summary>
        public KeyCollection Keys => _handle->Keys;

        /// <summary>
        ///     Values
        /// </summary>
        public ValueCollection Values => _handle->Values;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">MemoryPool size</param>
        /// <param name="maxFreeSlabs">MemoryPool maxFreeSlabs</param>
        public NativeSortedDictionary(int size, int maxFreeSlabs)
        {
            var sortedSet = new NativeSortedSet<SortedKeyValuePair>(size, maxFreeSlabs);
            _handle = (NativeSortedDictionaryHandle*)NativeMemoryAllocator.Alloc(sizeof(NativeSortedDictionaryHandle));
            _handle->SortedSet = sortedSet;
            _handle->Keys = new KeyCollection(this);
            _handle->Values = new ValueCollection(this);
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _handle->SortedSet.Count == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _handle->SortedSet.Count;

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="key">Key</param>
        public TValue this[in TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!_handle->SortedSet.TryGetValue(new SortedKeyValuePair(key, default), out var keyValuePair))
                    throw new KeyNotFoundException(key.ToString());
                return keyValuePair.Value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _handle->SortedSet.Add(new SortedKeyValuePair(key, default), new SortedKeyValuePair(key, value));
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeSortedDictionary<TKey, TValue> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeSortedDictionary<TKey, TValue> nativeSortedDictionary && nativeSortedDictionary == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => (int)(nint)_handle;

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeSortedDictionary<{typeof(TKey).Name}, {typeof(TValue).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeSortedDictionary<TKey, TValue> left, NativeSortedDictionary<TKey, TValue> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeSortedDictionary<TKey, TValue> left, NativeSortedDictionary<TKey, TValue> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_handle == null)
                return;
            _handle->SortedSet.Dispose();
            NativeMemoryAllocator.Free(_handle);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->SortedSet.Clear();

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in TKey key, in TValue value) => _handle->SortedSet.Add(new SortedKeyValuePair(key, value));

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key) => _handle->SortedSet.Remove(new SortedKeyValuePair(key, default));

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key, out TValue value)
        {
            if (_handle->SortedSet.Remove(new SortedKeyValuePair(key, default), out var keyValuePair))
            {
                value = keyValuePair.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Contains key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in TKey key) => _handle->SortedSet.Contains(new SortedKeyValuePair(key, default));

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in TKey key, out TValue value)
        {
            if (!_handle->SortedSet.TryGetValue(new SortedKeyValuePair(key, default), out var keyValuePair))
            {
                value = default;
                return false;
            }

            value = keyValuePair.Value;
            return true;
        }

        /// <summary>
        ///     Key value pair
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct SortedKeyValuePair : IComparable<SortedKeyValuePair>
        {
            /// <summary>
            ///     Key
            /// </summary>
            private readonly TKey _key;

            /// <summary>
            ///     Value
            /// </summary>
            private readonly TValue _value;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SortedKeyValuePair(TKey key, TValue value)
            {
                _key = key;
                _value = value;
            }

            /// <summary>
            ///     Key
            /// </summary>
            public TKey Key
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _key;
            }

            /// <summary>
            ///     Value
            /// </summary>
            public TValue Value
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _value;
            }

            /// <summary>
            ///     Compare to
            /// </summary>
            /// <param name="other">Other</param>
            /// <returns>Compared</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(SortedKeyValuePair other) => _key.CompareTo(other._key);

            /// <summary>
            ///     As keyValuePair
            /// </summary>
            /// <param name="keyValuePair">KeyValuePair</param>
            /// <returns>KeyValuePair</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator KeyValuePair<TKey, TValue>(SortedKeyValuePair keyValuePair) => new(keyValuePair._key, keyValuePair._value);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeSortedDictionary<TKey, TValue> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(this);

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator : IDisposable
        {
            /// <summary>
            ///     Enumerator
            /// </summary>
            private NativeSortedSet<SortedKeyValuePair>.Enumerator _sortedSetEnumerator;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedDictionary">NativeSortedDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeSortedDictionary<TKey, TValue> nativeSortedDictionary) => _sortedSetEnumerator = nativeSortedDictionary._handle->SortedSet.GetEnumerator();

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => _sortedSetEnumerator.MoveNext();

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _sortedSetEnumerator.Dispose();

            /// <summary>
            ///     Current
            /// </summary>
            public KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _sortedSetEnumerator.Current;
            }
        }

        /// <summary>
        ///     Key collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct KeyCollection
        {
            /// <summary>
            ///     NativeSortedDictionary
            /// </summary>
            private readonly NativeSortedDictionary<TKey, TValue> _nativeSortedDictionary;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedDictionary">NativeSortedDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(NativeSortedDictionary<TKey, TValue> nativeSortedDictionary) => _nativeSortedDictionary = nativeSortedDictionary;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSortedDictionary);

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator : IDisposable
            {
                /// <summary>
                ///     Enumerator
                /// </summary>
                private NativeSortedSet<SortedKeyValuePair>.Enumerator _sortedSetEnumerator;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSortedDictionary">NativeSortedDictionary</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(NativeSortedDictionary<TKey, TValue> nativeSortedDictionary) => _sortedSetEnumerator = nativeSortedDictionary._handle->SortedSet.GetEnumerator();

                /// <summary>
                ///     Dispose
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose() => _sortedSetEnumerator.Dispose();

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext() => _sortedSetEnumerator.MoveNext();

                /// <summary>
                ///     Current
                /// </summary>
                public TKey Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _sortedSetEnumerator.Current.Key;
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
            ///     NativeSortedDictionary
            /// </summary>
            private readonly NativeSortedDictionary<TKey, TValue> _nativeSortedDictionary;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedDictionary">NativeSortedDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(NativeSortedDictionary<TKey, TValue> nativeSortedDictionary) => _nativeSortedDictionary = nativeSortedDictionary;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSortedDictionary);

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator : IDisposable
            {
                /// <summary>
                ///     Enumerator
                /// </summary>
                private NativeSortedSet<SortedKeyValuePair>.Enumerator _sortedSetEnumerator;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSortedDictionary">NativeSortedDictionary</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(NativeSortedDictionary<TKey, TValue> nativeSortedDictionary) => _sortedSetEnumerator = nativeSortedDictionary._handle->SortedSet.GetEnumerator();

                /// <summary>
                ///     Dispose
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose() => _sortedSetEnumerator.Dispose();

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext() => _sortedSetEnumerator.MoveNext();

                /// <summary>
                ///     Current
                /// </summary>
                public TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _sortedSetEnumerator.Current.Value;
                }
            }
        }
    }
}