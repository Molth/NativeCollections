using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a set of function pointers for custom aligned memory allocation and deallocation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SpecializedCollection(FromType.Standard)]
    public readonly unsafe struct CustomMemoryCallbacks : IEquatable<CustomMemoryCallbacks>
    {
        /// <summary>
        ///     Allocates an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        public readonly delegate* managed<void*, uint, uint, void*> AlignedAlloc;

        /// <summary>
        ///     Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        public readonly delegate* managed<void*, uint, uint, void*> AlignedAllocZeroed;

        /// <summary>
        ///     Frees an aligned block of memory.
        /// </summary>
        public readonly delegate* managed<void*, void*, void> AlignedFree;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CustomMemoryCallbacks(delegate* managed<void*, uint, uint, void*> alignedAlloc, delegate* managed<void*, uint, uint, void*> alignedAllocZeroed, delegate* managed<void*, void*, void> alignedFree)
        {
            AlignedAlloc = alignedAlloc;
            AlignedAllocZeroed = alignedAllocZeroed;
            AlignedFree = alignedFree;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(CustomMemoryCallbacks other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is CustomMemoryCallbacks other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "CustomMemoryCallbacks";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(CustomMemoryCallbacks left, CustomMemoryCallbacks right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(CustomMemoryCallbacks left, CustomMemoryCallbacks right) => !left.Equals(right);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static CustomMemoryCallbacks Empty => default;
    }
}