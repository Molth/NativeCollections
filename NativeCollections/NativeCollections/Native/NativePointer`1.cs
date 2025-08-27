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
        ///     Handle
        /// </summary>
        public nint Handle => _handle;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != 0;

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
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativePointer<T> Create<TFrom>(ref TFrom reference) => new(Unsafe.ByteOffset(ref Unsafe.NullRef<TFrom>(), ref reference));

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativePointer<T> Empty => new();
    }
}