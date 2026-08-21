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
                return handle.Items(ref handle.Value);
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
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSet<T> Create(HashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            var index = 0;
            foreach (var item in source)
                items[index++] = item;
            return new UnsafeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSet<T> Create(NativeHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            source.CopyTo(items);
            return new UnsafeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSet<T> Create(UnsafeHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            source.CopyTo(items);
            return new UnsafeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSet<T> Create(StackallocHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            source.CopyTo(items);
            return new UnsafeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MustBeDistinct(nameof(source))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFrozenSet([MustBeDistinct] ReadOnlySpan<T> source) => _handle = Initialize(source);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(UnsafeFrozenSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is UnsafeFrozenSet<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("UnsafeFrozenSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeFrozenSet<T> left, UnsafeFrozenSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeFrozenSet<T> left, UnsafeFrozenSet<T> right) => !left.Equals(right);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnsafeFrozenSetHandle<T> Initialize(ReadOnlySpan<T> source)
        {
            UnsafeFrozenSetHandle<T> handle;
            if (source.IsEmpty)
            {
                handle = GetUnsafeHandle<EmptyFrozenSet<T>, T>();
                Unsafe.As<UnsafeFrozenSetValue, EmptyFrozenSet<T>>(ref handle.Value) = new EmptyFrozenSet<T>();
                return handle;
            }

            if (source.Length <= 10)
            {
                if (FrozenHelpers.IsKnownComparable<T>())
                {
                    handle = GetUnsafeHandle<SmallComparableFrozenSet<T>, T>();
                    Unsafe.As<UnsafeFrozenSetValue, SmallComparableFrozenSet<T>>(ref handle.Value) = new SmallComparableFrozenSet<T>(source);
                    return handle;
                }

                handle = GetUnsafeHandle<SmallFrozenSet<T>, T>();
                Unsafe.As<UnsafeFrozenSetValue, SmallFrozenSet<T>>(ref handle.Value) = new SmallFrozenSet<T>(source);
                return handle;
            }

            if (typeof(T) == typeof(int))
            {
                var int32Handle = GetUnsafeHandle<Int32FrozenSet, int>();
                handle = Unsafe.As<UnsafeFrozenSetHandle<int>, UnsafeFrozenSetHandle<T>>(ref int32Handle);
                Unsafe.As<UnsafeFrozenSetValue, Int32FrozenSet>(ref handle.Value) = new Int32FrozenSet(MemoryMarshal.Cast<T, int>(source));
                return handle;
            }

            handle = GetUnsafeHandle<DefaultFrozenSet<T>, T>();
            Unsafe.As<UnsafeFrozenSetValue, DefaultFrozenSet<T>>(ref handle.Value) = new DefaultFrozenSet<T>(source);
            return handle;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            handle.Dispose(ref handle.Value);
        }

        /// <summary>
        ///     Determines whether this contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in this.</param>
        /// <returns>true if this contains the specified element; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item)
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            return handle.FindItemIndex(ref handle.Value, item) >= 0;
        }

        /// <summary>
        ///     Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     The value from the set that the search found, or the default value of
        ///     <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        ///     This can be useful when you want to reuse a previously stored reference instead of
        ///     a newly constructed one (so that more sharing of references can occur) or to look up
        ///     a value that has more complete data than the value you currently have, although their
        ///     comparer functions indicate they are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in T equalValue, out T actualValue)
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
        ///     Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     The value from the set that the search found, or the default value of
        ///     <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        ///     This can be useful when you want to reuse a previously stored reference instead of
        ///     a newly constructed one (so that more sharing of references can occur) or to look up
        ///     a value that has more complete data than the value you currently have, although their
        ///     comparer functions indicate they are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReference(in T equalValue, out NativePtr<T> actualValue)
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            var index = handle.FindItemIndex(ref handle.Value, equalValue);
            if (index >= 0)
            {
                actualValue = new NativePtr<T>(UnsafeHelpers.AsPointer(ref handle.Items(ref handle.Value)[index]));
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeFrozenSet<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
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