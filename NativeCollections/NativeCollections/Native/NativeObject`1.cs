using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native object
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly struct NativeObject<T> : IIsCreated, IDisposable, IEquatable<NativeObject<T>>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly GCHandle _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="handle">GCHandle</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeObject(GCHandle handle) => _handle = handle;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle.IsAllocated;

        /// <summary>
        ///     Handle
        /// </summary>
        public GCHandle Handle => _handle;

        /// <summary>
        ///     Target
        /// </summary>
        public object? Target => _handle.Target;

        /// <summary>
        ///     Value
        /// </summary>
        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T)_handle.Target!;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeObject<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeObject<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => SR.Format("NativeObject<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeObject<T> left, NativeObject<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeObject<T> left, NativeObject<T> right) => !left.Equals(right);

        /// <summary>
        ///     As value
        /// </summary>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(NativeObject<T> nativeObject) => nativeObject.Value;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (!handle.IsAllocated)
                return;
            handle.Free();
        }

        /// <summary>
        ///     Alloc
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>NativeObject</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeObject<T> Alloc(T value) => new(GCHandle.Alloc(value));

        /// <summary>
        ///     Alloc
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="type">Type</param>
        /// <returns>NativeObject</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeObject<T> Alloc(T value, GCHandleType type) => new(GCHandle.Alloc(value, type));

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeObject<T> Empty => new();
    }
}