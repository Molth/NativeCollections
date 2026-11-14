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
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community)]
    public readonly struct UnsafeShardedHashSet<T> : IIsCreated, IDisposable, IEquatable<UnsafeShardedHashSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Array of shards, each containing a separate hash set and its own reader‑writer lock.
        /// </summary>
        /// <remarks>
        ///     Segment design is inspired by the algorithm outlined at: https://github.com/xacrimon/dashmap
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
        /// <value>
        ///     The number of items contained in this.
        /// </value>
        /// <exception cref="OverflowException">Thrown when the hash set contains too many elements.</exception>
        /// <remarks>
        ///     Each shard is read while its read lock is held, so the counts
        ///     are accumulated shard by shard rather than captured atomically
        ///     and may not correspond to any single moment in time.
        /// </remarks>
        public int Count => GetCount(_shards);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="shards">The shards that back the collection.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeShardedHashSet(NativeArray<Shard> shards) => _shards = shards;

        /// <summary>
        ///     Creates a new instance that is empty, has the default concurrency level, has the
        ///     default initial capacity, and uses the default comparer for the element type.
        /// </summary>
        /// <returns>A new instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeShardedHashSet<T> Create() => Create(-1, 31);

        /// <summary>
        ///     Creates a new instance that is empty, has the specified concurrency
        ///     level and capacity, and uses the default comparer for the element type.
        /// </summary>
        /// <param name="concurrencyLevel">
        ///     The estimated number of threads that will update this concurrently, or -1 to use the default concurrency level.
        /// </param>
        /// <param name="capacity">
        ///     The initial number of elements that each shard can contain.
        /// </param>
        /// <returns>A new instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="concurrencyLevel" /> is less than 1 and not -1,
        ///     or <paramref name="capacity" /> is negative.
        /// </exception>
        /// <remarks>
        ///     The collection is partitioned into shards whose number is the specified concurrency
        ///     level rounded up to a power of two, and each shard is created with the specified
        ///     initial capacity; the total initial capacity is therefore approximately the number of
        ///     shards multiplied by <paramref name="capacity" />. The exact shard count is an
        ///     implementation detail.
        /// </remarks>
        public static UnsafeShardedHashSet<T> Create(int concurrencyLevel, int capacity)
        {
            if (concurrencyLevel < 1)
            {
                ThrowHelpers.ThrowIfNotEqual(concurrencyLevel, -1, ExceptionArgument.concurrencyLevel);
                concurrencyLevel = EnvironmentHelpers.DefaultConcurrencyLevel;
            }

            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            concurrencyLevel = I32.FromU32Unchecked(BitOperationsHelpers.RoundUpToPowerOf2(U32.FromI32Unchecked(concurrencyLevel)));
            if (concurrencyLevel < 0)
                concurrencyLevel = 1 << 30;

            var shards = NativeArray<Shard>.Create(concurrencyLevel, CACHE_LINE_SIZE, true);
            foreach (ref var shard in shards)
                shard.HashSet = UnsafeHashSet<T>.Create(capacity);

            return new UnsafeShardedHashSet<T>(shards);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(UnsafeShardedHashSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is UnsafeShardedHashSet<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("UnsafeShardedHashSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeShardedHashSet<T> left, UnsafeShardedHashSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeShardedHashSet<T> left, UnsafeShardedHashSet<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            foreach (ref var shard in _shards)
                shard.HashSet.Dispose();

            _shards.Dispose();
        }

        /// <summary>
        ///     Removes all elements from this.
        /// </summary>
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
        ///     Attempts to add the specified element to this.
        /// </summary>
        /// <param name="item">The element to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the element was successfully
        ///     added to this; otherwise, <see langword="false" />.
        /// </returns>
        public bool Add(T item)
        {
            ref var shard = ref GetShard(U32.FromI32Unchecked(item.GetHashCode()));
            using (shard.RwLock.EnterWriteScope())
            {
                return shard.HashSet.Add(item);
            }
        }

        /// <summary>
        ///     Removes the specified element from this.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if an object was removed successfully; otherwise, <see langword="false" />.
        /// </returns>
        public bool Remove(T item)
        {
            ref var shard = ref GetShard(U32.FromI32Unchecked(item.GetHashCode()));
            using (shard.RwLock.EnterWriteScope())
            {
                return shard.HashSet.Remove(item);
            }
        }

        /// <summary>
        ///     Determines whether this contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains the specified element; otherwise, <see langword="false" />.
        /// </returns>
        public bool Contains(T item)
        {
            ref var shard = ref GetShard(U32.FromI32Unchecked(item.GetHashCode()));
            using (shard.RwLock.EnterReadScope())
            {
                return shard.HashSet.Contains(item);
            }
        }

        /// <summary>
        ///     Gets the shard selected by the specified hash code.
        /// </summary>
        /// <param name="hashCode">The hash code used to select the shard.</param>
        /// <returns>A reference to the shard selected by <paramref name="hashCode" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Shard GetShard(uint hashCode) => ref _shards[I32.FromU32Unchecked(hashCode & U32.FromI32Unchecked(_shards.Length).WrappingSub(1U))];

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <param name="shards">The shards to inspect.</param>
        /// <returns><see langword="true" /> if every shard is empty; otherwise, <see langword="false" />.</returns>
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
        /// <param name="shards">The shards to sum the counts from.</param>
        /// <returns>
        ///     The total number of items contained in all of <paramref name="shards" />.
        /// </returns>
        /// <exception cref="OverflowException">Thrown when the hash set contains too many elements.</exception>
        /// <remarks>
        ///     Each shard is read while its read lock is held, so the counts
        ///     are accumulated shard by shard rather than captured atomically.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetCount(NativeArray<Shard> shards)
        {
            var count = 0;
            foreach (ref var shard in shards)
            {
                using (shard.RwLock.EnterReadScope())
                {
                    count += shard.HashSet.Count;
                }
            }

            return count;
        }

        /// <summary>
        ///     A single shard consisting of a hash set and its associated lock for concurrent access.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        internal struct Shard
        {
            /// <summary>
            ///     Reader‑writer lock protecting the hash set in this shard.
            /// </summary>
            public UnsafeReaderWriterLock RwLock;

            /// <summary>
            ///     The underlying set holding the items for this shard.
            /// </summary>
            public UnsafeHashSet<T> HashSet;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        /// <remarks>
        ///     The enumerator acquires the read lock of each shard in turn. Do not mutate this
        ///     hash set from the enumerating thread while enumeration is in progress.
        /// </remarks>
        public Enumerator GetEnumerator() => Enumerator.From(_shards);

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
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
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
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
        /// <remarks>
        ///     At most one shard read lock is held at a time. Mutating the hash set from the
        ///     enumerating thread while enumeration is in progress may deadlock; take a snapshot
        ///     instead.
        /// </remarks>
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
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(NativeArray<Shard> handle)
            {
                _handle = handle;
                _index = -1;
                _enumerator = default;
                _locked = false;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(NativeArray<Shard> handle) => new(handle);

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
                if (_locked)
                {
                    if (_enumerator.MoveNext())
                        return true;

                    _handle[_index].RwLock.ExitRead();
                    _locked = false;
                }

                while ((_index = _index.WrappingAdd(1)) < _handle.Length)
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
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly T Current => _enumerator.Current;

            /// <summary>
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose() => Reset();
        }
    }
}