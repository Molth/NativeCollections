using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CA2231 // Overload operator equals on overriding ValueType.Equals
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides atomic operations on a pointer to type <typeparamref name="T" />.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.Rust)]
    [BindingType(typeof(Interlocked))]
    public unsafe struct UnsafeAtomicPtr<T> where T : unmanaged
    {
        /// <summary>
        ///     Gets the value to the underlying object.
        /// </summary>
        private UnsafeAtomicIsize _value;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomicPtr(T* handle) => _value = new UnsafeAtomicIsize((nint)handle);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomicPtr(nint handle) => _value = new UnsafeAtomicIsize(handle);

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref T* AsRef() => ref Unsafe.As<UnsafeAtomicIsize, UnsafePtr<T>>(ref _value).Handle;

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Load(Ordering order) => (T*)_value.Load(order);

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(T* value, Ordering order) => _value.Store((nint)value, order);

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Exchange(T* value) => (T*)_value.Exchange((nint)value);

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* CompareExchange(T* value, T* comparand) => (T*)_value.CompareExchange((nint)value, (nint)comparand);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeAtomicPtr<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(reference))]
        public static UnsafeAtomicPtr<T> Create([MustBePinned] ref T reference) => new(UnsafeHelpers.AsPointer(ref reference));

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(buffer))]
        public static UnsafeAtomicPtr<T> Create([MustBePinned] Span<T> buffer) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(buffer)));

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeAtomicPtr<T> Empty => default;
    }
}