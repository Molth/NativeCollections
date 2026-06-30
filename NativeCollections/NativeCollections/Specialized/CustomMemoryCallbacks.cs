using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Custom memory callbacks
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct CustomMemoryCallbacks : IEquatable<CustomMemoryCallbacks>
    {
        /// <summary>Allocates an aligned block of memory of the specified size and alignment, in bytes.</summary>
        public readonly delegate* managed<void*, uint, uint, void*> AlignedAlloc;

        /// <summary>Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.</summary>
        public readonly delegate* managed<void*, uint, uint, void*> AlignedAllocZeroed;

        /// <summary>Frees an aligned block of memory.</summary>
        public readonly delegate* managed<void*, void*, void> AlignedFree;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CustomMemoryCallbacks(delegate* managed<void*, uint, uint, void*> alignedAlloc, delegate* managed<void*, uint, uint, void*> alignedAllocZeroed, delegate* managed<void*, void*, void> alignedFree)
        {
            AlignedAlloc = alignedAlloc;
            AlignedAllocZeroed = alignedAllocZeroed;
            AlignedFree = alignedFree;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(CustomMemoryCallbacks other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is CustomMemoryCallbacks other && other.Equals(this);

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