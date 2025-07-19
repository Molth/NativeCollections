using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8600
#pragma warning disable CS8603
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native object
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly struct NativeObject<T> : IDisposable, IEquatable<NativeObject<T>>
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
        ///     Value
        /// </summary>
        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T)_handle.Target;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeObject<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeObject<T> nativeObject && nativeObject == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeObject<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeObject<T> left, NativeObject<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeObject<T> left, NativeObject<T> right) => left._handle != right._handle;

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
        public static NativeObject<T> Alloc(object? value) => new(GCHandle.Alloc(value));

        /// <summary>
        ///     Alloc
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="type">Type</param>
        /// <returns>NativeObject</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeObject<T> Alloc(object? value, GCHandleType type) => new(GCHandle.Alloc(value, type));

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeObject<T> Empty => new();
    }
}