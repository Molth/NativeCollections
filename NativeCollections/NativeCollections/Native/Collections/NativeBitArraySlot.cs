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
        public bool IsCreated => _segment != null;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="segment">A pointer to the integer containing the target bit.</param>
        /// <param name="bitMask">The mask used to isolate the target bit.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeBitArraySlot(int* segment, int bitMask)
        {
            _segment = segment;
            _bitMask = bitMask;
        }

        /// <summary>
        ///     Creates a bit slot over an existing native segment.
        /// </summary>
        /// <param name="segment">A pointer to the integer containing the target bit.</param>
        /// <param name="bitMask">The mask used to isolate the target bit.</param>
        /// <returns>A bit slot that wraps <paramref name="segment" /> using <paramref name="bitMask" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeBitArraySlot From(int* segment, int bitMask) => new(segment, bitMask);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeBitArraySlot other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeBitArraySlot other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeBitArraySlot";

        /// <summary>
        ///     Gets the value of the bit at a specific position in this.
        /// </summary>
        /// <returns><see langword="true" /> if the bit is set; otherwise, <see langword="false" />.</returns>
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
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeBitArraySlot left, NativeBitArraySlot right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeBitArraySlot left, NativeBitArraySlot right) => !left.Equals(right);
    }
}