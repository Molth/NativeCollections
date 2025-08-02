using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CS1591
#pragma warning disable CA2208
#pragma warning disable CA2231
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native atomic reference
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public unsafe struct NativeAtomicReference<T> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private nint _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeAtomicReference(T* handle) => _handle = (nint)handle;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeAtomicReference(nint handle) => _handle = handle;

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Read() => (T*)Interlocked.CompareExchange(ref _handle, new IntPtr(0), new IntPtr(0));

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Exchange(T* value) => (T*)Interlocked.Exchange(ref _handle, (nint)value);

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* CompareExchange(T* value, T* comparand) => (T*)Interlocked.CompareExchange(ref _handle, (nint)value, (nint)comparand);

        /// <summary>
        ///     Equals
        /// </summary>
        public readonly override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Get hashCode
        /// </summary>
        public readonly override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     To string
        /// </summary>
        public readonly override string ToString() => $"NativeAtomicReference{typeof(T).Name}";

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="reference">Reference</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeAtomicReference<T> Create(ref T reference) => new((T*)Unsafe.AsPointer(ref reference));

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeAtomicReference<T> Create(Span<T> buffer) => new((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)));

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeAtomicReference<T> Empty => new();
    }
}