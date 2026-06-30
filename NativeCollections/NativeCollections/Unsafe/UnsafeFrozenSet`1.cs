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
    ///     Unsafe hashSet
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public readonly unsafe struct UnsafeFrozenSet<T> : IIsCreated, IDisposable, IEquatable<UnsafeFrozenSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeFrozenSetHandle<T> _handle;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        ///     Items
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
        ///     Count
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
        ///     Structure
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
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSet<T> Create(NativeHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            source.CopyTo(items);
            return new UnsafeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSet<T> Create(UnsafeHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            source.CopyTo(items);
            return new UnsafeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSet<T> Create(StackallocHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            source.CopyTo(items);
            return new UnsafeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MustBeDistinct(nameof(source))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFrozenSet([MustBeDistinct] ReadOnlySpan<T> source) => _handle = Initialize(source);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(UnsafeFrozenSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is UnsafeFrozenSet<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => SR.Format("UnsafeFrozenSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeFrozenSet<T> left, UnsafeFrozenSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeFrozenSet<T> left, UnsafeFrozenSet<T> right) => !left.Equals(right);

        /// <summary>
        ///     Structure
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
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            handle.Dispose(ref handle.Value);
        }

        /// <summary>
        ///     Contains
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Contains</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item)
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            return handle.FindItemIndex(ref handle.Value, item) >= 0;
        }

        /// <summary>
        ///     Try to get the actual value
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Got</returns>
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
        ///     Try to get the actual value
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReference(in T equalValue, out NativeReference<T> actualValue)
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            var index = handle.FindItemIndex(ref handle.Value, equalValue);
            if (index >= 0)
            {
                actualValue = new NativeReference<T>(UnsafeHelpers.AsPointer(ref handle.Items(ref handle.Value)[index]));
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeFrozenSet<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeFrozenSet<T>.Enumerator GetEnumerator()
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            return handle.GetEnumerator(ref handle.Value);
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
    }
}