using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe disposable reference
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeDisposable<T> : IIsCreated, IDisposable, IEquatable<UnsafeDisposable<T>> where T : unmanaged, IDisposable
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private T* _handle;

        /// <summary>
        ///     Is created
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
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeDisposable<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeDisposable<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => SR.Format("UnsafeDisposable<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     As reference
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafeDisposable<T>(T* handle) => new(handle);

        /// <summary>
        ///     As handle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(UnsafeDisposable<T> unsafeDisposable) => unsafeDisposable._handle;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeDisposable<T> left, UnsafeDisposable<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeDisposable<T> left, UnsafeDisposable<T> right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
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
        public static UnsafeDisposable<T> Empty => new();
    }
}