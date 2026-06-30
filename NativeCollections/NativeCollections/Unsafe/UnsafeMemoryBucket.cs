using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe memory bucket
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeMemoryBucket : IIsCreated, IDisposable, IEquatable<UnsafeMemoryBucket>
    {
        /// <summary>
        ///     Capacity
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Alignment
        /// </summary>
        private readonly int _alignment;

        /// <summary>
        ///     Buffer
        /// </summary>
        [NativePointer(typeof(void*))] private readonly nint* _buffer;

        /// <summary>
        ///     Index
        /// </summary>
        private int _index;

        /// <summary>
        ///     Memory allocator
        /// </summary>
        private readonly CustomMemoryAllocator _allocator;

        /// <summary>
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Capacity
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Length
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Alignment
        /// </summary>
        public readonly int Alignment => _alignment;

        /// <summary>
        ///     Memory allocator
        /// </summary>
        public readonly CustomMemoryAllocator Allocator => _allocator;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="length">Length</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryBucket(int capacity, int length, int alignment) : this(capacity, length, alignment, CustomMemoryAllocator.Default)
        {
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="length">Length</param>
        /// <param name="alignment">Alignment</param>
        /// <param name="allocator">Memory allocator</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryBucket(int capacity, int length, int alignment, CustomMemoryAllocator allocator)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            _capacity = capacity;
            _length = length;
            _alignment = alignment;
            _buffer = NativeMemoryAllocator.AlignedAllocZeroed<nint>((uint)capacity);
            _index = 0;
            _allocator = allocator;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeMemoryBucket other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryBucket other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => "UnsafeMemoryBucket";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeMemoryBucket left, UnsafeMemoryBucket right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeMemoryBucket left, UnsafeMemoryBucket right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {
            for (var i = _index; i < _capacity; ++i)
            {
                var buffer = (void*)Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)i);
                if (UnsafeHelpers.IsNull(buffer))
                    break;
                _allocator.AlignedFree(buffer);
            }

            NativeMemoryAllocator.AlignedFree(_buffer);
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent()
        {
            void* ptr = null;
            if (_index < _capacity)
            {
                ptr = (void*)Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)_index);
                Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)_index++) = 0;
            }

            if (UnsafeHelpers.IsNull(ptr))
                ptr = _allocator.AlignedAlloc((uint)_length, (uint)_alignment);
            return ptr;
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            if (_index != 0)
                Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)(--_index)) = (nint)ptr;
            else
                _allocator.AlignedFree(ptr);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeMemoryBucket Empty => new();
    }
}