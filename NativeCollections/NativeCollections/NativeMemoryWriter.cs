using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native memory writer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public unsafe ref struct NativeMemoryWriter
    {
        /// <summary>
        ///     Array
        /// </summary>
        public readonly byte* Array;

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
        public NativeMemoryWriter(byte* array, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            Array = array;
            Length = length;
            _position = 0;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => Array != null;

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
        public byte* this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Array + index;
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public byte* this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Array + index;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeMemoryWriter other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => throw new NotSupportedException("Cannot call Equals on NativeMemoryWriter");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => HashCode.Combine((int)(nint)Array, Length, _position);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeMemoryWriter";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeMemoryWriter left, NativeMemoryWriter right) => left.Array == right.Array && left.Length == right.Length && left._position == right._position;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeMemoryWriter left, NativeMemoryWriter right) => left.Array != right.Array || left.Length != right.Length || left._position != right._position;

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
        ///     Write
        /// </summary>
        /// <param name="obj">object</param>
        /// <typeparam name="T">Type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(T* obj) where T : unmanaged
        {
            if (_position + sizeof(T) > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {sizeof(T)}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(Array + _position, obj, (uint)sizeof(T));
            _position += sizeof(T);
        }

        /// <summary>
        ///     Try write
        /// </summary>
        /// <param name="obj">object</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Wrote</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWrite<T>(T* obj) where T : unmanaged
        {
            if (_position + sizeof(T) > Length)
                return false;
            Unsafe.CopyBlockUnaligned(Array + _position, obj, (uint)sizeof(T));
            _position += sizeof(T);
            return true;
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="count">Count</param>
        /// <typeparam name="T">Type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(T* obj, int count) where T : unmanaged
        {
            count *= sizeof(T);
            if (_position + count > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {count}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(Array + _position, obj, (uint)count);
            _position += count;
        }

        /// <summary>
        ///     Try write
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="count">Count</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Wrote</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWrite<T>(T* obj, int count) where T : unmanaged
        {
            count *= sizeof(T);
            if (_position + count > Length)
                return false;
            Unsafe.CopyBlockUnaligned(Array + _position, obj, (uint)count);
            _position += count;
            return true;
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="obj">object</param>
        /// <typeparam name="T">Type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(in T obj) where T : unmanaged
        {
            if (_position + sizeof(T) > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {sizeof(T)}, but buffer length is {Remaining}.");
            Unsafe.WriteUnaligned(Array + _position, obj);
            _position += sizeof(T);
        }

        /// <summary>
        ///     Try write
        /// </summary>
        /// <param name="obj">object</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Wrote</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWrite<T>(in T obj) where T : unmanaged
        {
            if (_position + sizeof(T) > Length)
                return false;
            Unsafe.WriteUnaligned(Array + _position, obj);
            _position += sizeof(T);
            return true;
        }

        /// <summary>
        ///     Write bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(byte* buffer, int length)
        {
            if (_position + length > Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"Requires size is {length}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(Array + _position, buffer, (uint)length);
            _position += length;
        }

        /// <summary>
        ///     Try write bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Wrote</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWriteBytes(byte* buffer, int length)
        {
            if (_position + length > Length)
                return false;
            Unsafe.CopyBlockUnaligned(Array + _position, buffer, (uint)length);
            _position += length;
            return true;
        }

        /// <summary>
        ///     Write bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(ReadOnlySpan<byte> buffer) => WriteBytes((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), buffer.Length);

        /// <summary>
        ///     Try write bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Wrote</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWriteBytes(ReadOnlySpan<byte> buffer) => TryWriteBytes((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), buffer.Length);

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _position = 0;

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref *Array, _position);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int length) => MemoryMarshal.CreateSpan(ref *Array, length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref *(Array + start), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref *Array, _position);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan(int length) => MemoryMarshal.CreateReadOnlySpan(ref *Array, length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref *(Array + start), length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(NativeMemoryWriter nativeMemoryWriter) => nativeMemoryWriter.AsSpan();

        /// <summary>
        ///     As native memory writer
        /// </summary>
        /// <returns>NativeMemoryWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryWriter(Span<byte> span) => new((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), span.Length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(NativeMemoryWriter nativeMemoryWriter) => nativeMemoryWriter.AsReadOnlySpan();

        /// <summary>
        ///     As native memory writer
        /// </summary>
        /// <returns>NativeMemoryWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryWriter(ReadOnlySpan<byte> readOnlySpan) => new((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(readOnlySpan)), readOnlySpan.Length);

        /// <summary>
        ///     As native memory reader
        /// </summary>
        /// <returns>NativeMemoryReader</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryReader(NativeMemoryWriter nativeMemoryWriter) => new(nativeMemoryWriter.Array, nativeMemoryWriter._position);

        /// <summary>
        ///     As native memory writer
        /// </summary>
        /// <returns>NativeMemoryWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryWriter(NativeArray<byte> nativeArray) => new(nativeArray.Array, nativeArray.Length);

        /// <summary>
        ///     As native memory writer
        /// </summary>
        /// <returns>NativeMemoryWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryWriter(NativeMemoryArray<byte> nativeMemoryArray) => new(nativeMemoryArray.Array, nativeMemoryArray.Length);

        /// <summary>
        ///     As native memory writer
        /// </summary>
        /// <returns>NativeMemoryWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryWriter(NativeSlice<byte> nativeSlice) => new(nativeSlice.Array + nativeSlice.Offset, nativeSlice.Count);

        /// <summary>
        ///     As native slice
        /// </summary>
        /// <returns>NativeSlice</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSlice<byte>(NativeMemoryWriter nativeMemoryWriter) => new(nativeMemoryWriter.Array, 0, nativeMemoryWriter._position);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeMemoryWriter Empty => new();
    }
}