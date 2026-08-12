using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a reference to an object pinned in memory using a <see cref="GCHandle" />.
    /// </summary>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeObject(GCHandle handle) => _handle = handle;

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
        /// <exception cref="T:System.InvalidOperationException">The handle was freed, or never initialized.</exception>
        /// <returns>The object this handle represents.</returns>
        public object? Target => _handle.Target;

        /// <summary>
        ///     Gets the value to the underlying object.
        /// </summary>
        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T)_handle.Target!;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeObject<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeObject<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeObject<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeObject<T> left, NativeObject<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeObject<T> left, NativeObject<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
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
        ///     Creates a new GC handle for an object.
        /// </summary>
        /// <param name="value">The object that the GC handle is created for.</param>
        /// <returns>A new GC handle that protects the object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeObject<T> Create(T value) => new(GCHandle.Alloc(value));

        /// <summary>
        ///     Creates a new GC handle for an object.
        /// </summary>
        /// <param name="value">The object that the GC handle is created for.</param>
        /// <param name="type">The type of GC handle to create.</param>
        /// <returns>A new GC handle that protects the object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeObject<T> Create(T value, GCHandleType type) => new(GCHandle.Alloc(value, type));

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeObject<T> Empty => default;
    }
}