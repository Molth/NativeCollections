using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native array
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeArray<T> : IDisposable, IEquatable<NativeArray<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            _buffer = NativeMemoryAllocator.AlignedAlloc<T>((uint)length);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="zeroed">Zeroed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray(int length, bool zeroed)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            _buffer = zeroed ? NativeMemoryAllocator.AlignedAllocZeroed<T>((uint)length) : NativeMemoryAllocator.AlignedAlloc<T>((uint)length);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray(int length, int alignment)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            if (alignment < 0)
                throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "MustBeNonNegative");
            if (!BitOperationsHelpers.IsPow2((uint)alignment))
                throw new ArgumentException("AlignmentMustBePow2", nameof(alignment));
            if (typeof(T) != typeof(byte) && (uint)alignment < NativeMemoryAllocator.AlignOf<T>())
                throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "MustBeGreaterOrEqual");
            _buffer = (T*)NativeMemoryAllocator.AlignedAlloc((uint)(length * sizeof(T)), (uint)alignment);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="zeroed">Zeroed</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray(int length, bool zeroed, int alignment)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            if (alignment < 0)
                throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "MustBeNonNegative");
            if (!BitOperationsHelpers.IsPow2((uint)alignment))
                throw new ArgumentException("AlignmentMustBePow2", nameof(alignment));
            if (typeof(T) != typeof(byte) && (uint)alignment < NativeMemoryAllocator.AlignOf<T>())
                throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "MustBeGreaterOrEqual");
            _buffer = zeroed ? (T*)NativeMemoryAllocator.AlignedAllocZeroed((uint)(length * sizeof(T)), (uint)alignment) : (T*)NativeMemoryAllocator.AlignedAlloc((uint)(length * sizeof(T)), (uint)alignment);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray(T* buffer, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            if (typeof(T) != typeof(byte) && (nint)buffer % (nint)NativeMemoryAllocator.AlignOf<T>() != 0)
                throw new AccessViolationException("MustBeAligned");
            _buffer = buffer;
            _length = length;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _buffer != null;

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
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index);
        }

        /// <summary>
        ///     Buffer
        /// </summary>
        public T* Buffer => _buffer;

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
        public bool Equals(NativeArray<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeArray<T> nativeArray && nativeArray == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_buffer).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeArray<{typeof(T).Name}>[{_length}]";

        /// <summary>
        ///     As pointer
        /// </summary>
        /// <returns>Pointer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(NativeArray<T> nativeArray) => nativeArray._buffer;

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(in NativeArray<T> nativeArray) => nativeArray.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(in NativeArray<T> nativeArray) => nativeArray.AsReadOnlySpan();

        /// <summary>
        ///     As native array
        /// </summary>
        /// <returns>NativeArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<T>(Span<T> span) => new((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), span.Length);

        /// <summary>
        ///     As native array
        /// </summary>
        /// <returns>NativeArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<T>(ReadOnlySpan<T> readOnlySpan) => new((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(readOnlySpan)), readOnlySpan.Length);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeArray<T> left, NativeArray<T> right) => left._length == right._length && left._buffer == right._buffer;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeArray<T> left, NativeArray<T> right) => left._length != right._length || left._buffer != right._buffer;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var buffer = _buffer;
            if (buffer == null)
                return;
            NativeMemoryAllocator.AlignedFree(buffer);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(_buffer), 0, (uint)(_length * sizeof(T)));

        /// <summary>
        ///     Cast
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<TTo> Cast<TTo>() where TTo : unmanaged => MemoryMarshal.Cast<T, TTo>(this);

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_buffer), _length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), _length - start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_buffer), _length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), _length - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), length);

        /// <summary>
        ///     Slice
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>NativeArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> Slice(int start) => new(UnsafeHelpers.Add<T>(_buffer, start), _length - start);

        /// <summary>
        ///     Slice
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>NativeArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> Slice(int start, int length) => new(UnsafeHelpers.Add<T>(_buffer, start), length);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeArray<T> Empty => new();

        /// <summary>
        ///     Get byte count
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Byte count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity) => capacity * sizeof(T) + (int)NativeMemoryAllocator.AlignOf<T>() - 1;

        /// <summary>
        ///     Create aligned native array
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="alignment">Alignment</param>
        /// <returns>NativeArray</returns>
        public static NativeArray<T> Create(Span<byte> buffer, uint alignment)
        {
            if (!BitOperationsHelpers.IsPow2(alignment))
                throw new ArgumentException("AlignmentMustBePow2", nameof(alignment));
            var ptr = Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
            var alignedPtr = (nint)NativeMemoryAllocator.AlignUp((nuint)(nint)ptr, alignment);
            var byteOffset = alignedPtr - (nint)ptr;
            var alignedBuffer = MemoryMarshal.Cast<byte, T>(buffer.Slice((int)byteOffset));
            return new NativeArray<T>((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(alignedBuffer)), alignedBuffer.Length);
        }

        /// <summary>
        ///     Create aligned native array
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="alignment">Alignment</param>
        /// <param name="byteOffset">Byte offset</param>
        /// <returns>NativeArray</returns>
        public static NativeArray<T> Create(Span<byte> buffer, uint alignment, out nint byteOffset)
        {
            if (!BitOperationsHelpers.IsPow2(alignment))
                throw new ArgumentException("AlignmentMustBePow2", nameof(alignment));
            var ptr = Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
            var alignedPtr = (nint)NativeMemoryAllocator.AlignUp((nuint)(nint)ptr, alignment);
            byteOffset = alignedPtr - (nint)ptr;
            var alignedBuffer = MemoryMarshal.Cast<byte, T>(buffer.Slice((int)byteOffset));
            return new NativeArray<T>((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(alignedBuffer)), alignedBuffer.Length);
        }

        /// <summary>
        ///     Create aligned native array
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>NativeArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> Create(Span<byte> buffer) => Create(buffer, (uint)NativeMemoryAllocator.AlignOf<T>());

        /// <summary>
        ///     Create aligned native array
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="byteOffset">Byte offset</param>
        /// <returns>NativeArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> Create(Span<byte> buffer, out nint byteOffset) => Create(buffer, (uint)NativeMemoryAllocator.AlignOf<T>(), out byteOffset);

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(this);

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
            ///     NativeArray
            /// </summary>
            private readonly NativeArray<T> _nativeArray;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeArray">NativeArray</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeArray<T> nativeArray)
            {
                _nativeArray = nativeArray;
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
                if (index < _nativeArray._length)
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
                get => ref _nativeArray[_index];
            }
        }
    }
}