using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a wrapper around an unmanaged type that enables bitwise comparison
    ///     and hashing of its binary representation.
    /// </summary>
    /// <remarks>
    ///     https://github.com/dotnet/dotNext/blob/master/src/DotNext/BitwiseComparer.cs
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community)]
    public unsafe struct UnsafeBitwise<T> : IEquatable<UnsafeBitwise<T>>, IComparable<UnsafeBitwise<T>>, IEquatable<T>, IComparable<T> where T : unmanaged
    {
        /// <summary>
        ///     Gets the value to the underlying object.
        /// </summary>
        public T Value;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeBitwise<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the other
        ///     object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///     <list type="table">
        ///         <listheader>
        ///             <term> Value</term><description> Meaning</description>
        ///         </listheader>
        ///         <item>
        ///             <term> Less than zero</term>
        ///             <description> This instance precedes <paramref name="other" /> in the sort order.</description>
        ///         </item>
        ///         <item>
        ///             <term> Zero</term>
        ///             <description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description>
        ///         </item>
        ///         <item>
        ///             <term> Greater than zero</term>
        ///             <description> This instance follows <paramref name="other" /> in the sort order.</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public readonly int CompareTo(UnsafeBitwise<T> other) => SpanHelpers.Compare(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(T other) => SpanHelpers.Equals(ref Unsafe.AsRef(in Value), ref other);

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the other
        ///     object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///     <list type="table">
        ///         <listheader>
        ///             <term> Value</term><description> Meaning</description>
        ///         </listheader>
        ///         <item>
        ///             <term> Less than zero</term>
        ///             <description> This instance precedes <paramref name="other" /> in the sort order.</description>
        ///         </item>
        ///         <item>
        ///             <term> Zero</term>
        ///             <description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description>
        ///         </item>
        ///         <item>
        ///             <term> Greater than zero</term>
        ///             <description> This instance follows <paramref name="other" /> in the sort order.</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public readonly int CompareTo(T other) => SpanHelpers.Compare(ref Unsafe.AsRef(in Value), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(ReadOnlySpan<byte> other) => AsReadOnlySpan().SequenceEqual(other);

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the other
        ///     object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///     <list type="table">
        ///         <listheader>
        ///             <term> Value</term><description> Meaning</description>
        ///         </listheader>
        ///         <item>
        ///             <term> Less than zero</term>
        ///             <description> This instance precedes <paramref name="other" /> in the sort order.</description>
        ///         </item>
        ///         <item>
        ///             <term> Zero</term>
        ///             <description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description>
        ///         </item>
        ///         <item>
        ///             <term> Greater than zero</term>
        ///             <description> This instance follows <paramref name="other" /> in the sort order.</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public readonly int CompareTo(ReadOnlySpan<byte> other) => AsReadOnlySpan().SequenceCompareTo(other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(AsReadOnlySpan());

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeBitwise<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public Span<byte> AsSpan() => MemoryMarshalHelpers.AsBytes(ref Value);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshalHelpers.AsReadOnlyBytes(ref Unsafe.AsRef(in Value));

        /// <summary>
        ///     Copies the element of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(UnsafeBitwise<T> value) => Unsafe.As<UnsafeBitwise<T>, T>(ref value);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafeBitwise<T>(T value) => Unsafe.As<T, UnsafeBitwise<T>>(ref value);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeBitwise<T> left, UnsafeBitwise<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeBitwise<T> left, UnsafeBitwise<T> right) => !left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeBitwise<T> left, T right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeBitwise<T> left, T right) => !left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(T left, UnsafeBitwise<T> right) => right.Equals(left);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(T left, UnsafeBitwise<T> right) => !right.Equals(left);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeBitwise<T> left, ReadOnlySpan<byte> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeBitwise<T> left, ReadOnlySpan<byte> right) => !left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(ReadOnlySpan<byte> left, UnsafeBitwise<T> right) => right.Equals(left);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(ReadOnlySpan<byte> left, UnsafeBitwise<T> right) => !right.Equals(left);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeBitwise<T> Empty => default;
    }
}