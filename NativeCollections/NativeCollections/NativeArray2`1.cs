using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native array 2
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeArray2<T> : IDisposable, IEquatable<NativeArray2<T>> where T : unmanaged
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
        ///     Structure
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray2(int x, int y)
        {
            if (x < 0)
                throw new ArgumentOutOfRangeException(nameof(x), x, "MustBeNonNegative");
            if (y < 0)
                throw new ArgumentOutOfRangeException(nameof(y), y, "MustBeNonNegative");
            _buffer = (T*)NativeMemoryAllocator.Alloc((uint)(x * y * sizeof(T)));
            _x = x;
            _y = y;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="zeroed">Zeroed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray2(int x, int y, bool zeroed)
        {
            if (x < 0)
                throw new ArgumentOutOfRangeException(nameof(x), x, "MustBeNonNegative");
            if (y < 0)
                throw new ArgumentOutOfRangeException(nameof(y), y, "MustBeNonNegative");
            _buffer = zeroed ? (T*)NativeMemoryAllocator.AllocZeroed((uint)(x * y * sizeof(T))) : (T*)NativeMemoryAllocator.Alloc((uint)(x * y * sizeof(T)));
            _x = x;
            _y = y;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray2(T* buffer, int x, int y)
        {
            if (x < 0)
                throw new ArgumentOutOfRangeException(nameof(x), x, "MustBeNonNegative");
            if (y < 0)
                throw new ArgumentOutOfRangeException(nameof(y), y, "MustBeNonNegative");
            _buffer = buffer;
            _x = x;
            _y = y;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _buffer != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _x == 0 || _y == 0;

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
        ///     Get reference
        /// </summary>
        /// <param name="x">X</param>
        public NativeArray<T> this[int x]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_buffer + x * _y, _y);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="x">X</param>
        public NativeArray<T> this[uint x]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_buffer + x * _y, _y);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public ref T this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[x * _y + y];
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public ref T this[uint x, uint y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[x * _y + y];
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeArray2<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeArray2<T> nativeArray2 && nativeArray2 == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_buffer).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeArray2<{typeof(T).Name}>[{_x}, {_y}]";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeArray2<T> left, NativeArray2<T> right) => left._buffer == right._buffer;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeArray2<T> left, NativeArray2<T> right) => left._buffer != right._buffer;

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
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref *_buffer, _x * _y);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => MemoryMarshal.CreateSpan(ref *(_buffer + start), _x * _y - start);

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
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref *_buffer, _x * _y);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref *(_buffer + start), _x * _y - start);

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
        public static implicit operator T*(NativeArray2<T> nativeArray2) => nativeArray2._buffer;

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(in NativeArray2<T> nativeArray2) => nativeArray2.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(in NativeArray2<T> nativeArray2) => nativeArray2.AsReadOnlySpan();

        /// <summary>
        ///     As native array
        /// </summary>
        /// <returns>NativeArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<T>(NativeArray2<T> nativeArray2) => new(nativeArray2._buffer, nativeArray2._x * nativeArray2._y);

        /// <summary>
        ///     As native memory array
        /// </summary>
        /// <returns>NativeMemoryArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryArray<T>(NativeArray2<T> nativeArray2) => new(nativeArray2._buffer, nativeArray2._x * nativeArray2._y);

        /// <summary>
        ///     As native slice
        /// </summary>
        /// <returns>NativeSlice</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSlice<T>(NativeArray2<T> nativeArray2) => new(nativeArray2._buffer, nativeArray2._x * nativeArray2._y);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeArray2<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public NativeArray<T>.Enumerator GetEnumerator() => new(this);
    }
}