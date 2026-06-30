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
    ///     Native array reference
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly struct NativeArrayReference<T> : IIsCreated, IDisposable, IEquatable<NativeArrayReference<T>>, IReadOnlyCollection<T>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly GCHandle _handle;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArrayReference(int length)
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
        public NativeArrayReference(int length, GCHandleType type)
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
        public NativeArrayReference(T[] buffer)
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
        public NativeArrayReference(T[] buffer, GCHandleType type)
        {
            ThrowHelpers.ThrowIfNull(buffer, ExceptionArgument.buffer);
            _handle = GCHandle.Alloc(buffer, type);
            _length = buffer.Length;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle.IsAllocated;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Buffer[index];
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
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
        ///     Length
        /// </summary>
        public int Length => _length;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _length;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeArrayReference<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeArrayReference<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => SR.Format("NativeArrayReference<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeArrayReference<T> left, NativeArrayReference<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeArrayReference<T> left, NativeArrayReference<T> right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
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
        public static NativeArrayReference<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(Buffer);

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
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
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
            ///     Reset
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _index = -1;

            /// <summary>
            ///     Current
            /// </summary>
            public readonly ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _handle[_index];
            }
        }
    }
}