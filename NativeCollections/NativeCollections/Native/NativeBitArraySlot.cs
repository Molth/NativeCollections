using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a reference to a single bit within a bit array, where <see langword="true" />
    ///     indicates the bit is set (1) and <see langword="false" /> indicates the bit is cleared (0).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeBitArraySlot : IIsCreated, IEquatable<NativeBitArraySlot>
    {
        /// <summary>
        ///     Pointer to the integer array containing the bit.
        /// </summary>
        private readonly int* _segment;

        /// <summary>
        ///     Bit mask used to isolate the target bit within the segment.
        /// </summary>
        private readonly int _bitMask;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_segment);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArraySlot(int* segment, int bitMask)
        {
            _segment = segment;
            _bitMask = bitMask;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeBitArraySlot other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeBitArraySlot other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeBitArraySlot";

        /// <summary>
        ///     Gets the value of the bit at a specific position in this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get() => (Unsafe.AsRef<int>(_segment) & _bitMask) != 0;

        /// <summary>
        ///     Sets the value of the bit at a specific position in this.
        /// </summary>
        /// <param name="value">The bool value to assign to the bit.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(bool value)
        {
            if (value)
                Unsafe.AsRef<int>(_segment) |= _bitMask;
            else
                Unsafe.AsRef<int>(_segment) &= ~_bitMask;
        }

        /// <summary>
        ///     Copies the element of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(NativeBitArraySlot value) => value.Get();

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeBitArraySlot left, NativeBitArraySlot right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeBitArraySlot left, NativeBitArraySlot right) => !left.Equals(right);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeBitArraySlot Empty => default;
    }
}