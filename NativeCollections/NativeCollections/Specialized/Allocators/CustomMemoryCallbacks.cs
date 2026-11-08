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
        public readonly delegate* managed<void*, nuint, nuint, void*> AlignedAlloc;

        /// <summary>
        ///     Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        public readonly delegate* managed<void*, nuint, nuint, void*> AlignedAllocZeroed;

        /// <summary>
        ///     Frees an aligned block of memory.
        /// </summary>
        public readonly delegate* managed<void*, void*, void> AlignedFree;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="alignedAlloc">The callback used to allocate aligned memory.</param>
        /// <param name="alignedAllocZeroed">The callback used to allocate zeroed aligned memory.</param>
        /// <param name="alignedFree">The callback used to free aligned memory.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CustomMemoryCallbacks(delegate* managed<void*, nuint, nuint, void*> alignedAlloc, delegate* managed<void*, nuint, nuint, void*> alignedAllocZeroed, delegate* managed<void*, void*, void> alignedFree)
        {
            AlignedAlloc = alignedAlloc;
            AlignedAllocZeroed = alignedAllocZeroed;
            AlignedFree = alignedFree;
        }

        /// <summary>
        ///     Creates a callback set from the specified function pointers.
        /// </summary>
        /// <param name="alignedAlloc">The callback used to allocate aligned memory.</param>
        /// <param name="alignedAllocZeroed">The callback used to allocate zeroed aligned memory.</param>
        /// <param name="alignedFree">The callback used to free aligned memory.</param>
        /// <returns>A callback set that uses the specified function pointers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CustomMemoryCallbacks From(delegate* managed<void*, nuint, nuint, void*> alignedAlloc, delegate* managed<void*, nuint, nuint, void*> alignedAllocZeroed, delegate* managed<void*, void*, void> alignedFree) => new(alignedAlloc, alignedAllocZeroed, alignedFree);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(CustomMemoryCallbacks other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is CustomMemoryCallbacks other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "CustomMemoryCallbacks";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(CustomMemoryCallbacks left, CustomMemoryCallbacks right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(CustomMemoryCallbacks left, CustomMemoryCallbacks right) => !left.Equals(right);
    }
}