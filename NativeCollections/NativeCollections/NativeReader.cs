#if NET7_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native memory reader
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public unsafe ref struct NativeReader
    {
        /// <summary>
        ///     Array
        /// </summary>
        public readonly ref byte Array;

        /// <summary>
        ///     Length
        /// </summary>
        public readonly int Length;

        /// <summary>
        ///     Position
        /// </summary>
        private int _position;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="array">Array</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReader(byte* array, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            Array = ref *array;
            Length = length;
            _position = 0;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => Unsafe.AsPointer(ref Unsafe.AsRef(in Array)) != null;

        /// <summary>
        ///     Position
        /// </summary>
        public int Position => _position;

        /// <summary>
        ///     Remaining
        /// </summary>
        public int Remaining => Length - _position;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AddByteOffset(ref Array, index);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref byte this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AddByteOffset(ref Array, index);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeReader other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => throw new NotSupportedException("Cannot call Equals on NativeReader");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => HashCode.Combine((nint)Unsafe.AsPointer(ref Array), Length, _position);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeReader";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeReader left, NativeReader right) => left.Array == right.Array && left.Length == right.Length && left._position == right._position;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeReader left, NativeReader right) => left.Array != right.Array || left.Length != right.Length || left._position != right._position;

        /// <summary>
        ///     Advance
        /// </summary>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            var newPosition = _position + count;
            if (newPosition < 0 || newPosition > Length)
                throw new ArgumentOutOfRangeException(nameof(count), "Cannot advance past the end of the buffer.");
            _position = newPosition;
        }

        /// <summary>
        ///     Try advance
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Advanced</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdvance(int count)
        {
            var newPosition = _position + count;
            if (newPosition < 0 || newPosition > Length)
                return false;
            _position = newPosition;
            return true;
        }

        /// <summary>
        ///     Set position
        /// </summary>
        /// <param name="position">Position</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(int position)
        {
            if (position < 0 || position > Length)
                throw new ArgumentOutOfRangeException(nameof(position), "Cannot advance past the end of the buffer.");
            _position = position;
        }

        /// <summary>
        ///     Try set position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Set</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetPosition(int position)
        {
            if (position < 0 || position > Length)
                return false;
            _position = position;
            return true;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            if (_position + sizeof(T) > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {sizeof(T)}, but buffer length is {Remaining}.");
            var obj = Unsafe.ReadUnaligned<T>(ref Unsafe.AddByteOffset(ref Array, _position));
            _position += sizeof(T);
            return obj;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="obj">object</param>
        /// <typeparam name="T">Type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(T* obj) where T : unmanaged
        {
            if (_position + sizeof(T) > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {sizeof(T)}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(obj, Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, _position)), (uint)sizeof(T));
            _position += sizeof(T);
        }

        /// <summary>
        ///     Try read
        /// </summary>
        /// <param name="obj">object</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Read</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(T* obj) where T : unmanaged
        {
            if (_position + sizeof(T) > Length)
                return false;
            Unsafe.CopyBlockUnaligned(obj, Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, _position)), (uint)sizeof(T));
            _position += sizeof(T);
            return true;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="count">Count</param>
        /// <typeparam name="T">Type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(T* obj, int count) where T : unmanaged
        {
            count *= sizeof(T);
            if (_position + count > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {count}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(obj, Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, _position)), (uint)count);
            _position += count;
        }

        /// <summary>
        ///     Try read
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="count">Count</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Read</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(T* obj, int count) where T : unmanaged
        {
            count *= sizeof(T);
            if (_position + count > Length)
                return false;
            Unsafe.CopyBlockUnaligned(obj, Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, _position)), (uint)count);
            _position += count;
            return true;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="obj">object</param>
        /// <typeparam name="T">Type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(ref T obj) where T : unmanaged
        {
            if (_position + sizeof(T) > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {sizeof(T)}, but buffer length is {Remaining}.");
            obj = Unsafe.ReadUnaligned<T>(ref Unsafe.AddByteOffset(ref Array, _position));
            _position += sizeof(T);
        }

        /// <summary>
        ///     Try read
        /// </summary>
        /// <param name="obj">object</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Read</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(ref T obj) where T : unmanaged
        {
            if (_position + sizeof(T) > Length)
                return false;
            obj = Unsafe.ReadUnaligned<T>(ref Unsafe.AddByteOffset(ref Array, _position));
            _position += sizeof(T);
            return true;
        }

        /// <summary>
        ///     Read bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBytes(byte* buffer, int length)
        {
            if (_position + length > Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"Requires size is {length}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(buffer, Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, _position)), (uint)length);
            _position += length;
        }

        /// <summary>
        ///     Try read bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Read</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadBytes(byte* buffer, int length)
        {
            if (_position + length > Length)
                return false;
            Unsafe.CopyBlockUnaligned(buffer, Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, _position)), (uint)length);
            _position += length;
            return true;
        }

        /// <summary>
        ///     Read bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBytes(ref byte buffer, int length)
        {
            if (_position + length > Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"Requires size is {length}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(ref buffer, ref Unsafe.AddByteOffset(ref Array, _position), (uint)length);
            _position += length;
        }

        /// <summary>
        ///     Try read bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Read</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadBytes(ref byte buffer, int length)
        {
            if (_position + length > Length)
                return false;
            Unsafe.CopyBlockUnaligned(ref buffer, ref Unsafe.AddByteOffset(ref Array, _position), (uint)length);
            _position += length;
            return true;
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Array, Length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start) => MemoryMarshal.CreateSpan(ref (Unsafe.AddByteOffset(ref Array, start)), Length - start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref (Unsafe.AddByteOffset(ref Array, start)), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Array, Length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref (Unsafe.AddByteOffset(ref Array, start)), Length - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref (Unsafe.AddByteOffset(ref Array, start)), length);

        /// <summary>
        ///     As native reader
        /// </summary>
        /// <returns>NativeReader</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeReader(Span<byte> span) => new NativeWriter((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), span.Length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(NativeReader nativeReader) => nativeReader.AsSpan();

        /// <summary>
        ///     As native reader
        /// </summary>
        /// <returns>NativeReader</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeReader(ReadOnlySpan<byte> readOnlySpan) => new NativeWriter((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(readOnlySpan)), readOnlySpan.Length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(NativeReader nativeReader) => nativeReader.AsReadOnlySpan();

        /// <summary>
        ///     As native reader
        /// </summary>
        /// <returns>NativeReader</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeReader(NativeMemoryReader nativeMemoryReader) => new NativeWriter(nativeMemoryReader.Array, nativeMemoryReader.Length);

        /// <summary>
        ///     As native memory reader
        /// </summary>
        /// <returns>NativeMemoryReader</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryReader(NativeReader nativeReader) => new((byte*)Unsafe.AsPointer(ref nativeReader.Array), nativeReader._position);

        /// <summary>
        ///     As native memory reader
        /// </summary>
        /// <returns>NativeMemoryReader</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeReader(NativeArray<byte> nativeArray) => new(nativeArray.Array, nativeArray.Length);

        /// <summary>
        ///     As native memory reader
        /// </summary>
        /// <returns>NativeMemoryReader</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeReader(NativeMemoryArray<byte> nativeMemoryArray) => new(nativeMemoryArray.Array, nativeMemoryArray.Length);

        /// <summary>
        ///     As native memory writer
        /// </summary>
        /// <returns>NativeMemoryWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeReader(NativeSlice<byte> nativeSlice) => new(nativeSlice.Array + nativeSlice.Offset, nativeSlice.Count);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeReader Empty => new();
    }
}
#endif