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
    ///     Native array 3
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeArray3<T> : IIsCreated, IDisposable, IEquatable<NativeArray3<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     X
        /// </summary>
        private readonly int _x;

        /// <summary>
        ///     Y
        /// </summary>
        private readonly int _y;

        /// <summary>
        ///     Z
        /// </summary>
        private readonly int _z;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray3(int x, int y, int z)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            ThrowHelpers.ThrowIfNegative(z, ExceptionArgument.z);
            _buffer = NativeMemoryAllocator.AlignedAlloc<T>((uint)(x * y * z));
            _x = x;
            _y = y;
            _z = z;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <param name="zeroed">Zeroed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray3(int x, int y, int z, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            ThrowHelpers.ThrowIfNegative(z, ExceptionArgument.z);
            _buffer = zeroed ? NativeMemoryAllocator.AlignedAllocZeroed<T>((uint)(x * y * z)) : NativeMemoryAllocator.AlignedAlloc<T>((uint)(x * y * z));
            _x = x;
            _y = y;
            _z = z;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray3(int x, int y, int z, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            ThrowHelpers.ThrowIfNegative(z, ExceptionArgument.z);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfLessThan((uint)alignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            _buffer = (T*)NativeMemoryAllocator.AlignedAlloc((uint)(x * y * z * Unsafe.SizeOf<T>()), (uint)alignment);
            _x = x;
            _y = y;
            _z = z;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <param name="alignment">Alignment</param>
        /// <param name="zeroed">Zeroed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray3(int x, int y, int z, int alignment, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            ThrowHelpers.ThrowIfNegative(z, ExceptionArgument.z);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfLessThan((uint)alignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            _buffer = zeroed ? (T*)NativeMemoryAllocator.AlignedAllocZeroed((uint)(x * y * z * Unsafe.SizeOf<T>()), (uint)alignment) : (T*)NativeMemoryAllocator.AlignedAlloc((uint)(x * y * z * Unsafe.SizeOf<T>()), (uint)alignment);
            _x = x;
            _y = y;
            _z = z;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray3(T* buffer, int x, int y, int z)
        {
            ThrowHelpers.ThrowIfNegative(x, ExceptionArgument.x);
            ThrowHelpers.ThrowIfNegative(y, ExceptionArgument.y);
            ThrowHelpers.ThrowIfNegative(z, ExceptionArgument.z);
            _buffer = buffer;
            _x = x;
            _y = y;
            _z = z;
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value>
        ///     true if this is empty;
        ///     otherwise, false.
        /// </value>
        public bool IsEmpty => _x == 0 || _y == 0 || _z == 0;

        /// <summary>
        ///     Buffer
        /// </summary>
        public T* Buffer => _buffer;

        /// <summary>
        ///     X
        /// </summary>
        public int X => _x;

        /// <summary>
        ///     Y
        /// </summary>
        public int Y => _y;

        /// <summary>
        ///     Z
        /// </summary>
        public int Z => _z;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count => _x * _y * _z;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public NativeArray2<T> this[int x]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(UnsafeHelpers.Add<T>(_buffer, x * _y * _z), _y, _z);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public NativeArray2<T> this[uint x]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(UnsafeHelpers.Add<T>(_buffer, (nint)(x * _y * _z)), _y, _z);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public NativeArray<T> this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(UnsafeHelpers.Add<T>(_buffer, x * _y * _z + y * _z), _z);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public NativeArray<T> this[uint x, uint y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(UnsafeHelpers.Add<T>(_buffer, (nint)(x * _y * _z + y * _z)), _z);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[int x, int y, int z]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(x * _y * _z + y * _z + z));
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[uint x, uint y, uint z]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(x * _y * _z + y * _z + z));
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeArray3<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeArray3<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeArray3<{0}>[{1}, {2}, {3}]", SR.GetTypeName(typeof(T)), _x, _y, _z);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeArray3<T> left, NativeArray3<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeArray3<T> left, NativeArray3<T> right) => !left.Equals(right);

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
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_buffer), _x * _y * _z);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), _x * _y * _z - start);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_buffer), _x * _y * _z);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), _x * _y * _z - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), length);

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        /// <returns>Pointer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(NativeArray3<T> value) => value._buffer;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativeArray3<T> value) => value.AsSpan();

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativeArray3<T> value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<T>(NativeArray3<T> value) => new(value._buffer, value._x * value._y * value._z);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryArray<T>(NativeArray3<T> value) => new(value._buffer, value._x * value._y * value._z);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSlice<T>(NativeArray3<T> value) => new(value._buffer, value._x * value._y * value._z);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeArray3<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public NativeArray<T>.Enumerator GetEnumerator() => new(this);

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
    }
}