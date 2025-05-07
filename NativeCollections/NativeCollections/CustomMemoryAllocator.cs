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

        /// <summary>
        ///     Alloc
        /// </summary>
        private readonly delegate* managed<void*, uint, void*> _alloc;

        /// <summary>
        ///     AllocZeroed
        /// </summary>
        private readonly delegate* managed<void*, uint, void*> _allocZeroed;

        /// <summary>
        ///     Free
        /// </summary>
        private readonly delegate* managed<void*, void*, void> _free;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="alloc">Alloc</param>
        /// <param name="allocZeroed">AllocZeroed</param>
        /// <param name="free">Free</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CustomMemoryAllocator(void* user, delegate* managed<void*, uint, void*> alloc, delegate* managed<void*, uint, void*> allocZeroed, delegate* managed<void*, void*, void> free)
        {
            _user = user;
            _alloc = alloc;
            _allocZeroed = allocZeroed;
            _free = free;
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
            _alloc = callbacks->Alloc;
            _allocZeroed = callbacks->AllocZeroed;
            _free = callbacks->Free;
        }

        /// <summary>
        ///     User
        /// </summary>
        public void* User => _user;

        /// <summary>
        ///     Callbacks
        /// </summary>
        public CustomMemoryCallbacks Callbacks => new(_alloc, _allocZeroed, _free);

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
            return left == right && Unsafe.Add(ref left, 1) == Unsafe.Add(ref right, 1) && Unsafe.Add(ref left, 2) == Unsafe.Add(ref right, 2) && Unsafe.Add(ref left, 3) == Unsafe.Add(ref right, 3);
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

        /// <summary>
        ///     Alloc
        /// </summary>
        /// <param name="byteCount">Byte count</param>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Alloc(uint byteCount) => _alloc(_user, byteCount);

        /// <summary>
        ///     Alloc zeroed
        /// </summary>
        /// <param name="byteCount">Byte count</param>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* AllocZeroed(uint byteCount) => _allocZeroed(_user, byteCount);

        /// <summary>
        ///     Free
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Free(void* ptr) => _free(_user, ptr);

        /// <summary>
        ///     Empty
        /// </summary>
        public static CustomMemoryAllocator Empty => new();
    }
}