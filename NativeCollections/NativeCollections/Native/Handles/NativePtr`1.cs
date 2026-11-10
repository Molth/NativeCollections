using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a typed pointer to unmanaged memory.
    /// </summary>
    /// <typeparam name="T">The unmanaged type pointed to.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativePtr<T> : IIsCreated, IDisposable, IEquatable<NativePtr<T>> where T : unmanaged
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly T* _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public T* Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle;
        }

        /// <summary>
        ///     Gets a reference to the value at the location this points to.
        /// </summary>
        public ref T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<T>(_handle);
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="handle">A pointer to the unmanaged value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativePtr(T* handle) => _handle = handle;

        /// <summary>
        ///     Creates a pointer wrapper for an existing native pointer.
        /// </summary>
        /// <param name="handle">The pointer to wrap.</param>
        /// <returns>A pointer wrapper that references <paramref name="handle" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativePtr<T> From(T* handle) => new(handle);

        /// <summary>
        ///     Creates a pointer wrapper from a native integer pointer value.
        /// </summary>
        /// <param name="handle">The native integer pointer value.</param>
        /// <returns>A pointer wrapper that references the address specified by <paramref name="handle" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativePtr<T> From(nint handle) => From((T*)handle);

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <typeparam name="TFrom">The type of the referenced value.</typeparam>
        /// <param name="reference">The reference whose address is used as the pointer.</param>
        /// <returns>A pointer wrapper that references the address of <paramref name="reference" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativePtr<T> From<TFrom>([MustBePinned] ref TFrom reference) => new((T*)Unsafe.AsPointer(ref reference));

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <typeparam name="TFrom">The type of the elements in the span.</typeparam>
        /// <param name="buffer">The span whose first element address is used as the pointer.</param>
        /// <returns>
        ///     A pointer wrapper that references the address of the first element of <paramref name="buffer" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativePtr<T> From<TFrom>([MustBePinned] Span<TFrom> buffer) => new((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativePtr<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativePtr<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativePtr<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Reinterprets the given location as a reference to a value of type <typeparamref name="T" />.
        /// </summary>
        /// <returns>A reference to the value at the address represented by this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AsRef() => ref Unsafe.AsRef<T>(_handle);

        /// <summary>
        ///     Casts an instance of one primitive type to another primitive type.
        /// </summary>
        /// <typeparam name="TTo">The type to reinterpret the pointer as.</typeparam>
        /// <returns>A pointer wrapper to the same address, typed as <typeparamref name="TTo" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativePtr<TTo> Cast<TTo>() where TTo : unmanaged => NativePtr<TTo>.From((TTo*)_handle);

        /// <summary>
        ///     Returns a span over the entire contents of this.
        /// </summary>
        /// <returns>A span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_handle), 1);

        /// <summary>
        ///     Returns a read-only span over the entire contents of this.
        /// </summary>
        /// <returns>A read-only span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_handle), 1);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativePtr<T> left, NativePtr<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativePtr<T> left, NativePtr<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => NativeMemoryAllocator.AlignedFree(_handle);
    }
}