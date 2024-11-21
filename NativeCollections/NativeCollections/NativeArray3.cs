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
    [NativeCollection]
    public readonly unsafe struct NativeArray3<T> : IDisposable, IEquatable<NativeArray3<T>> where T : unmanaged
    {
        /// <summary>
        ///     Array
        /// </summary>
        private readonly T* _array;

        /// <summary>
        ///     Rows
        /// </summary>
        private readonly int _rows;

        /// <summary>
        ///     Columns
        /// </summary>
        private readonly int _columns;

        /// <summary>
        ///     Depth
        /// </summary>
        private readonly int _depth;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="rows">Rows</param>
        /// <param name="columns">Columns</param>
        /// <param name="depth">Depth</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray3(int rows, int columns, int depth)
        {
            if (rows < 0)
                throw new ArgumentOutOfRangeException(nameof(rows), rows, "MustBeNonNegative");
            if (columns < 0)
                throw new ArgumentOutOfRangeException(nameof(columns), columns, "MustBeNonNegative");
            if (depth < 0)
                throw new ArgumentOutOfRangeException(nameof(depth), depth, "MustBeNonNegative");
            _array = (T*)NativeMemoryAllocator.Alloc((uint)(rows * columns * depth * sizeof(T)));
            _rows = rows;
            _columns = columns;
            _depth = depth;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="rows">Rows</param>
        /// <param name="columns">Columns</param>
        /// <param name="depth">Depth</param>
        /// <param name="zeroed">Zeroed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray3(int rows, int columns, int depth, bool zeroed)
        {
            if (rows < 0)
                throw new ArgumentOutOfRangeException(nameof(rows), rows, "MustBeNonNegative");
            if (columns < 0)
                throw new ArgumentOutOfRangeException(nameof(columns), columns, "MustBeNonNegative");
            if (depth < 0)
                throw new ArgumentOutOfRangeException(nameof(depth), depth, "MustBeNonNegative");
            _array = zeroed ? (T*)NativeMemoryAllocator.AllocZeroed((uint)(rows * columns * depth * sizeof(T))) : (T*)NativeMemoryAllocator.Alloc((uint)(rows * columns * depth * sizeof(T)));
            _rows = rows;
            _columns = columns;
            _depth = depth;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="array">Array</param>
        /// <param name="rows">Rows</param>
        /// <param name="columns">Columns</param>
        /// <param name="depth">Depth</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray3(T* array, int rows, int columns, int depth)
        {
            if (rows < 0)
                throw new ArgumentOutOfRangeException(nameof(rows), rows, "MustBeNonNegative");
            if (columns < 0)
                throw new ArgumentOutOfRangeException(nameof(columns), columns, "MustBeNonNegative");
            if (depth < 0)
                throw new ArgumentOutOfRangeException(nameof(depth), depth, "MustBeNonNegative");
            _array = array;
            _rows = rows;
            _columns = columns;
            _depth = depth;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _array != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _rows == 0 || _columns == 0 || _depth == 0;

        /// <summary>
        ///     Array
        /// </summary>
        public T* Array => _array;

        /// <summary>
        ///     Rows
        /// </summary>
        public int Rows => _rows;

        /// <summary>
        ///     Columns
        /// </summary>
        public int Columns => _columns;

        /// <summary>
        ///     Depth
        /// </summary>
        public int Depth => _depth;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="rows">Rows</param>
        public NativeArray2<T> this[int rows]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_array + rows * _columns * _depth, _columns, _depth);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="rows">Rows</param>
        public NativeArray2<T> this[uint rows]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_array + rows * _columns * _depth, _columns, _depth);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="rows">Rows</param>
        /// <param name="columns">Columns</param>
        public NativeArray<T> this[int rows, int columns]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_array + rows * _columns * _depth + columns * _depth, _depth);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="rows">Rows</param>
        /// <param name="columns">Columns</param>
        public NativeArray<T> this[uint rows, uint columns]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_array + rows * _columns * _depth + columns * _depth, _depth);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="rows">Rows</param>
        /// <param name="columns">Columns</param>
        /// <param name="depth">Depth</param>
        public ref T this[int rows, int columns, int depth]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _array[rows * _columns * _depth + columns * _depth + depth];
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="rows">Rows</param>
        /// <param name="columns">Columns</param>
        /// <param name="depth">Depth</param>
        public ref T this[uint rows, uint columns, uint depth]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _array[rows * _columns * _depth + columns * _depth + depth];
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
        public override int GetHashCode() => ((nint)_array).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeArray3<{typeof(T).Name}>[{_rows}, {_columns}, {_depth}]";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeArray3<T> left, NativeArray3<T> right) => left._array == right._array;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeArray3<T> left, NativeArray3<T> right) => left._array != right._array;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var array = _array;
            if (array == null)
                return;
            NativeMemoryAllocator.Free(array);
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref *_array, _rows * _columns * _depth);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => MemoryMarshal.CreateSpan(ref *(_array + start), _rows * _columns * _depth - start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref *(_array + start), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref *_array, _rows * _columns * _depth);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref *(_array + start), _rows * _columns * _depth - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref *(_array + start), length);

        /// <summary>
        ///     As pointer
        /// </summary>
        /// <returns>Pointer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(NativeArray3<T> nativeArray3) => nativeArray3._array;

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativeArray3<T> nativeArray3) => nativeArray3.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativeArray3<T> nativeArray3) => nativeArray3.AsReadOnlySpan();

        /// <summary>
        ///     As native array
        /// </summary>
        /// <returns>NativeArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<T>(NativeArray3<T> nativeArray3) => new(nativeArray3._array, nativeArray3._rows * nativeArray3._columns * nativeArray3._depth);

        /// <summary>
        ///     As native memory array
        /// </summary>
        /// <returns>NativeMemoryArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryArray<T>(NativeArray3<T> nativeArray3) => new(nativeArray3._array, nativeArray3._rows * nativeArray3._columns * nativeArray3._depth);

        /// <summary>
        ///     As native slice
        /// </summary>
        /// <returns>NativeSlice</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSlice<T>(NativeArray3<T> nativeArray3) => new(nativeArray3._array, nativeArray3._rows * nativeArray3._columns * nativeArray3._depth);

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