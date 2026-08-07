using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a disposable wrapper over a pointer to an unmanaged disposable type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeDisposable<T> : IIsCreated, IDisposable, IEquatable<UnsafeDisposable<T>> where T : unmanaged, IDisposable
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private T* _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Handle
        /// </summary>
        public readonly T* Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="handle">Handle</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeDisposable(T* handle) => _handle = handle;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeDisposable<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeDisposable<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeDisposable<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafeDisposable<T>(T* value) => new(value);

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(UnsafeDisposable<T> value) => value._handle;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeDisposable<T> left, UnsafeDisposable<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeDisposable<T> left, UnsafeDisposable<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {
            var handle = _handle;
            if (UnsafeHelpers.IsNull(handle))
                return;
            handle->Dispose();
        }

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Create(T* reference) => _handle = reference;

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(reference))]
        public void Create([MustBePinned] ref T reference) => _handle = UnsafeHelpers.AsPointer(ref reference);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeDisposable<T> Empty => default;
    }
}