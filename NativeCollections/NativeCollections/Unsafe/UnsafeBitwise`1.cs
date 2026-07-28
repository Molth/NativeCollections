using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe bitwise
    /// </summary>
    /// <remarks>
    ///     https://github.com/dotnet/dotNext/blob/master/src/DotNext/BitwiseComparer.cs
    /// </remarks>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community)]
    public struct UnsafeBitwise<T> : IEquatable<UnsafeBitwise<T>>, IComparable<UnsafeBitwise<T>>, IEquatable<T>, IComparable<T> where T : unmanaged
    {
        /// <summary>
        ///     Value
        /// </summary>
        public T Value;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeBitwise<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Compare to
        /// </summary>
        public readonly int CompareTo(UnsafeBitwise<T> other) => SpanHelpers.Compare(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(T other) => SpanHelpers.Equals(ref Unsafe.AsRef(in Value), ref other);

        /// <summary>
        ///     Compare to
        /// </summary>
        public readonly int CompareTo(T other) => SpanHelpers.Compare(ref Unsafe.AsRef(in Value), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(ReadOnlySpan<byte> other) => AsReadOnlySpan().SequenceEqual(other);

        /// <summary>
        ///     Compare to
        /// </summary>
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
        ///     Get hashCode
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
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref Value), Unsafe.SizeOf<T>());

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in Value)), Unsafe.SizeOf<T>());

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
        ///     Empty
        /// </summary>
        public static UnsafeBitwise<T> Empty => default;

        /// <summary>
        ///     Determines whether two values are equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(ref T left, ref T right) => SpanHelpers.Equals(ref left, ref right);

        /// <summary>
        ///     Determines the relative order of the values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(ref T left, ref T right) => SpanHelpers.Compare(ref left, ref right);
    }
}