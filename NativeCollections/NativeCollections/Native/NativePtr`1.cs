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
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativePtr<T> : IIsCreated, IDisposable, IEquatable<NativePtr<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly T* _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Handle
        /// </summary>
        public T* Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle;
        }

        /// <summary>
        ///     Value
        /// </summary>
        public ref T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<T>(_handle);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_handle), (nint)index);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_handle), (nint)index);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="handle">Handle</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativePtr(T* handle) => _handle = handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="handle">Handle</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativePtr(nint handle) => _handle = (T*)handle;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativePtr<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativePtr<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativePtr<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Reinterprets the given location as a reference to a value of type <typeparamref name="T" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AsRef() => ref Unsafe.AsRef<T>(_handle);

        /// <summary>
        ///     Casts a instance of one primitive type to another primitive type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativePtr<TTo> Cast<TTo>() where TTo : unmanaged => new((TTo*)_handle);

        /// <summary>
        ///     Forms a slice out of the given span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativePtr<T> Slice(int start) => new(UnsafeHelpers.Add<T>(_handle, start));

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_handle), 1);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_handle), (nint)start), length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_handle), 1);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_handle), (nint)start), length);

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativePtr<T>(T* value) => new(value);

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(NativePtr<T> value) => value._handle;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativePtr<T>(nint value) => new((T*)value);

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator nint(NativePtr<T> value) => (nint)value._handle;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativePtr<T> value) => value.AsSpan();

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativePtr<T> value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativePtr<T> left, NativePtr<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativePtr<T> left, NativePtr<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Free(_handle);

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="reference">Reference</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(reference))]
        public static NativePtr<T> Create<TFrom>([MustBePinned] ref TFrom reference) => new((T*)Unsafe.AsPointer(ref reference));

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(buffer))]
        public static NativePtr<T> Create<TFrom>([MustBePinned] Span<TFrom> buffer) => new((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)));

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(buffer))]
        public static NativePtr<T> Create<TFrom>([MustBePinned] ReadOnlySpan<TFrom> buffer) => new((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)));

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativePtr<T> Empty => default;
    }
}