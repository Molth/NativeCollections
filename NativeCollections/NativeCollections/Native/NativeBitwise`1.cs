using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native bitwise
    ///     https://github.com/dotnet/dotNext/blob/master/src/DotNext/BitwiseComparer.cs
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Community)]
    public struct NativeBitwise<T> : IEquatable<NativeBitwise<T>>, IComparable<NativeBitwise<T>>, IEquatable<T>, IComparable<T> where T : unmanaged
    {
        /// <summary>
        ///     Value
        /// </summary>
        public T Value;

        /// <summary>
        ///     Equals
        /// </summary>
        public bool Equals(NativeBitwise<T> other)
        {
            var left = AsReadOnlySpan();
            var right = other.AsReadOnlySpan();
            return left.SequenceEqual(right);
        }

        /// <summary>
        ///     Compare to
        /// </summary>
        public int CompareTo(NativeBitwise<T> other)
        {
            var left = AsReadOnlySpan();
            var right = other.AsReadOnlySpan();
            return left.SequenceCompareTo(right);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        public bool Equals(T other)
        {
            var left = AsReadOnlySpan();
            var right = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref other), Unsafe.SizeOf<T>());
            return left.SequenceEqual(right);
        }

        /// <summary>
        ///     Compare to
        /// </summary>
        public int CompareTo(T other)
        {
            var left = AsReadOnlySpan();
            var right = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref other), Unsafe.SizeOf<T>());
            return left.SequenceCompareTo(right);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        public bool Equals(ReadOnlySpan<byte> other)
        {
            var left = AsReadOnlySpan();
            return left.SequenceEqual(other);
        }

        /// <summary>
        ///     Compare to
        /// </summary>
        public int CompareTo(ReadOnlySpan<byte> other)
        {
            var left = AsReadOnlySpan();
            return left.SequenceCompareTo(other);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        [Obsolete("Call this method will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Get hashCode
        /// </summary>
        public override int GetHashCode()
        {
            var left = AsReadOnlySpan();
            return NativeHashCode.GetHashCode(left);
        }

        /// <summary>
        ///     To string
        /// </summary>
        public readonly override string ToString() => $"NativeBitwise<{typeof(T).Name}>";

        /// <summary>
        ///     As span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref Value), Unsafe.SizeOf<T>());

        /// <summary>
        ///     As readOnly span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Value), Unsafe.SizeOf<T>());

        /// <summary>
        ///     As value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(NativeBitwise<T> value) => Unsafe.As<NativeBitwise<T>, T>(ref value);

        /// <summary>
        ///     As native bitwise
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeBitwise<T>(T value) => Unsafe.As<T, NativeBitwise<T>>(ref value);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(NativeBitwise<T> left, NativeBitwise<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(NativeBitwise<T> left, NativeBitwise<T> right) => !left.Equals(right);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(NativeBitwise<T> left, T right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(NativeBitwise<T> left, T right) => !left.Equals(right);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(T left, NativeBitwise<T> right) => right.Equals(left);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(T left, NativeBitwise<T> right) => !right.Equals(left);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(NativeBitwise<T> left, ReadOnlySpan<byte> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(NativeBitwise<T> left, ReadOnlySpan<byte> right) => !left.Equals(right);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(ReadOnlySpan<byte> left, NativeBitwise<T> right) => right.Equals(left);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(ReadOnlySpan<byte> left, NativeBitwise<T> right) => !right.Equals(left);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeBitwise<T> Empty => new();
    }
}