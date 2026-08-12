using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.PaddingHelpers;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a thread-safe collection of items.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community)]
    public readonly struct UnsafeShardedHashSet<T> : IIsCreated, IDisposable, IEquatable<UnsafeShardedHashSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Array of shards, each containing a separate hash set and its own reader‑writer lock.
        /// </summary>
        /// <remarks>
        ///     Segment design is inspired by the algorithm outlined at:
        ///     https://github.com/xacrimon/dashmap
        /// </remarks>
        private readonly NativeArray<Shard> _shards;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _shards.IsCreated;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => CheckIsEmpty(_shards);

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <exception cref="OverflowException">
        ///     The hashSet contains too many elements.
        /// </exception>
        /// <value>
        ///     The number of items contained in this.
        /// </value>
        /// <remarks>
        ///     Count has snapshot semantics and represents the number of items in this
        ///     at the moment when Count was accessed.
        /// </remarks>
        public int Count => GetCount(_shards);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeShardedHashSet(NativeArray<Shard> shards) => _shards = shards;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(UnsafeShardedHashSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is UnsafeShardedHashSet<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("UnsafeShardedHashSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeShardedHashSet<T> left, UnsafeShardedHashSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeShardedHashSet<T> left, UnsafeShardedHashSet<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            foreach (ref var shard in _shards)
                shard.HashSet.Dispose();
            _shards.Dispose();
        }

        /// <summary>
        ///     Removes all keys and values from this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            foreach (ref var shard in _shards)
            {
                using (shard.RwLock.EnterWriteScope())
                {
                    shard.HashSet.Clear();
                }
            }
        }

        /// <summary>
        ///     Attempts to add the specified key to this.
        /// </summary>
        /// <param name="item">The element to add.</param>
        /// <returns>
        ///     true if the key was added to this successfully;
        ///     otherwise, false.
        /// </returns>
        /// <exception cref="OverflowException">This contains too many elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T item)
        {
            ref var shard = ref GetShard((uint)item.GetHashCode());
            using (shard.RwLock.EnterWriteScope())
            {
                return shard.HashSet.Add(item);
            }
        }

        /// <summary>
        ///     Attempts to remove with the specified key from this.
        /// </summary>
        /// <param name="item">The element to remove and return.</param>
        /// <returns>
        ///     true if an object was removed successfully;
        ///     otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            ref var shard = ref GetShard((uint)item.GetHashCode());
            using (shard.RwLock.EnterWriteScope())
            {
                return shard.HashSet.Remove(item);
            }
        }

        /// <summary>
        ///     Determines whether this contains the specified key.
        /// </summary>
        /// <param name="item">The key to locate in this.</param>
        /// <returns>
        ///     true if this contains an element with the specified key;
        ///     otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            ref var shard = ref GetShard((uint)item.GetHashCode());
            using (shard.RwLock.EnterReadScope())
            {
                return shard.HashSet.Contains(item);
            }
        }

        /// <summary>
        ///     Ger shard
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Shard GetShard(uint hashCode) => ref _shards[hashCode & ((uint)_shards.Length - 1)];

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool CheckIsEmpty(NativeArray<Shard> shards)
        {
            foreach (ref var shard in shards)
            {
                using (shard.RwLock.EnterReadScope())
                {
                    if (!shard.HashSet.IsEmpty)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <exception cref="OverflowException">
        ///     The hashSet contains too many elements.
        /// </exception>
        /// <value>
        ///     The number of items contained in this.
        /// </value>
        /// <remarks>
        ///     Count has snapshot semantics and represents the number of items in this
        ///     at the moment when Count was accessed.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetCount(NativeArray<Shard> shards)
        {
            var count = 0;
            foreach (ref var shard in shards)
            {
                using (shard.RwLock.EnterReadScope())
                {
                    checked
                    {
                        count += shard.HashSet.Count;
                    }
                }
            }

            return count;
        }

        /// <summary>
        ///     A single shard consisting of a hash map and its associated lock for concurrent access.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        internal struct Shard
        {
            /// <summary>
            ///     Reader‑writer lock protecting the hash map in this shard.
            /// </summary>
            public UnsafeReaderWriterLock RwLock;

            /// <summary>
            ///     The underlying set holding the items for this shard.
            /// </summary>
            public UnsafeHashSet<T> HashSet;
        }

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeShardedHashSet<T> Empty => default;

        /// <summary>
        ///     Initializes a new instance of this
        ///     class that is empty, has the default concurrency level, has the default initial capacity, and
        ///     uses the default comparer for the key type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeShardedHashSet<T> Create() => Create(-1, 31);

        /// <summary>
        ///     Initializes a new instance of this
        ///     class that is empty, has the default concurrency level, has the default initial capacity, and
        ///     uses the default comparer for the key type.
        /// </summary>
        /// <param name="concurrencyLevel">
        ///     The estimated number of threads that will update this concurrently, or -1 to indicate a default value.
        /// </param>
        /// <param name="capacity">
        ///     The initial number of elements that this can contain.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="concurrencyLevel" /> is less than 1.</exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="capacity" /> is less than 0.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeShardedHashSet<T> Create(int concurrencyLevel, int capacity)
        {
            if (concurrencyLevel < 1)
            {
                ThrowHelpers.ThrowIfNotEqual(concurrencyLevel, -1, ExceptionArgument.concurrencyLevel);
                concurrencyLevel = EnvironmentHelpers.DefaultConcurrencyLevel;
            }

            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            concurrencyLevel = (int)BitOperationsHelpers.RoundUpToPowerOf2((uint)concurrencyLevel);
            if (concurrencyLevel < 0)
                concurrencyLevel = 1 << 30;
            var shards = new NativeArray<Shard>(concurrencyLevel, CACHE_LINE_SIZE, true);
            foreach (ref var shard in shards)
                shard.HashSet = new UnsafeHashSet<T>(capacity);
            return new UnsafeShardedHashSet<T>(shards);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public Enumerator GetEnumerator() => new(_shards);

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Supports a simple iteration over a generic collection.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IIterator<T>, IDisposable
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly NativeArray<Shard> _handle;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Supports a simple iteration over a generic collection.
            /// </summary>
            private UnsafeHashSet<T>.Enumerator _enumerator;

            /// <summary>
            ///     Indicates whether the enumerator currently holds a read lock on a shard.
            /// </summary>
            private bool _locked;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeArray<Shard> handle)
            {
                _handle = handle;
                _index = -1;
                _enumerator = default;
                _locked = false;
            }

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
            ///     <see langword="false" /> if the enumerator has passed the end of the collection.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_index >= 0)
                {
                    if (_enumerator.MoveNext())
                        return true;

                    _handle[_index].RwLock.ExitRead();
                    _locked = false;
                }

                while (++_index < _handle.Length)
                {
                    ref var shard = ref _handle[_index];
                    shard.RwLock.EnterRead();
                    _locked = true;
                    _enumerator = shard.HashSet.GetEnumerator();

                    if (_enumerator.MoveNext())
                        return true;

                    shard.RwLock.ExitRead();
                    _locked = false;
                }

                return false;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                if (_locked)
                    _handle[_index].RwLock.ExitRead();
                _index = -1;
                _enumerator = default;
                _locked = false;
            }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly T Current => _enumerator.Current;

            /// <summary>
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => Reset();
        }
    }
}