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
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly struct NativeArrayRef<T> : IIsCreated, IDisposable, IEquatable<NativeArrayRef<T>>, IReadOnlyCollection<T>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly GCHandle _handle;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArrayRef(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            _handle = GCHandle.Alloc(new T[length], GCHandleType.Normal);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="type">GCHandle type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArrayRef(int length, GCHandleType type)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            _handle = GCHandle.Alloc(new T[length], type);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArrayRef(T[] buffer)
        {
            ThrowHelpers.ThrowIfNull(buffer, ExceptionArgument.buffer);
            _handle = GCHandle.Alloc(buffer, GCHandleType.Normal);
            _length = buffer.Length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="type">GCHandle type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArrayRef(T[] buffer, GCHandleType type)
        {
            ThrowHelpers.ThrowIfNull(buffer, ExceptionArgument.buffer);
            _handle = GCHandle.Alloc(buffer, type);
            _length = buffer.Length;
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
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Buffer[index];
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Buffer[index];
        }

        /// <summary>
        ///     Buffer
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
        public bool Equals(NativeArrayRef<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeArrayRef<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeArrayReference<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeArrayRef<T> left, NativeArrayRef<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeArrayRef<T> left, NativeArrayRef<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (!handle.IsAllocated)
                return;
            handle.Free();
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeArrayRef<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public Enumerator GetEnumerator() => new(Buffer);

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
        ///     Enumerator
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IRefIterator<T>
        {
            /// <summary>
            ///     Buffer
            /// </summary>
            private readonly T[] _handle;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(T[] handle)
            {
                _handle = handle;
                _index = -1;
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
                var index = _index + 1;
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _index = -1;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _handle[_index];
            }
        }
    }
}