using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native reference
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [IsAssignableTo(typeof(IEquatable<>))]
    public readonly unsafe ref struct NativeReference
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly Span<byte> _buffer;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => !Unsafe.IsNullRef(ref MemoryMarshal.GetReference(_buffer));

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _buffer.Length == 0;

        /// <summary>
        ///     Buffer
        /// </summary>
        public Span<byte> Buffer => _buffer;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(_buffer), UnsafeHelpers.ToIntPtr(index));
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref byte this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(_buffer), (nint)index);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReference(Span<byte> buffer) => _buffer = buffer;

        /// <summary>
        ///     Cast
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Cast<T>() where T : unmanaged => ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(_buffer));

        /// <summary>
        ///     Cast
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Cast<T>(int start) where T : unmanaged => ref Unsafe.Add(ref Cast<T>(), (nint)start);

        /// <summary>
        ///     Slice
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReference Slice<T>(int start) where T : unmanaged => new(_buffer.Slice(start * sizeof(T)));

        /// <summary>
        ///     As span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan<T>() where T : unmanaged => MemoryMarshal.Cast<byte, T>(_buffer);

        /// <summary>
        ///     As span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan<T>(int start, int length) where T : unmanaged => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(_buffer)), (nint)start), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan<T>() where T : unmanaged => MemoryMarshal.Cast<byte, T>(_buffer);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan<T>(int start, int length) where T : unmanaged => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(_buffer)), (nint)start), length);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeReference other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => throw new NotSupportedException("CannotCallEquals");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => throw new NotSupportedException("CannotCallGetHashCode");

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeReference";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeReference left, NativeReference right) => Unsafe.AreSame(ref MemoryMarshal.GetReference(left._buffer), ref MemoryMarshal.GetReference(right._buffer));

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeReference left, NativeReference right) => !Unsafe.AreSame(ref MemoryMarshal.GetReference(left._buffer), ref MemoryMarshal.GetReference(right._buffer));

        /// <summary>
        ///     As span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(in NativeReference nativeReference) => nativeReference._buffer;

        /// <summary>
        ///     As readOnly span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(in NativeReference nativeReference) => nativeReference._buffer;

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeReference Create<T>(ref T reference) where T : unmanaged => new(MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref reference), sizeof(T)));

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeReference Create<T>(Span<T> buffer) where T : unmanaged => new(MemoryMarshal.Cast<T, byte>(buffer));

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeReference Empty => new();
    }
}