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
    ///     Unsafe atomic reference
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Rust)]
    public unsafe struct UnsafeAtomicReference<T> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private nint _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomicReference(T* handle) => _handle = (nint)handle;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomicReference(nint handle) => _handle = handle;

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref nint AsRef() => ref _handle;

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Read() => (T*)Interlocked.CompareExchange(ref _handle, new IntPtr(0), new IntPtr(0));

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Exchange(T* value) => (T*)Interlocked.Exchange(ref _handle, (nint)value);

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* CompareExchange(T* value, T* comparand) => (T*)Interlocked.CompareExchange(ref _handle, (nint)value, (nint)comparand);

        /// <summary>
        ///     Equals
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Get hashCode
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     To string
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeAtomicReference<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="reference">Reference</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(reference))]
        public static UnsafeAtomicReference<T> Create([MustBePinned] ref T reference) => new(UnsafeHelpers.AsPointer(ref reference));

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(buffer))]
        public static UnsafeAtomicReference<T> Create([MustBePinned] Span<T> buffer) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(buffer)));

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeAtomicReference<T> Empty => new();
    }
}