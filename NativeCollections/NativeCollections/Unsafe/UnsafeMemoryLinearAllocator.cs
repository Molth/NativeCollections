using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a memory allocator that provides reusable dynamic-size memory blocks.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeMemoryLinearAllocator : IIsCreated, IDisposable, IEquatable<UnsafeMemoryLinearAllocator>
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        public readonly byte* Buffer;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
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
        public UnsafeMemoryLinearAllocator(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            Buffer = buffer;
            Length = length;
            _position = 0;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => Box.Free(Buffer);

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(Buffer);

        /// <summary>
        ///     Gets the current position within the stream.
        /// </summary>
        public readonly int Position => _position;

        /// <summary>
        ///     Gets the number of remaining bytes available in the bucket.
        /// </summary>
        public readonly int Remaining => Length - _position;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly byte* this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnsafeHelpers.AddByteOffset<byte>(Buffer, index);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly byte* this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnsafeHelpers.AddByteOffset<byte>(Buffer, (nint)index);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeMemoryLinearAllocator other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryLinearAllocator other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeMemoryLinearAllocator";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeMemoryLinearAllocator left, UnsafeMemoryLinearAllocator right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeMemoryLinearAllocator left, UnsafeMemoryLinearAllocator right) => !left.Equals(right);

        /// <summary>
        ///     Notifies this that <paramref name="count" /> data items were written to the output.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     The position is set to a negative value or a value greater than
        ///     <see cref="Length"></see>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            var newPosition = _position + count;
            ThrowHelpers.ThrowIfGreaterThan((uint)newPosition, (uint)Length, ExceptionArgument.count);
            _position = newPosition;
        }

        /// <summary>
        ///     Notifies this that <paramref name="count" /> data items were written to the output.
        /// </summary>
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
        ///     Sets the current position within the stream.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     The position is set to a negative value or a value greater than
        ///     <see cref="Length"></see>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(int position)
        {
            ThrowHelpers.ThrowIfGreaterThan((uint)position, (uint)Length, ExceptionArgument.position);
            _position = position;
        }

        /// <summary>
        ///     Sets the current position within the stream.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetPosition(int position)
        {
            if ((uint)position > (uint)Length)
                return false;
            _position = position;
            return true;
        }

        /// <summary>
        ///     Allocates an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAlignedAlloc<T>(uint elementCount, out T* ptr) where T : unmanaged
        {
            var byteCount = elementCount * (uint)Unsafe.SizeOf<T>();
            var alignment = NativeMemoryAllocator.AlignOf<T>();
            if (TryAlignedAlloc(byteCount, alignment, out var voidPtr))
            {
                ptr = (T*)voidPtr;
                return true;
            }

            ptr = null;
            return false;
        }

        /// <summary>
        ///     Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAlignedAllocZeroed<T>(uint elementCount, out T* ptr) where T : unmanaged
        {
            var byteCount = elementCount * (uint)Unsafe.SizeOf<T>();
            var alignment = NativeMemoryAllocator.AlignOf<T>();
            if (TryAlignedAllocZeroed(byteCount, alignment, out var voidPtr))
            {
                ptr = (T*)voidPtr;
                return true;
            }

            ptr = null;
            return false;
        }

        /// <summary>
        ///     Allocates an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAlignedAlloc(uint byteCount, uint alignment, out void* ptr)
        {
            ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);
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

        /// <summary>
        ///     Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAlignedAllocZeroed(uint byteCount, uint alignment, out void* ptr)
        {
            if (!TryAlignedAlloc(byteCount, alignment, out ptr))
                return false;
            SpanHelpers.Set(ref Unsafe.AsRef<byte>(ptr), 0, byteCount);
            return true;
        }

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(Buffer), Length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start) => MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(start)), Length - start);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(start)), length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(Buffer), Length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(start)), Length - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(start)), length);

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        /// <returns>Pointer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte*(UnsafeMemoryLinearAllocator value) => value.Buffer;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(value))]
        public static implicit operator UnsafeMemoryLinearAllocator([MustBePinned] Span<byte> value) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), value.Length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(UnsafeMemoryLinearAllocator value) => value.AsSpan();

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(value))]
        public static implicit operator UnsafeMemoryLinearAllocator([MustBePinned] ReadOnlySpan<byte> value) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), value.Length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(UnsafeMemoryLinearAllocator value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeMemoryLinearAllocator Empty => default;
    }
}