using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a reference to an object held by a <see cref="GCHandle" />.
    /// </summary>
    /// <typeparam name="T">The type of the object referenced by the handle.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly struct NativeObject<T> : IIsCreated, IDisposable, IEquatable<NativeObject<T>>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly GCHandle _handle;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="handle">The GC handle to wrap.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeObject(GCHandle handle) => _handle = handle;

        /// <summary>
        ///     Creates a new GC handle for an object using the default
        ///     <see cref="GCHandleType.Normal" /> handle type.
        /// </summary>
        /// <param name="value">The object that the GC handle is created for.</param>
        /// <returns>A new instance of this class that wraps the created GC handle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeObject<T> Create(T value) => new(GCHandle.Alloc(value));

        /// <summary>
        ///     Creates a new GC handle of the specified type for an object.
        /// </summary>
        /// <param name="value">The object that the GC handle is created for.</param>
        /// <param name="type">
        ///     The type of GC handle to create; the object is pinned only when
        ///     <see cref="GCHandleType.Pinned" /> is specified.
        /// </param>
        /// <returns>A new instance of this class that wraps the created GC handle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeObject<T> Create(T value, GCHandleType type) => new(GCHandle.Alloc(value, type));

        /// <summary>
        ///     Creates an object wrapper for an existing GC handle.
        /// </summary>
        /// <param name="handle">The GC handle to wrap.</param>
        /// <returns>A new instance of this class.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeObject<T> From(GCHandle handle) => new(handle);

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsAllocated;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        public GCHandle Handle => _handle;

        /// <summary>
        ///     Gets the object this handle represents.
        /// </summary>
        /// <returns>
        ///     The object this handle represents.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the handle was freed or was never initialized.
        /// </exception>
        public object? Target => _handle.Target;

        /// <summary>
        ///     Gets the value of the underlying object.
        /// </summary>
        /// <remarks>
        ///     A freed or never-initialized handle causes <see cref="InvalidOperationException" /> to be
        ///     thrown when the underlying handle target is accessed.
        /// </remarks>
        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T)_handle.Target!;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeObject<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeObject<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeObject<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeObject<T> left, NativeObject<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeObject<T> left, NativeObject<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var handle = _handle;
            if (!handle.IsAllocated)
                return;

            handle.Free();
        }
    }
}