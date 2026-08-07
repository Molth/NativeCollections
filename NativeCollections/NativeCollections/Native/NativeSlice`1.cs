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
    ///     Represents a contiguous region of arbitrary native memory.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeSlice<T> : IIsCreated, IDisposable, IEquatable<NativeSlice<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Offset
        /// </summary>
        private readonly int _offset;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        private readonly int _count;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">The number of elements.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSlice(T* buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            _buffer = buffer;
            _offset = 0;
            _count = count;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">The number of elements.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSlice(T* buffer, int offset, int count)
        {
            ThrowHelpers.ThrowIfNegative(offset, ExceptionArgument.offset);
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            _buffer = buffer;
            _offset = offset;
            _count = count;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="nativeArray">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSlice(NativeArray<T> nativeArray)
        {
            _buffer = nativeArray.Buffer;
            _offset = 0;
            _count = nativeArray.Length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="nativeArray">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">The number of elements.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSlice(NativeArray<T> nativeArray, int offset, int count)
        {
            _buffer = nativeArray.Buffer;
            _offset = offset;
            _count = count;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="nativeMemoryArray">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSlice(NativeMemoryArray<T> nativeMemoryArray)
        {
            _buffer = nativeMemoryArray.Buffer;
            _offset = 0;
            _count = nativeMemoryArray.Length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="nativeMemoryArray">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">The number of elements.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSlice(NativeMemoryArray<T> nativeMemoryArray, int offset, int count)
        {
            _buffer = nativeMemoryArray.Buffer;
            _offset = offset;
            _count = count;
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(_offset + index));
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(_offset + index));
        }

        /// <summary>
        ///     Buffer
        /// </summary>
        public T* Buffer => _buffer;

        /// <summary>
        ///     Offset
        /// </summary>
        public int Offset => _offset;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count => _count;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeSlice<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeSlice<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeSlice<{0}>[{1}, {2}]", SR.GetTypeName(typeof(T)), _offset, _count);

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        /// <returns>Pointer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(NativeSlice<T> value) => value._buffer;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativeSlice<T> value) => value.AsSpan();

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(value))]
        public static implicit operator NativeSlice<T>([MustBePinned] Span<T> value) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), 0, value.Length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativeSlice<T> value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(value))]
        public static implicit operator NativeSlice<T>([MustBePinned] ReadOnlySpan<T> value) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), 0, value.Length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<T>(NativeSlice<T> value) => new(value._buffer, value._offset + value._count);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSlice<T>(NativeArray<T> value) => new(value);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryArray<T>(NativeSlice<T> value) => new(value._buffer, value._offset + value._count);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSlice<T>(NativeMemoryArray<T> value) => new(value);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeSlice<T> left, NativeSlice<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeSlice<T> left, NativeSlice<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Free(_buffer);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_offset), _count);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(_offset + start)), _count - start);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(_offset + start)), length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_offset), _count);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(_offset + start)), _count - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(_offset + start)), length);

        /// <summary>
        ///     Forms a slice out of the given span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSlice<T> Slice(int start) => new(_buffer, _offset + start, _count - start);

        /// <summary>
        ///     Forms a slice out of the given span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSlice<T> Slice(int start, int length) => new(_buffer, _offset + start, length);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeSlice<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public Enumerator GetEnumerator() => new(this);

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
            ///     NativeSlice
            /// </summary>
            private readonly NativeSlice<T> _handle;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeSlice<T> handle)
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
                if (index < _handle._count)
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