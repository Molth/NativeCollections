using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native array 3
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeArray3<T> : IDisposable, IEquatable<NativeArray3<T>> where T : unmanaged
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
            if (x < 0)
                throw new ArgumentOutOfRangeException(nameof(x), x, "MustBeNonNegative");
            if (y < 0)
                throw new ArgumentOutOfRangeException(nameof(y), y, "MustBeNonNegative");
            if (z < 0)
                throw new ArgumentOutOfRangeException(nameof(z), z, "MustBeNonNegative");
            _buffer = (T*)NativeMemoryAllocator.Alloc((uint)(x * y * z * sizeof(T)));
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
            if (x < 0)
                throw new ArgumentOutOfRangeException(nameof(x), x, "MustBeNonNegative");
            if (y < 0)
                throw new ArgumentOutOfRangeException(nameof(y), y, "MustBeNonNegative");
            if (z < 0)
                throw new ArgumentOutOfRangeException(nameof(z), z, "MustBeNonNegative");
            _buffer = zeroed ? (T*)NativeMemoryAllocator.AllocZeroed((uint)(x * y * z * sizeof(T))) : (T*)NativeMemoryAllocator.Alloc((uint)(x * y * z * sizeof(T)));
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
            if (x < 0)
                throw new ArgumentOutOfRangeException(nameof(x), x, "MustBeNonNegative");
            if (y < 0)
                throw new ArgumentOutOfRangeException(nameof(y), y, "MustBeNonNegative");
            if (z < 0)
                throw new ArgumentOutOfRangeException(nameof(z), z, "MustBeNonNegative");
            _buffer = buffer;
            _x = x;
            _y = y;
            _z = z;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _buffer != null;

        /// <summary>
        ///     Is empty
        /// </summary>
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
        ///     Get reference
        /// </summary>
        /// <param name="x">X</param>
        public NativeArray2<T> this[int x]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_buffer + x * _y * _z, _y, _z);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="x">X</param>
        public NativeArray2<T> this[uint x]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_buffer + x * _y * _z, _y, _z);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public NativeArray<T> this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_buffer + x * _y * _z + y * _z, _z);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public NativeArray<T> this[uint x, uint y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_buffer + x * _y * _z + y * _z, _z);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        public ref T this[int x, int y, int z]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[x * _y * _z + y * _z + z];
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        public ref T this[uint x, uint y, uint z]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[x * _y * _z + y * _z + z];
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeArray3<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeArray3<T> nativeArray3 && nativeArray3 == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_buffer).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeArray3<{typeof(T).Name}>[{_x}, {_y}, {_z}]";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeArray3<T> left, NativeArray3<T> right) => left._buffer == right._buffer;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeArray3<T> left, NativeArray3<T> right) => left._buffer != right._buffer;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var buffer = _buffer;
            if (buffer == null)
                return;
            NativeMemoryAllocator.Free(buffer);
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref *_buffer, _x * _y * _z);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => MemoryMarshal.CreateSpan(ref *(_buffer + start), _x * _y * _z - start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref *(_buffer + start), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref *_buffer, _x * _y * _z);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref *(_buffer + start), _x * _y * _z - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref *(_buffer + start), length);

        /// <summary>
        ///     As pointer
        /// </summary>
        /// <returns>Pointer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(NativeArray3<T> nativeArray3) => nativeArray3._buffer;

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(in NativeArray3<T> nativeArray3) => nativeArray3.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(in NativeArray3<T> nativeArray3) => nativeArray3.AsReadOnlySpan();

        /// <summary>
        ///     As native array
        /// </summary>
        /// <returns>NativeArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<T>(NativeArray3<T> nativeArray3) => new(nativeArray3._buffer, nativeArray3._x * nativeArray3._y * nativeArray3._z);

        /// <summary>
        ///     As native memory array
        /// </summary>
        /// <returns>NativeMemoryArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryArray<T>(NativeArray3<T> nativeArray3) => new(nativeArray3._buffer, nativeArray3._x * nativeArray3._y * nativeArray3._z);

        /// <summary>
        ///     As native slice
        /// </summary>
        /// <returns>NativeSlice</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSlice<T>(NativeArray3<T> nativeArray3) => new(nativeArray3._buffer, nativeArray3._x * nativeArray3._y * nativeArray3._z);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeArray3<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public NativeArray<T>.Enumerator GetEnumerator() => new(this);
    }
}