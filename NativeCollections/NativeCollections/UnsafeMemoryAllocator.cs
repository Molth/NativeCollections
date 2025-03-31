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
    ///     Unsafe memory allocator
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public readonly unsafe struct UnsafeMemoryAllocator : IEquatable<UnsafeMemoryAllocator>
    {
        /// <summary>
        ///     User
        /// </summary>
        private readonly void* _user;

        /// <summary>
        ///     Alloc
        /// </summary>
        private readonly delegate* managed<void*, uint, void*> _malloc;

        /// <summary>
        ///     AllocZeroed
        /// </summary>
        private readonly delegate* managed<void*, uint, void*> _calloc;

        /// <summary>
        ///     Free
        /// </summary>
        private readonly delegate* managed<void*, void*, void> _free;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="malloc">Alloc</param>
        /// <param name="calloc">AllocZeroed</param>
        /// <param name="free">Free</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryAllocator(void* user, delegate* managed<void*, uint, void*> malloc, delegate* managed<void*, uint, void*> calloc, delegate* managed<void*, void*, void> free)
        {
            _user = user;
            _malloc = malloc;
            _calloc = calloc;
            _free = free;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(UnsafeMemoryAllocator other)
        {
#if NET7_0_OR_GREATER
            if (sizeof(nint) == 8 && Vector256.IsHardwareAccelerated)
                return Vector256.LoadUnsafe(ref Unsafe.As<UnsafeMemoryAllocator, byte>(ref Unsafe.AsRef(in this))) == Vector256.LoadUnsafe(ref Unsafe.As<UnsafeMemoryAllocator, byte>(ref other));
            if (sizeof(nint) == 4 && Vector128.IsHardwareAccelerated)
                return Vector128.LoadUnsafe(ref Unsafe.As<UnsafeMemoryAllocator, byte>(ref Unsafe.AsRef(in this))) == Vector128.LoadUnsafe(ref Unsafe.As<UnsafeMemoryAllocator, byte>(ref other));
#endif
            ref var left = ref Unsafe.As<UnsafeMemoryAllocator, nint>(ref Unsafe.AsRef(in this));
            ref var right = ref Unsafe.As<UnsafeMemoryAllocator, nint>(ref other);
            return left == right && Unsafe.Add(ref left, 1) == Unsafe.Add(ref right, 1) && Unsafe.Add(ref left, 2) == Unsafe.Add(ref right, 2) && Unsafe.Add(ref left, 3) == Unsafe.Add(ref right, 3);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is UnsafeMemoryAllocator other && Equals(other);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode()
        {
#if NET7_0_OR_GREATER
            if (sizeof(nint) == 8 && Vector256.IsHardwareAccelerated)
                return Vector256.LoadUnsafe(ref Unsafe.As<UnsafeMemoryAllocator, byte>(ref Unsafe.AsRef(in this))).GetHashCode();
            if (sizeof(nint) == 4 && Vector128.IsHardwareAccelerated)
                return Vector128.LoadUnsafe(ref Unsafe.As<UnsafeMemoryAllocator, byte>(ref Unsafe.AsRef(in this))).GetHashCode();
#endif
            return HashCode.Combine((nint)_user, (nint)_malloc, (nint)_calloc, (nint)_free);
        }

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
        public static bool operator ==(UnsafeMemoryAllocator left, UnsafeMemoryAllocator right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeMemoryAllocator left, UnsafeMemoryAllocator right) => !left.Equals(right);

        /// <summary>
        ///     Alloc
        /// </summary>
        /// <param name="byteCount">Byte count</param>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Alloc(uint byteCount) => _malloc(_user, byteCount);

        /// <summary>
        ///     Alloc zeroed
        /// </summary>
        /// <param name="byteCount">Byte count</param>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* AllocZeroed(uint byteCount) => _calloc(_user, byteCount);

        /// <summary>
        ///     Free
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Free(void* ptr) => _free(_user, ptr);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeMemoryAllocator Empty => new();
    }
}