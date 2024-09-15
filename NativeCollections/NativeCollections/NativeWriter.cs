using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native writer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe ref struct NativeWriter
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
        public int Position;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="array">Array</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeWriter(byte* array, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            Array = ref *array;
            Length = length;
            Position = 0;
        }

        /// <summary>
        ///     Remaining
        /// </summary>
        public int Remaining => Length - Position;

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
        public bool Equals(NativeWriter other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => throw new NotSupportedException("Cannot call Equals on NativeWriter");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => HashCode.Combine((int)(nint)Unsafe.AsPointer(ref Array), Length, Position);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeWriter";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeWriter left, NativeWriter right) => left.Array == right.Array && left.Length == right.Length && left.Position == right.Position;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeWriter left, NativeWriter right) => left.Array != right.Array || left.Length != right.Length || left.Position != right.Position;

        /// <summary>
        ///     Advance
        /// </summary>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            var newPosition = Position + count;
            if (newPosition < 0 || newPosition > Length)
                throw new ArgumentOutOfRangeException(nameof(count), "Cannot advance past the end of the buffer.");
            Position = newPosition;
        }

        /// <summary>
        ///     Try advance
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Advanced</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdvance(int count)
        {
            var newPosition = Position + count;
            if (newPosition < 0 || newPosition > Length)
                return false;
            Position = newPosition;
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
            if (Position + sizeof(T) > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {sizeof(T)}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, Position)), obj, (uint)sizeof(T));
            Position += sizeof(T);
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
            if (Position + sizeof(T) > Length)
                return false;
            Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, Position)), obj, (uint)sizeof(T));
            Position += sizeof(T);
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
            if (Position + count > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {count}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, Position)), obj, (uint)count);
            Position += count;
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
            if (Position + count > Length)
                return false;
            Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, Position)), obj, (uint)count);
            Position += count;
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
            if (Position + sizeof(T) > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {sizeof(T)}, but buffer length is {Remaining}.");
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref Array, Position), obj);
            Position += sizeof(T);
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
            if (Position + sizeof(T) > Length)
                return false;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref Array, Position), obj);
            Position += sizeof(T);
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
            if (Position + length > Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"Requires size is {length}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, Position)), buffer, (uint)length);
            Position += length;
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
            if (Position + length > Length)
                return false;
            Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, Position)), buffer, (uint)length);
            Position += length;
            return true;
        }

        /// <summary>
        ///     Write bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(ref byte buffer, int length)
        {
            if (Position + length > Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"Requires size is {length}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Array, Position), ref buffer, (uint)length);
            Position += length;
        }

        /// <summary>
        ///     Try write bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Wrote</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWriteBytes(ref byte buffer, int length)
        {
            if (Position + length > Length)
                return false;
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Array, Position), ref buffer, (uint)length);
            Position += length;
            return true;
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Position = 0;

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Array, Position);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int length) => MemoryMarshal.CreateSpan(ref Array, length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Array, start), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Array, Position);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan(int length) => MemoryMarshal.CreateReadOnlySpan(ref Array, length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref Array, start), length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(NativeWriter nativeWriter) => nativeWriter.AsSpan();

        /// <summary>
        ///     As native writer
        /// </summary>
        /// <returns>NativeWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeWriter(Span<byte> span) => new((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), span.Length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(NativeWriter nativeWriter) => nativeWriter.AsReadOnlySpan();

        /// <summary>
        ///     As native writer
        /// </summary>
        /// <returns>NativeWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeWriter(ReadOnlySpan<byte> readOnlySpan) => new((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(readOnlySpan)), readOnlySpan.Length);

        /// <summary>
        ///     As native reader
        /// </summary>
        /// <returns>NativeReader</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeReader(NativeWriter nativeWriter) => new((byte*)Unsafe.AsPointer(ref nativeWriter.Array), nativeWriter.Position);

        /// <summary>
        ///     As native writer
        /// </summary>
        /// <returns>NativeWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeWriter(NativeArray<byte> nativeArray) => new(nativeArray.Array, nativeArray.Length);

        /// <summary>
        ///     As native writer
        /// </summary>
        /// <returns>NativeWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeWriter(NativeMemoryArray<byte> nativeMemoryArray) => new(nativeMemoryArray.Array, nativeMemoryArray.Length);

        /// <summary>
        ///     As native writer
        /// </summary>
        /// <returns>NativeWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeWriter(NativeSlice<byte> nativeSlice) => new(nativeSlice.Array + nativeSlice.Offset, nativeSlice.Count);

        /// <summary>
        ///     As native slice
        /// </summary>
        /// <returns>NativeSlice</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeSlice<byte>(NativeWriter nativeWriter) => new((byte*)Unsafe.AsPointer(ref nativeWriter.Array), 0, nativeWriter.Position);

        /// <summary>
        ///     As native writer
        /// </summary>
        /// <returns>NativeWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeWriter(NativeMemoryWriter nativeMemoryWriter) => new(nativeMemoryWriter.Array, nativeMemoryWriter.Length);

        /// <summary>
        ///     As native memory writer
        /// </summary>
        /// <returns>NativeMemoryWriter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryWriter(NativeWriter nativeWriter) => new((byte*)Unsafe.AsPointer(ref nativeWriter.Array), nativeWriter.Length);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeWriter Empty => new();
    }
}