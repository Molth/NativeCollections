using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native pointer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly struct NativePointer<T> : IEquatable<NativePointer<T>>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly nint _handle;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != 0;

        /// <summary>
        ///     Handle
        /// </summary>
        public nint Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle;
        }

        /// <summary>
        ///     Value
        /// </summary>
        public ref T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref AsRef();
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref AsRef(), (nint)index);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref AsRef(), (nint)index);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativePointer<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativePointer<T> nativePointer && nativePointer == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => _handle.GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        public override string ToString() => $"NativePointer<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativePointer<T> left, NativePointer<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativePointer<T> left, NativePointer<T> right) => left._handle != right._handle;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativePointer(nint handle) => _handle = handle;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value of type <typeparamref name="T" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AsRef() => ref Unsafe.AddByteOffset(ref Unsafe.NullRef<T>(), _handle);

        /// <summary>
        ///     Reinterprets the given location as a reference to a value of type <typeparamref name="TTo" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TTo Cast<TTo>() => ref Unsafe.AddByteOffset(ref Unsafe.NullRef<TTo>(), _handle);

        /// <summary>
        ///     Slice
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativePointer<T> Slice(int start) => new(_handle + (nint)start * Unsafe.SizeOf<T>());

        /// <summary>
        ///     As span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref AsRef(), 1);

        /// <summary>
        ///     As span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref AsRef(), (nint)start), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref AsRef(), 1);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref AsRef(), (nint)start), length);

        /// <summary>
        ///     As reference
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativePointer<T>(nint handle) => new(handle);

        /// <summary>
        ///     As handle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator nint(NativePointer<T> nativePointer) => nativePointer._handle;

        /// <summary>
        ///     As span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativePointer<T> nativePointer) => nativePointer.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativePointer<T> nativePointer) => nativePointer.AsReadOnlySpan();

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativePointer<T> Create<TFrom>(ref TFrom reference) => new(Unsafe.ByteOffset(ref Unsafe.NullRef<TFrom>(), ref reference));

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativePointer<T> Create<TFrom>(Span<TFrom> buffer) => new(Unsafe.ByteOffset(ref Unsafe.NullRef<TFrom>(), ref MemoryMarshal.GetReference(buffer)));

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativePointer<T> Create<TFrom>(ReadOnlySpan<TFrom> buffer) => new(Unsafe.ByteOffset(ref Unsafe.NullRef<TFrom>(), ref MemoryMarshal.GetReference(buffer)));

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativePointer<T> Empty => new();
    }
}