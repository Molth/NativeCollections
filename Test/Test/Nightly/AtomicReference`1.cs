using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CA2231 // Overload operator equals on overriding ValueType.Equals
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

// ReSharper disable ALL

namespace Examples
{
    /// <summary>
    ///     Atomic reference
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public struct AtomicReference<T> where T : class?
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private object? _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AtomicReference(T? handle) => _handle = (object?)handle;

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T? AsRef() => ref Unsafe.As<object?, T?>(ref Unsafe.AsRef(in _handle));

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Read() => (T?)Interlocked.CompareExchange(ref _handle, null, null);

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Exchange(T value) => (T?)Interlocked.Exchange(ref _handle, (object?)value);

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? CompareExchange(T? value, T? comparand) => (T?)Interlocked.CompareExchange(ref _handle, (object?)value, (object?)comparand);

        /// <summary>
        ///     Equals
        /// </summary>
        [Obsolete("Call this method will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override bool Equals(object? obj) => throw new NotSupportedException("CannotCallEquals");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        [Obsolete("Call this method will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override int GetHashCode() => throw new NotSupportedException("CannotCallGetHashCode");

        /// <summary>
        ///     To string
        /// </summary>
        public readonly override string ToString() => $"AtomicReference{typeof(T).Name}";
    }
}