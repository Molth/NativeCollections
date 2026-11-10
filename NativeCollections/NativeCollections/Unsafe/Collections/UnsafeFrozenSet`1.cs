using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.NativeFrozenSet;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public readonly unsafe struct UnsafeFrozenSet<T> : IIsCreated, IDisposable, IEquatable<UnsafeFrozenSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeFrozenSetHandle<T> _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        ///     Gets a collection containing the values in this.
        /// </summary>
        public ReadOnlySpan<T> Items
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref var handle = ref Unsafe.AsRef(in _handle);
                return handle.Items(ref handle.Value).AsReadOnlySpan();
            }
        }

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref var handle = ref Unsafe.AsRef(in _handle);
                return handle.Count(ref handle.Value);
            }
        }

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        private UnsafeFrozenSet(UnsafeFrozenSetHandle<T> handle) => _handle = handle;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="source">The set from which to create this.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        public static UnsafeFrozenSet<T> Create(HashSet<T> source)
        {
            using (var items = NativeArray<T>.Create(source.Count))
            {
                var index = 0;
                foreach (var item in source)
                    items[(index = index.WrappingAdd(1)).WrappingSub(1)] = item;

                return Create(items.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="source">The set from which to create this.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        public static UnsafeFrozenSet<T> Create(NativeHashSet<T> source)
        {
            using (var items = NativeArray<T>.Create(source.Count))
            {
                source.CopyTo(items.AsSpan());
                return Create(items.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="source">The set from which to create this.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        public static UnsafeFrozenSet<T> Create(UnsafeHashSet<T> source)
        {
            using (var items = NativeArray<T>.Create(source.Count))
            {
                source.CopyTo(items.AsSpan());
                return Create(items.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="source">The set from which to create this.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        public static UnsafeFrozenSet<T> Create(StackallocHashSet<T> source)
        {
            using (var items = NativeArray<T>.Create(source.Count))
            {
                source.CopyTo(items.AsSpan());
                return Create(items.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="source">The items from which to create this.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSet<T> Create([MustBeDistinct] ReadOnlySpan<T> source) => new(Initialize(source));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(UnsafeFrozenSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is UnsafeFrozenSet<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("UnsafeFrozenSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeFrozenSet<T> left, UnsafeFrozenSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeFrozenSet<T> left, UnsafeFrozenSet<T> right) => !left.Equals(right);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="source">The items to store in the frozen set.</param>
        /// <returns>A handle to the frozen set that represents <paramref name="source" />.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static UnsafeFrozenSetHandle<T> Initialize(ReadOnlySpan<T> source)
        {
            UnsafeFrozenSetHandle<T> handle;
            if (source.IsEmpty)
            {
                handle = GetUnsafeHandle<EmptyFrozenSet<T>, T>();
                Unsafe.As<UnsafeFrozenSetValue, EmptyFrozenSet<T>>(ref handle.Value) = default;
                return handle;
            }

            if (source.Length <= 10)
            {
                if (FrozenHelpers.IsKnownComparable<T>())
                {
                    handle = GetUnsafeHandle<SmallComparableFrozenSet<T>, T>();
                    Unsafe.As<UnsafeFrozenSetValue, SmallComparableFrozenSet<T>>(ref handle.Value) = SmallComparableFrozenSet<T>.Create(source);
                    return handle;
                }

                handle = GetUnsafeHandle<SmallFrozenSet<T>, T>();
                Unsafe.As<UnsafeFrozenSetValue, SmallFrozenSet<T>>(ref handle.Value) = SmallFrozenSet<T>.Create(source);
                return handle;
            }

            if (typeof(T) == typeof(int))
            {
                var int32Handle = GetUnsafeHandle<Int32FrozenSet, int>();
                handle = Unsafe.As<UnsafeFrozenSetHandle<int>, UnsafeFrozenSetHandle<T>>(ref int32Handle);
                Unsafe.As<UnsafeFrozenSetValue, Int32FrozenSet>(ref handle.Value) = Int32FrozenSet.Create(MemoryMarshal.Cast<T, int>(source));
                return handle;
            }

            handle = GetUnsafeHandle<DefaultFrozenSet<T>, T>();
            Unsafe.As<UnsafeFrozenSetValue, DefaultFrozenSet<T>>(ref handle.Value) = DefaultFrozenSet<T>.Create(source);
            return handle;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            handle.Dispose(ref handle.Value);
        }

        /// <summary>
        ///     Determines whether this contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains the specified element; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            return handle.FindItemIndex(ref handle.Value, item) >= 0;
        }

        /// <summary>
        ///     Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     The value from the set that the search found, or the default value
        ///     of <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        ///     This can be useful when you want to reuse a previously stored reference instead of a newly constructed
        ///     one (so that more sharing of references can occur) or to look up a value that has more complete
        ///     data than the value you currently have, although their comparer functions indicate they are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(T equalValue, out T actualValue)
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            var index = handle.FindItemIndex(ref handle.Value, equalValue);
            if (index >= 0)
            {
                actualValue = handle.Items(ref handle.Value)[index];
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Searches the set for a given value and returns a reference to the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     A reference to the value from the set that the search found, or the default value
        ///     of <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        ///     This can be useful when you want to reuse a previously stored reference instead of a newly constructed
        ///     one (so that more sharing of references can occur) or to look up a value that has more complete
        ///     data than the value you currently have, although their comparer functions indicate they are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReference(T equalValue, out NativePtr<T> actualValue)
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            var index = handle.FindItemIndex(ref handle.Value, equalValue);
            if (index >= 0)
            {
                actualValue = NativePtr<T>.From(UnsafeHelpers.AsPointer(ref handle.Items(ref handle.Value)[index]));
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeFrozenSet<T>.Enumerator GetEnumerator()
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            return handle.GetEnumerator(ref handle.Value);
        }

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
    }
}