using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a typed pointer to unmanaged memory.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafePtr<T> : IIsCreated, IDisposable, IEquatable<UnsafePtr<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        public T* Handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(Handle);

        /// <summary>
        ///     Value
        /// </summary>
        public readonly ref T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<T>(Handle);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(Handle), (nint)index);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(Handle), (nint)index);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="handle">Handle</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafePtr(T* handle) => Handle = handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="handle">Handle</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafePtr(nint handle) => Handle = (T*)handle;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafePtr<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafePtr<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafePtr<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Reinterprets the given location as a reference to a value of type <typeparamref name="T" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref T AsRef() => ref Unsafe.AsRef<T>(Handle);

        /// <summary>
        ///     Casts a instance of one primitive type to another primitive type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnsafePtr<TTo> Cast<TTo>() where TTo : unmanaged => new((TTo*)Handle);

        /// <summary>
        ///     Forms a slice out of the given span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnsafePtr<T> Slice(int start) => new(UnsafeHelpers.Add<T>(Handle, start));

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(Handle), 1);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(Handle), (nint)start), length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(Handle), 1);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(Handle), (nint)start), length);

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafePtr<T>(T* value) => new(value);

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(UnsafePtr<T> value) => value.Handle;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafePtr<T>(nint value) => new((T*)value);

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator nint(UnsafePtr<T> value) => (nint)value.Handle;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(UnsafePtr<T> value) => value.AsSpan();

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(UnsafePtr<T> value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafePtr<T> left, UnsafePtr<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafePtr<T> left, UnsafePtr<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => Box.Free(Handle);

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="reference">Reference</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(reference))]
        public static UnsafePtr<T> Create<TFrom>([MustBePinned] ref TFrom reference) => new((T*)Unsafe.AsPointer(ref reference));

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(buffer))]
        public static UnsafePtr<T> Create<TFrom>([MustBePinned] Span<TFrom> buffer) => new((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)));

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(buffer))]
        public static UnsafePtr<T> Create<TFrom>([MustBePinned] ReadOnlySpan<TFrom> buffer) => new((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)));

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafePtr<T> Empty => default;
    }
}