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
    ///     https://github.com/dotnet/dotNext/blob/master/src/DotNext/BitwiseComparer.cs
    /// </summary>
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
        ///     Equals
        /// </summary>
        public readonly bool Equals(UnsafeBitwise<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Compare to
        /// </summary>
        public readonly int CompareTo(UnsafeBitwise<T> other) => SpanHelpers.Compare(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        public readonly bool Equals(T other) => SpanHelpers.Equals(ref Unsafe.AsRef(in Value), ref other);

        /// <summary>
        ///     Compare to
        /// </summary>
        public readonly int CompareTo(T other) => SpanHelpers.Compare(ref Unsafe.AsRef(in Value), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        public readonly bool Equals(ReadOnlySpan<byte> other) => AsReadOnlySpan().SequenceEqual(other);

        /// <summary>
        ///     Compare to
        /// </summary>
        public readonly int CompareTo(ReadOnlySpan<byte> other) => AsReadOnlySpan().SequenceCompareTo(other);

        /// <summary>
        ///     Equals
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
        ///     To string
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeBitwise<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     As span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref Value), Unsafe.SizeOf<T>());

        /// <summary>
        ///     As readOnly span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in Value)), Unsafe.SizeOf<T>());

        /// <summary>
        ///     As value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(UnsafeBitwise<T> value) => Unsafe.As<UnsafeBitwise<T>, T>(ref value);

        /// <summary>
        ///     As native bitwise
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafeBitwise<T>(T value) => Unsafe.As<T, UnsafeBitwise<T>>(ref value);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(UnsafeBitwise<T> left, UnsafeBitwise<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(UnsafeBitwise<T> left, UnsafeBitwise<T> right) => !left.Equals(right);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(UnsafeBitwise<T> left, T right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(UnsafeBitwise<T> left, T right) => !left.Equals(right);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(T left, UnsafeBitwise<T> right) => right.Equals(left);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(T left, UnsafeBitwise<T> right) => !right.Equals(left);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(UnsafeBitwise<T> left, ReadOnlySpan<byte> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(UnsafeBitwise<T> left, ReadOnlySpan<byte> right) => !left.Equals(right);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(ReadOnlySpan<byte> left, UnsafeBitwise<T> right) => right.Equals(left);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(ReadOnlySpan<byte> left, UnsafeBitwise<T> right) => !right.Equals(left);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeBitwise<T> Empty => new();
    }
}