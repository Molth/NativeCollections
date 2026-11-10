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
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public readonly byte* Buffer;

        /// <summary>
        ///     Gets the size of the buffer in bytes.
        /// </summary>
        public readonly int Length;

        /// <summary>
        ///     Gets the current position within the buffer.
        /// </summary>
        private int _position;

        /// <summary>
        ///     Initializes a new instance of this class with the specified buffer and length.
        /// </summary>
        /// <param name="buffer">Represents a contiguous region of arbitrary memory.</param>
        /// <param name="length">
        ///     The size of the buffer in bytes. Must be non-negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeMemoryLinearAllocator(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            Buffer = buffer;
            Length = length;
            _position = 0;
        }

        /// <summary>
        ///     Creates an allocator that takes ownership of the specified native buffer.
        /// </summary>
        /// <param name="buffer">Represents a contiguous region of arbitrary memory.</param>
        /// <param name="length">The size of the buffer in bytes. Must be non-negative.</param>
        /// <returns>
        ///     An allocator over <paramref name="buffer" /> whose total size is
        ///     <paramref name="length" /> bytes and whose current position is zero.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        /// <remarks>
        ///     The returned allocator owns <paramref name="buffer" />; <see cref="Dispose" /> calls
        ///     <see cref="NativeMemoryAllocator.AlignedFree" /> on it. Therefore, <paramref name="buffer" />
        ///     must be allocated by <see cref="NativeMemoryAllocator" /> or a compatible allocator, or the
        ///     caller must release it separately and must not call <see cref="Dispose" /> on the returned
        ///     allocator.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeMemoryLinearAllocator From(byte* buffer, int length) => new(buffer, length);

        /// <summary>
        ///     Creates a new instance that references the specified span.
        /// </summary>
        /// <param name="value">The span to reference as the allocator buffer.</param>
        /// <returns>
        ///     An allocator over <paramref name="value" /> whose total
        ///     size is the span length and whose current position is zero.
        /// </returns>
        /// <remarks>
        ///     The span must remain pinned and valid for the lifetime of the allocator. Do not call
        ///     <see cref="Dispose" /> on the returned allocator: it calls
        ///     <see cref="NativeMemoryAllocator.AlignedFree" /> on the span's address, which is not
        ///     necessarily memory allocated by <see cref="NativeMemoryAllocator" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeMemoryLinearAllocator From([MustBePinned] Span<byte> value) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), value.Length);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(Buffer);

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => Buffer != null;

        /// <summary>
        ///     Gets the current position within the buffer.
        /// </summary>
        public readonly int Position => _position;

        /// <summary>
        ///     Gets the number of remaining bytes in the buffer.
        /// </summary>
        public readonly int Remaining => Length.WrappingSub(_position);

        /// <summary>
        ///     Gets a pointer to the byte at the specified byte offset.
        /// </summary>
        /// <param name="index">The byte offset from the start of the buffer at which to access a byte.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Length" />.
        /// </exception>
        public readonly byte* this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(Length), ExceptionArgument.index);
                return UnsafeHelpers.AddByteOffset<byte>(Buffer, Isize.FromI32Unchecked(index));
            }
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeMemoryLinearAllocator other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryLinearAllocator other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeMemoryLinearAllocator";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeMemoryLinearAllocator left, UnsafeMemoryLinearAllocator right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeMemoryLinearAllocator left, UnsafeMemoryLinearAllocator right) => !left.Equals(right);

        /// <summary>
        ///     Advances the current position by <paramref name="count" /> bytes.
        /// </summary>
        /// <param name="count">The number of bytes by which to advance the position.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="count" /> is negative or the resulting position
        ///     would exceed <see cref="Length" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            var newPosition = _position.WrappingAdd(count);
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(newPosition), U32.FromI32Unchecked(Length), ExceptionArgument.count);
            _position = newPosition;
        }

        /// <summary>
        ///     Attempts to advance the current position by <paramref name="count" /> bytes.
        /// </summary>
        /// <param name="count">The number of bytes by which to advance the position.</param>
        /// <returns>
        ///     <see langword="true" /> if the position was advanced; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     A negative <paramref name="count" /> is rejected: the method returns
        ///     <see langword="false" /> and the position is not changed.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdvance(int count)
        {
            if (count < 0)
                return false;

            var newPosition = _position.WrappingAdd(count);
            if (U32.FromI32Unchecked(newPosition) > U32.FromI32Unchecked(Length))
                return false;

            _position = newPosition;
            return true;
        }

        /// <summary>
        ///     Sets the current position within the buffer.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the position is set to a negative value or a value greater than <see cref="Length" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(int position)
        {
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(position), U32.FromI32Unchecked(Length), ExceptionArgument.position);
            _position = position;
        }

        /// <summary>
        ///     Attempts to set the current position within the buffer.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <returns><see langword="true" /> if the position was set; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetPosition(int position)
        {
            if (U32.FromI32Unchecked(position) > U32.FromI32Unchecked(Length))
                return false;

            _position = position;
            return true;
        }

        /// <summary>
        ///     Allocates an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type to allocate.</typeparam>
        /// <param name="elementCount">The number of elements to allocate.</param>
        /// <param name="ptr">
        ///     When this method returns, contains a pointer to the allocated
        ///     block, or <see langword="null" /> if the allocation failed.
        /// </param>
        /// <returns><see langword="true" /> if the allocation succeeded; otherwise, <see langword="false" />.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the byte count required for <paramref name="elementCount" /> elements
        ///     exceeds the maximum native-sized value.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAlignedAlloc<T>(nuint elementCount, out T* ptr) where T : unmanaged
        {
            var byteCount = elementCount * Usize.FromI32Unchecked(Unsafe.SizeOf<T>());
            if (TryAlignedAlloc(byteCount, NativeMemoryAllocator.AlignOf<T>(), out var voidPtr))
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
        /// <typeparam name="T">The unmanaged element type to allocate.</typeparam>
        /// <param name="elementCount">The number of elements to allocate.</param>
        /// <param name="ptr">
        ///     When this method returns, contains a pointer to the allocated and
        ///     zeroed block, or <see langword="null" /> if the allocation failed.
        /// </param>
        /// <returns><see langword="true" /> if the allocation succeeded; otherwise, <see langword="false" />.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the byte count required for <paramref name="elementCount" /> elements
        ///     exceeds the maximum native-sized value.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAlignedAllocZeroed<T>(nuint elementCount, out T* ptr) where T : unmanaged
        {
            var byteCount = elementCount * Usize.FromI32Unchecked(Unsafe.SizeOf<T>());
            if (TryAlignedAllocZeroed(byteCount, NativeMemoryAllocator.AlignOf<T>(), out var voidPtr))
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
        /// <param name="byteCount">The number of bytes to allocate.</param>
        /// <param name="alignment">The required alignment in bytes. The value must be a power of two.</param>
        /// <param name="ptr">
        ///     When this method returns, contains a pointer to the allocated
        ///     block, or <see langword="null" /> if the allocation failed.
        /// </param>
        /// <returns><see langword="true" /> if the allocation succeeded; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAlignedAlloc(nuint byteCount, nuint alignment, out void* ptr)
        {
            ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);
            var bufferAddress = Usize.FromIsizeUnchecked(Isize.FromPtrUnchecked(Buffer));
            var currentAddress = bufferAddress.WrappingAdd(Usize.FromI32Unchecked(_position));
            var position = NativeMemoryAllocator.AlignUp(currentAddress, alignment);
            var bufferEnd = bufferAddress.WrappingAdd(Usize.FromI32Unchecked(Length));
            if (position < currentAddress || position > bufferEnd || byteCount > bufferEnd.WrappingSub(position))
            {
                ptr = null;
                return false;
            }

            ptr = (void*)Isize.FromUsizeUnchecked(position);
            _position = I32.FromUsizeUnchecked(position.WrappingAdd(byteCount).WrappingSub(bufferAddress));
            return true;
        }

        /// <summary>
        ///     Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        /// <param name="byteCount">The number of bytes to allocate.</param>
        /// <param name="alignment">The required alignment in bytes. The value must be a power of two.</param>
        /// <param name="ptr">
        ///     When this method returns, contains a pointer to the allocated and
        ///     zeroed block, or <see langword="null" /> if the allocation failed.
        /// </param>
        /// <returns><see langword="true" /> if the allocation succeeded; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAlignedAllocZeroed(nuint byteCount, nuint alignment, out void* ptr)
        {
            if (!TryAlignedAlloc(byteCount, alignment, out ptr))
                return false;

            SpanHelpers.Set(ref Unsafe.AsRef<byte>(ptr), U8.FromI32Unchecked(0), byteCount);
            return true;
        }

        /// <summary>
        ///     Returns a span over the entire contents of this.
        /// </summary>
        /// <returns>A span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(Buffer), Length);

        /// <summary>
        ///     Returns a read-only span over the entire contents of this.
        /// </summary>
        /// <returns>A read-only span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(Buffer), Length);
    }
}