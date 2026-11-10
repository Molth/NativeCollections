using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CA2231 // Overload operator equals on overriding ValueType.Equals.
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member.
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides atomic operations on a managed reference to type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type of the referenced object.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    [IsReferenceOrContainsReferences]
    [BindingType(typeof(Interlocked))]
    public unsafe struct UnsafeAtomicRef<T> where T : class?
    {
        /// <summary>
        ///     Gets the underlying value.
        /// </summary>
        private object? _value;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="handle">The initial value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeAtomicRef(T? handle) => _value = handle;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="reference">The value used to create the instance.</param>
        /// <returns>A new instance initialized with <paramref name="reference" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeAtomicRef<T> From([MustBePinned] T? reference) => new(reference);

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="buffer">The value used to create the instance.</param>
        /// <returns>A new instance initialized with the first element of <paramref name="buffer" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeAtomicRef<T> From([MustBePinned] Span<T> buffer) => new(MemoryMarshal.GetReference(buffer));

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        /// <returns>A reference to the atomic value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref T? AsRef() => ref Unsafe.As<object?, T?>(ref _value);

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <param name="order">The memory ordering constraint applied to the load operation.</param>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="order" /> is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Load(Ordering order) => (T?)AtomicHelpers.Load(ref _value, order);

        /// <summary>
        ///     Sets a variable to a specified value, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="order">The memory ordering constraint applied to the store operation.</param>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="order" /> is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(T? value, Ordering order) => AtomicHelpers.Store(ref _value, value, order);

        /// <summary>
        ///     Sets a variable to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Exchange(T? value) => (T?)Interlocked.Exchange(ref _value, value);

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value, as an atomic operation.
        /// </summary>
        /// <param name="value">
        ///     The value that replaces the destination value if the comparison results in equality.
        /// </param>
        /// <param name="comparand">The value that is compared to the value at the destination.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? CompareExchange(T? value, T? comparand) => (T?)Interlocked.CompareExchange(ref _value, value, comparand);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
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
        /// <returns>The hash code for this instance.</returns>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeAtomicRef<{0}>", SR.GetTypeName(typeof(T)));
    }
}