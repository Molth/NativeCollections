using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a reference to a managed array pinned in memory using a <see cref="GCHandle" />.
    /// </summary>
    /// <typeparam name="T">The unmanaged element type.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly struct NativeArrayRef<T> : IIsCreated, IDisposable, IEquatable<NativeArrayRef<T>>, IReadOnlyCollection<T>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly GCHandle _handle;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        /// <param name="length">The total number of elements the internal data structure can hold.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeArrayRef(GCHandle handle, int length)
        {
            _handle = handle;
            _length = length;
        }

        /// <summary>
        ///     Creates a managed array reference with the specified length and a normal <see cref="GCHandle" />.
        /// </summary>
        /// <param name="length">
        ///     The number of elements in the array. Must be non‑negative.
        /// </param>
        /// <returns>A newly allocated array reference.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArrayRef<T> Create(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            return new NativeArrayRef<T>(GCHandle.Alloc(new T[length], GCHandleType.Normal), length);
        }

        /// <summary>
        ///     Creates a managed array reference with the specified length and handle type.
        /// </summary>
        /// <param name="length">
        ///     The number of elements in the array. Must be non‑negative.
        /// </param>
        /// <param name="type">The type of GCHandle to use.</param>
        /// <returns>A newly allocated managed array reference.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArrayRef<T> Create(int length, GCHandleType type)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            return new NativeArrayRef<T>(GCHandle.Alloc(new T[length], type), length);
        }

        /// <summary>
        ///     Initializes a new instance of this class from an external managed array.
        /// </summary>
        /// <param name="buffer">The managed array to wrap.</param>
        /// <returns>A native array reference that wraps <paramref name="buffer" />.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="buffer" /> is <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArrayRef<T> From(T[] buffer)
        {
            ThrowHelpers.ThrowIfNull(buffer, ExceptionArgument.buffer);
            return new NativeArrayRef<T>(GCHandle.Alloc(buffer, GCHandleType.Normal), buffer.Length);
        }

        /// <summary>
        ///     Wraps an external managed array using the specified handle type.
        /// </summary>
        /// <param name="buffer">The managed array to wrap.</param>
        /// <param name="type">The type of GCHandle to use.</param>
        /// <returns>A native array reference that wraps <paramref name="buffer" />.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="buffer" /> is <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArrayRef<T> From(T[] buffer, GCHandleType type)
        {
            ThrowHelpers.ThrowIfNull(buffer, ExceptionArgument.buffer);
            return new NativeArrayRef<T>(GCHandle.Alloc(buffer, type), buffer.Length);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsAllocated;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        ///     Gets a reference to the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to access.</param>
        /// <exception cref="IndexOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is outside the bounds of the underlying array.
        /// </exception>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Buffer[index];
        }

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public T[] Buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T[])_handle.Target!;
        }

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        public int Length => _length;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count => _length;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeArrayRef<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeArrayRef<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeArrayRef<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeArrayRef<T> left, NativeArrayRef<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeArrayRef<T> left, NativeArrayRef<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var handle = _handle;
            if (!handle.IsAllocated)
                return;

            handle.Free();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        public Enumerator GetEnumerator() => Enumerator.From(Buffer);

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
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IRefIterator<T>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly T[] _handle;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(T[] handle)
            {
                _handle = handle;
                _index = -1;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(T[] handle) => new(handle);

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
                var index = _index.WrappingAdd(1);
                if (index < _handle.Length)
                {
                    _index = index;
                    return true;
                }

                return false;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset() => _index = -1;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _handle[_index];
            }
        }
    }
}