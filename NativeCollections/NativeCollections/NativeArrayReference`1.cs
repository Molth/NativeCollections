using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8600
#pragma warning disable CS8603
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native array reference
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public struct NativeArrayReference<T> : IDisposable, IEquatable<NativeArrayReference<T>>, IReadOnlyCollection<T>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private GCHandle _handle;

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
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
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
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
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
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "MustBeNotNull");
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
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "MustBeNotNull");
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
            get => (T[])_handle.Target;
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
        public bool Equals(NativeArrayReference<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeArrayReference<T> nativeArrayReference && nativeArrayReference == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeArrayReference<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeArrayReference<T> left, NativeArrayReference<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeArrayReference<T> left, NativeArrayReference<T> right) => left._handle != right._handle;

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
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     Buffer
            /// </summary>
            private readonly T[] _buffer;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(T[] buffer)
            {
                _buffer = buffer;
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
                if (index < _buffer.Length)
                {
                    _index = index;
                    return true;
                }

                return false;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _buffer[_index];
            }
        }
    }
}