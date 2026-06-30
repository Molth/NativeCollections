using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native bit array slot
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeBitArraySlot : IIsCreated, IEquatable<NativeBitArraySlot>
    {
        /// <summary>
        ///     Segment
        /// </summary>
        private readonly int* _segment;

        /// <summary>
        ///     BitMask
        /// </summary>
        private readonly int _bitMask;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_segment);

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="segment">Segment</param>
        /// <param name="bitMask">BitMask</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArraySlot(int* segment, int bitMask)
        {
            _segment = segment;
            _bitMask = bitMask;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeBitArraySlot other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeBitArraySlot other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeBitArraySlot";

        /// <summary>
        ///     Get
        /// </summary>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get() => (Unsafe.AsRef<int>(_segment) & _bitMask) != 0;

        /// <summary>
        ///     Set
        /// </summary>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(bool value)
        {
            if (value)
                Unsafe.AsRef<int>(_segment) |= _bitMask;
            else
                Unsafe.AsRef<int>(_segment) &= ~_bitMask;
        }

        /// <summary>
        ///     As bool
        /// </summary>
        /// <returns>Boolean</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(NativeBitArraySlot unsafeBitArraySlot) => unsafeBitArraySlot.Get();

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeBitArraySlot left, NativeBitArraySlot right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeBitArraySlot left, NativeBitArraySlot right) => !left.Equals(right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeBitArraySlot Empty => new();
    }
}