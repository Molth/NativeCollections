using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native memory linear allocator
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public unsafe struct NativeMemoryLinearAllocator : IDisposable, IEquatable<NativeMemoryLinearAllocator>
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
        public NativeMemoryLinearAllocator(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, nameof(length));
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
            if (buffer == null)
                return;
            NativeMemoryAllocator.AlignedFree(buffer);
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => Buffer != null;

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
        public readonly bool Equals(NativeMemoryLinearAllocator other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is NativeMemoryLinearAllocator nativeMemoryLinearAllocator && nativeMemoryLinearAllocator == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => "NativeMemoryLinearAllocator";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeMemoryLinearAllocator left, NativeMemoryLinearAllocator right) => left.Buffer == right.Buffer && left.Length == right.Length && left._position == right._position;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeMemoryLinearAllocator left, NativeMemoryLinearAllocator right) => left.Buffer != right.Buffer || left.Length != right.Length || left._position != right._position;

        /// <summary>
        ///     Advance
        /// </summary>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            var newPosition = _position + count;
            ThrowHelpers.ThrowIfGreaterThan((uint)newPosition, (uint)Length, nameof(count));
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
            ThrowHelpers.ThrowIfGreaterThan((uint)position, (uint)Length, nameof(position));
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

        /// <summary>Allocates an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAlignedAlloc<T>(uint elementCount, out T* ptr) where T : unmanaged
        {
            var byteCount = elementCount * (uint)sizeof(T);
            var alignment = (uint)NativeMemoryAllocator.AlignOf<T>();
            if (TryAlignedAlloc(byteCount, alignment, out var voidPtr))
            {
                ptr = (T*)voidPtr;
                return true;
            }

            ptr = null;
            return false;
        }

        /// <summary>Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAlignedAllocZeroed<T>(uint elementCount, out T* ptr) where T : unmanaged
        {
            var byteCount = elementCount * (uint)sizeof(T);
            var alignment = (uint)NativeMemoryAllocator.AlignOf<T>();
            if (TryAlignedAllocZeroed(byteCount, alignment, out var voidPtr))
            {
                ptr = (T*)voidPtr;
                return true;
            }

            ptr = null;
            return false;
        }

        /// <summary>Allocates an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAlignedAlloc(uint byteCount, uint alignment, out void* ptr)
        {
            ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, nameof(alignment));
            var position = (nint)NativeMemoryAllocator.AlignUp((nuint)((nint)Buffer + _position), alignment);
            if (position + (nint)byteCount > (nint)Buffer + Length)
            {
                ptr = null;
                return false;
            }

            ptr = (void*)position;
            _position = (int)(position + (nint)byteCount - (nint)Buffer);
            return true;
        }

        /// <summary>Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAlignedAllocZeroed(uint byteCount, uint alignment, out void* ptr)
        {
            if (!TryAlignedAlloc(byteCount, alignment, out ptr))
                return false;
            Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(ptr), 0, byteCount);
            return true;
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(Buffer), Length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start) => MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), UnsafeHelpers.ToIntPtr(start)), Length - start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), UnsafeHelpers.ToIntPtr(start)), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(Buffer), Length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), UnsafeHelpers.ToIntPtr(start)), Length - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), UnsafeHelpers.ToIntPtr(start)), length);

        /// <summary>
        ///     As pointer
        /// </summary>
        /// <returns>Pointer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte*(NativeMemoryLinearAllocator nativeMemoryLinearAllocator) => nativeMemoryLinearAllocator.Buffer;

        /// <summary>
        ///     As native memory linear allocator
        /// </summary>
        /// <returns>NativeMemoryLinearAllocator</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryLinearAllocator(Span<byte> span) => new((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), span.Length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(NativeMemoryLinearAllocator nativeMemoryLinearAllocator) => nativeMemoryLinearAllocator.AsSpan();

        /// <summary>
        ///     As native memory linear allocator
        /// </summary>
        /// <returns>NativeMemoryLinearAllocator</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeMemoryLinearAllocator(ReadOnlySpan<byte> readOnlySpan) => new((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(readOnlySpan)), readOnlySpan.Length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(NativeMemoryLinearAllocator nativeMemoryLinearAllocator) => nativeMemoryLinearAllocator.AsReadOnlySpan();

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeMemoryLinearAllocator Empty => new();
    }
}