using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
        ///     Alloc
        /// </summary>
        public readonly delegate* managed<int, uint, void*> Malloc;

        /// <summary>
        ///     AllocZeroed
        /// </summary>
        public readonly delegate* managed<int, uint, void*> Calloc;

        /// <summary>
        ///     Free
        /// </summary>
        public readonly delegate* managed<int, void*, void> Free;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="malloc">Alloc</param>
        /// <param name="calloc">AllocZeroed</param>
        /// <param name="free">Free</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CustomMemoryAllocator(delegate* managed<int, uint, void*> malloc, delegate* managed<int, uint, void*> calloc, delegate* managed<int, void*, void> free)
        {
            Malloc = malloc;
            Calloc = calloc;
            Free = free;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(CustomMemoryAllocator other)
        {
            ref var left = ref Unsafe.As<CustomMemoryAllocator, nint>(ref Unsafe.AsRef(in this));
            ref var right = ref Unsafe.As<CustomMemoryAllocator, nint>(ref other);
            return left == right && Unsafe.Add(ref left, 1) == Unsafe.Add(ref right, 1) && Unsafe.Add(ref left, 2) == Unsafe.Add(ref right, 2);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is CustomMemoryAllocator customMemoryAllocator && Equals(customMemoryAllocator);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => HashCode.Combine((nint)Malloc, (nint)Calloc, (nint)Free);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "CustomMemoryAllocator";

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
        ///     Empty
        /// </summary>
        public static CustomMemoryAllocator Empty => new();
    }
}