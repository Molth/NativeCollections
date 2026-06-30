using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe memory writer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeMemoryWriter : IIsCreated, IDisposable, IEquatable<UnsafeMemoryWriter>
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        public readonly byte* Buffer;

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
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryWriter(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            Buffer = buffer;
            Length = length;
            _position = 0;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {
            var buffer = Buffer;
            if (UnsafeHelpers.IsNull(buffer))
                return;
            NativeMemoryAllocator.AlignedFree(buffer);
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(Buffer);

        /// <summary>
        ///     Position
        /// </summary>
        public readonly int Position => _position;

        /// <summary>
        ///     Remaining
        /// </summary>
        public readonly int Remaining => Length - _position;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public readonly byte* this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnsafeHelpers.AddByteOffset<byte>(Buffer, index);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public readonly byte* this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnsafeHelpers.AddByteOffset<byte>(Buffer, (nint)index);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeMemoryWriter other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryWriter other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => "UnsafeMemoryWriter";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeMemoryWriter left, UnsafeMemoryWriter right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeMemoryWriter left, UnsafeMemoryWriter right) => !left.Equals(right);

        /// <summary>
        ///     Advance
        /// </summary>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            var newPosition = _position + count;
            ThrowHelpers.ThrowIfGreaterThan((uint)newPosition, (uint)Length, ExceptionArgument.count);
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
            if ((uint)newPosition > (uint)Length)
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
            ThrowHelpers.ThrowIfGreaterThan((uint)position, (uint)Length, ExceptionArgument.position);
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
            if ((uint)position > (uint)Length)
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
            ThrowHelpers.ThrowIfGreaterThan(_position + Unsafe.SizeOf<T>(), Length, ExceptionArgument.obj);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref Unsafe.AsRef<byte>(obj), (uint)Unsafe.SizeOf<T>());
            _position += Unsafe.SizeOf<T>();
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
            if (_position + Unsafe.SizeOf<T>() > Length)
                return false;
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref Unsafe.AsRef<byte>(obj), (uint)Unsafe.SizeOf<T>());
            _position += Unsafe.SizeOf<T>();
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
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            WriteSpan(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(obj), count));
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
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            return TryWriteSpan(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(obj), count));
        }

        /// <summary>
        ///     Write
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSpan<T>(ReadOnlySpan<T> buffer) where T : unmanaged
        {
            var count = buffer.Length * Unsafe.SizeOf<T>();
            ThrowHelpers.ThrowIfGreaterThan(_position + count, Length, ExceptionArgument.buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)count);
            _position += count;
        }

        /// <summary>
        ///     Try write
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWriteSpan<T>(ReadOnlySpan<T> buffer) where T : unmanaged
        {
            var count = buffer.Length * Unsafe.SizeOf<T>();
            if (_position + count > Length)
                return false;
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)count);
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
            ThrowHelpers.ThrowIfGreaterThan(_position + Unsafe.SizeOf<T>(), Length, ExceptionArgument.obj);
            Unsafe.WriteUnaligned(UnsafeHelpers.AddByteOffset(Buffer, new IntPtr(_position)), obj);
            _position += Unsafe.SizeOf<T>();
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
            if (_position + Unsafe.SizeOf<T>() > Length)
                return false;
            Unsafe.WriteUnaligned(UnsafeHelpers.AddByteOffset(Buffer, new IntPtr(_position)), obj);
            _position += Unsafe.SizeOf<T>();
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
            ThrowHelpers.ThrowIfGreaterThan(_position + length, Length, ExceptionArgument.length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref Unsafe.AsRef<byte>(buffer), (uint)length);
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
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref Unsafe.AsRef<byte>(buffer), (uint)length);
            _position += length;
            return true;
        }

        /// <summary>
        ///     Write bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(ReadOnlySpan<byte> buffer)
        {
            ThrowHelpers.ThrowIfGreaterThan(_position + buffer.Length, Length, ExceptionArgument.buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref MemoryMarshal.GetReference(buffer), (uint)buffer.Length);
            _position += buffer.Length;
        }

        /// <summary>
        ///     Try write bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Wrote</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWriteBytes(ReadOnlySpan<byte> buffer)
        {
            if (_position + buffer.Length > Length)
                return false;
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref MemoryMarshal.GetReference(buffer), (uint)buffer.Length);
            _position += buffer.Length;
            return true;
        }

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
        public readonly Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(Buffer), _position);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start) => MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(start)), Length - start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(start)), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(Buffer), _position);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(start)), Length - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(start)), length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(UnsafeMemoryWriter nativeMemoryWriter) => nativeMemoryWriter.AsSpan();

        /// <summary>
        ///     As native memory writer
        /// </summary>
        /// <returns>NativeMemoryWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(span))]
        public static implicit operator UnsafeMemoryWriter([MustBePinned] Span<byte> span) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(span)), span.Length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(UnsafeMemoryWriter nativeMemoryWriter) => nativeMemoryWriter.AsReadOnlySpan();

        /// <summary>
        ///     As native memory writer
        /// </summary>
        /// <returns>NativeMemoryWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(readOnlySpan))]
        public static implicit operator UnsafeMemoryWriter([MustBePinned] ReadOnlySpan<byte> readOnlySpan) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(readOnlySpan)), readOnlySpan.Length);

        /// <summary>
        ///     As native memory reader
        /// </summary>
        /// <returns>NativeMemoryReader</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafeMemoryReader(UnsafeMemoryWriter nativeMemoryWriter) => new(nativeMemoryWriter.Buffer, nativeMemoryWriter._position);

        /// <summary>
        ///     As native memory writer
        /// </summary>
        /// <returns>NativeMemoryWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafeMemoryWriter(NativeArray<byte> nativeArray) => new(nativeArray.Buffer, nativeArray.Length);

        /// <summary>
        ///     As native memory writer
        /// </summary>
        /// <returns>NativeMemoryWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafeMemoryWriter(NativeMemoryArray<byte> nativeMemoryArray) => new(nativeMemoryArray.Buffer, nativeMemoryArray.Length);

        /// <summary>
        ///     As native memory writer
        /// </summary>
        /// <returns>NativeMemoryWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafeMemoryWriter(NativeSlice<byte> nativeSlice) => new(UnsafeHelpers.AddByteOffset<byte>(nativeSlice.Buffer, nativeSlice.Offset), nativeSlice.Count);

        /// <summary>
        ///     As native slice
        /// </summary>
        /// <returns>NativeSlice</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSlice<byte>(UnsafeMemoryWriter nativeMemoryWriter) => new(nativeMemoryWriter.Buffer, 0, nativeMemoryWriter._position);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeMemoryWriter Empty => new();
    }
}