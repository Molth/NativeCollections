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
        ///     Value
        /// </summary>
        private nint _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeAtomicReference(T* value) => _value = (nint)value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeAtomicReference(nint value) => _value = value;

        /// <summary>
        ///     Value
        /// </summary>
        public T* Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Read();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Exchange(value);
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Read() => (T*)Interlocked.CompareExchange(ref _value, new IntPtr(0), new IntPtr(0));

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Exchange(T* value) => (T*)Interlocked.Exchange(ref _value, (nint)value);

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* CompareExchange(T* value, T* comparand) => (T*)Interlocked.CompareExchange(ref _value, (nint)value, (nint)comparand);

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
        ///     Empty
        /// </summary>
        public static NativeAtomicReference<T> Empty => new();
    }
}