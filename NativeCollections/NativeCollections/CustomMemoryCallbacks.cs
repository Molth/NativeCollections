using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Custom memory callbacks
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct CustomMemoryCallbacks : IEquatable<CustomMemoryCallbacks>
    {
        /// <summary>
        ///     Alloc
        /// </summary>
        public delegate* managed<void*, uint, void*> Alloc;

        /// <summary>
        ///     AllocZeroed
        /// </summary>
        public delegate* managed<void*, uint, void*> AllocZeroed;

        /// <summary>
        ///     Free
        /// </summary>
        public delegate* managed<void*, void*, void> Free;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="alloc">Alloc</param>
        /// <param name="allocZeroed">AllocZeroed</param>
        /// <param name="free">Free</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CustomMemoryCallbacks(delegate* managed<void*, uint, void*> alloc, delegate* managed<void*, uint, void*> allocZeroed, delegate* managed<void*, void*, void> free)
        {
            Alloc = alloc;
            AllocZeroed = allocZeroed;
            Free = free;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(CustomMemoryCallbacks other)
        {
            ref var left = ref Unsafe.As<CustomMemoryCallbacks, nint>(ref Unsafe.AsRef(in this));
            ref var right = ref Unsafe.As<CustomMemoryCallbacks, nint>(ref other);
            return left == right && Unsafe.Add(ref left, 1) == Unsafe.Add(ref right, 1) && Unsafe.Add(ref left, 2) == Unsafe.Add(ref right, 2);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is CustomMemoryCallbacks customMemoryCallbacks && Equals(customMemoryCallbacks);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "CustomMemoryCallbacks";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(CustomMemoryCallbacks left, CustomMemoryCallbacks right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(CustomMemoryCallbacks left, CustomMemoryCallbacks right) => !left.Equals(right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static CustomMemoryCallbacks Empty => new();
    }
}