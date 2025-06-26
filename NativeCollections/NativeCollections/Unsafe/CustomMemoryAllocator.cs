using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET7_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Custom memory allocator
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public readonly unsafe struct CustomMemoryAllocator : IEquatable<CustomMemoryAllocator>
    {
        /// <summary>
        ///     User
        /// </summary>
        private readonly void* _user;

        /// <summary>Allocates an aligned block of memory of the specified size and alignment, in bytes.</summary>
        private readonly delegate* managed<void*, uint, uint, void*> _alignedAlloc;

        /// <summary>Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.</summary>
        private readonly delegate* managed<void*, uint, uint, void*> _alignedAllocZeroed;

        /// <summary>Frees an aligned block of memory.</summary>
        private readonly delegate* managed<void*, void*, void> _alignedFree;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CustomMemoryAllocator(void* user, delegate* managed<void*, uint, uint, void*> alignedAlloc, delegate* managed<void*, uint, uint, void*> alignedAllocZeroed, delegate* managed<void*, void*, void> alignedFree)
        {
            _user = user;
            _alignedAlloc = alignedAlloc;
            _alignedAllocZeroed = alignedAllocZeroed;
            _alignedFree = alignedFree;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="callbacks">Callbacks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CustomMemoryAllocator(void* user, CustomMemoryCallbacks* callbacks)
        {
            _user = user;
            _alignedAlloc = callbacks->AlignedAlloc;
            _alignedAllocZeroed = callbacks->AlignedAllocZeroed;
            _alignedFree = callbacks->AlignedFree;
        }

        /// <summary>
        ///     User
        /// </summary>
        public void* User => _user;

        /// <summary>
        ///     Callbacks
        /// </summary>
        public CustomMemoryCallbacks Callbacks => new(_alignedAlloc, _alignedAllocZeroed, _alignedFree);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(CustomMemoryAllocator other)
        {
#if NET7_0_OR_GREATER
            if (sizeof(nint) == 8 && Vector256.IsHardwareAccelerated)
                return Vector256.LoadUnsafe(ref Unsafe.As<CustomMemoryAllocator, byte>(ref Unsafe.AsRef(in this))) == Vector256.LoadUnsafe(ref Unsafe.As<CustomMemoryAllocator, byte>(ref other));
            if (sizeof(nint) == 4 && Vector128.IsHardwareAccelerated)
                return Vector128.LoadUnsafe(ref Unsafe.As<CustomMemoryAllocator, byte>(ref Unsafe.AsRef(in this))) == Vector128.LoadUnsafe(ref Unsafe.As<CustomMemoryAllocator, byte>(ref other));
#endif
            ref var left = ref Unsafe.As<CustomMemoryAllocator, nint>(ref Unsafe.AsRef(in this));
            ref var right = ref Unsafe.As<CustomMemoryAllocator, nint>(ref other);
            return left == right && Unsafe.Add(ref left, (nint)1) == Unsafe.Add(ref right, (nint)1) && Unsafe.Add(ref left, (nint)2) == Unsafe.Add(ref right, (nint)2) && Unsafe.Add(ref left, (nint)3) == Unsafe.Add(ref right, (nint)3);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is CustomMemoryAllocator other && Equals(other);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "UnsafeMemoryAllocator";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(CustomMemoryAllocator left, CustomMemoryAllocator right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(CustomMemoryAllocator left, CustomMemoryAllocator right) => !left.Equals(right);

        /// <summary>Allocates an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="elementCount">The count, in elements, of the block to allocate.</param>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* AlignedAlloc<T>(uint elementCount) where T : unmanaged
        {
            var byteCount = elementCount * (uint)sizeof(T);
            var alignment = (uint)NativeMemoryAllocator.AlignOf<T>();
            return (T*)AlignedAlloc(byteCount, alignment);
        }

        /// <summary>Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="elementCount">The count, in elements, of the block to allocate.</param>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* AlignedAllocZeroed<T>(uint elementCount) where T : unmanaged
        {
            var byteCount = elementCount * (uint)sizeof(T);
            var alignment = (uint)NativeMemoryAllocator.AlignOf<T>();
            return (T*)AlignedAllocZeroed(byteCount, alignment);
        }

        /// <summary>Allocates an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <param name="alignment">The alignment, in bytes, of the block to allocate. This must be a power of <c>2</c>.</param>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* AlignedAlloc(uint byteCount, uint alignment) => _alignedAlloc(_user, byteCount, alignment);

        /// <summary>Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <param name="alignment">The alignment, in bytes, of the block to allocate. This must be a power of <c>2</c>.</param>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* AlignedAllocZeroed(uint byteCount, uint alignment) => _alignedAllocZeroed(_user, byteCount, alignment);

        /// <summary>Frees an aligned block of memory.</summary>
        /// <param name="ptr">A pointer to the aligned block of memory that should be freed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AlignedFree(void* ptr) => _alignedFree(_user, ptr);

        /// <summary>
        ///     Default
        /// </summary>
        public static CustomMemoryAllocator Default
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return new CustomMemoryAllocator(null, &AlignedAlloc, &AlignedAllocZeroed, &AlignedFree);

                static void* AlignedAlloc(void* user, uint byteCount, uint alignment) => NativeMemoryAllocator.AlignedAlloc(byteCount, alignment);
                static void* AlignedAllocZeroed(void* user, uint byteCount, uint alignment) => NativeMemoryAllocator.AlignedAllocZeroed(byteCount, alignment);
                static void AlignedFree(void* user, void* ptr) => NativeMemoryAllocator.AlignedFree(ptr);
            }
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static CustomMemoryAllocator Empty => new();
    }
}