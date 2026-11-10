using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a disposable wrapper over a pointer to an unmanaged disposable type.
    /// </summary>
    /// <typeparam name="T">The unmanaged type whose instances are disposed.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeDisposable<T> : IIsCreated, IDisposable, IEquatable<UnsafeDisposable<T>> where T : unmanaged, IDisposable
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private T* _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _handle != null;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        public readonly T* Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle;
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="handle">A pointer to the unmanaged disposable value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeDisposable(T* handle) => _handle = handle;

        /// <summary>
        ///     Creates a disposable wrapper for an existing native pointer.
        /// </summary>
        /// <param name="handle">The pointer to wrap.</param>
        /// <returns>A disposable wrapper that references <paramref name="handle" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeDisposable<T> From(T* handle) => new(handle);

        /// <summary>
        ///     Creates a disposable wrapper for an existing unmanaged disposable value.
        /// </summary>
        /// <param name="reference">The unmanaged disposable value to reference.</param>
        /// <returns>A disposable wrapper that references <paramref name="reference" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeDisposable<T> From([MustBePinned] ref T reference) => new(UnsafeHelpers.AsPointer(ref reference));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeDisposable<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeDisposable<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeDisposable<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeDisposable<T> left, UnsafeDisposable<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeDisposable<T> left, UnsafeDisposable<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;

            handle->Dispose();
        }
    }
}