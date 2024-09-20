#if NET7_0_OR_GREATER
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
        public int Position;

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
        public override int GetHashCode() => HashCode.Combine((int)(nint)Unsafe.AsPointer(ref Array), Length, Position);

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
        public static bool operator ==(NativeReader left, NativeReader right) => left.Array == right.Array && left.Length == right.Length && left.Position == right.Position;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeReader left, NativeReader right) => left.Array != right.Array || left.Length != right.Length || left.Position != right.Position;

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
        ///     Read
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            if (Position + sizeof(T) > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {sizeof(T)}, but buffer length is {Remaining}.");
            var obj = Unsafe.ReadUnaligned<T>(ref Unsafe.AddByteOffset(ref Array, Position));
            Position += sizeof(T);
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
            if (Position + sizeof(T) > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {sizeof(T)}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(obj, Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, Position)), (uint)sizeof(T));
            Position += sizeof(T);
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
            if (Position + sizeof(T) > Length)
                return false;
            Unsafe.CopyBlockUnaligned(obj, Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, Position)), (uint)sizeof(T));
            Position += sizeof(T);
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
            if (Position + count > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {count}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(obj, Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, Position)), (uint)count);
            Position += count;
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
            if (Position + count > Length)
                return false;
            Unsafe.CopyBlockUnaligned(obj, Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, Position)), (uint)count);
            Position += count;
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
            if (Position + sizeof(T) > Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {sizeof(T)}, but buffer length is {Remaining}.");
            obj = Unsafe.ReadUnaligned<T>(ref Unsafe.AddByteOffset(ref Array, Position));
            Position += sizeof(T);
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
            if (Position + sizeof(T) > Length)
                return false;
            obj = Unsafe.ReadUnaligned<T>(ref Unsafe.AddByteOffset(ref Array, Position));
            Position += sizeof(T);
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
            if (Position + length > Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"Requires size is {length}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(buffer, Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, Position)), (uint)length);
            Position += length;
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
            if (Position + length > Length)
                return false;
            Unsafe.CopyBlockUnaligned(buffer, Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Array, Position)), (uint)length);
            Position += length;
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
            if (Position + length > Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"Requires size is {length}, but buffer length is {Remaining}.");
            Unsafe.CopyBlockUnaligned(ref buffer, ref Unsafe.AddByteOffset(ref Array, Position), (uint)length);
            Position += length;
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
            if (Position + length > Length)
                return false;
            Unsafe.CopyBlockUnaligned(ref buffer, ref Unsafe.AddByteOffset(ref Array, Position), (uint)length);
            Position += length;
            return true;
        }

        /// <summary>
        ///     As native reader
        /// </summary>
        /// <returns>NativeReader</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeReader(Span<byte> span) => new NativeWriter((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), span.Length);

        /// <summary>
        ///     As native reader
        /// </summary>
        /// <returns>NativeReader</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeReader(ReadOnlySpan<byte> readOnlySpan) => new NativeWriter((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(readOnlySpan)), readOnlySpan.Length);

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
        public static implicit operator NativeMemoryReader(NativeReader nativeReader) => new((byte*)Unsafe.AsPointer(ref nativeReader.Array), nativeReader.Position);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeReader Empty => new();
    }
}
#endif