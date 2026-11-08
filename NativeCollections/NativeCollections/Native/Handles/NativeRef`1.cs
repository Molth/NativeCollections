using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a managed reference to a value.
    /// </summary>
    /// <typeparam name="T">The type of the referenced value.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [IsReferenceOrContainsReferences]
    [IsAssignableTo(typeof(IIsCreated))]
    public readonly ref struct NativeRef<T>
#if NET9_0_OR_GREATER
        : IIsCreated
#endif
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly Span<T> _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !Unsafe.IsNullRef(ref AsRef());

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="handle">The span to wrap.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeRef(Span<T> handle) => _handle = handle;

        /// <summary>
        ///     Wraps an external span.
        /// </summary>
        /// <param name="handle">The span to wrap.</param>
        /// <returns>A native reference that wraps <paramref name="handle" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeRef<T> From(Span<T> handle) => new(handle);

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
        public override string ToString() => SR.Format("NativeRef<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Reinterprets the given location as a reference to a value of type <typeparamref name="T" />.
        /// </summary>
        /// <returns>A reference to the value wrapped by this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AsRef() => ref MemoryMarshal.GetReference(_handle);

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="reference">The reference to wrap.</param>
        /// <returns>A new instance that wraps <paramref name="reference" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeRef<T> From(ref T reference) => new(MemoryMarshal.CreateSpan(ref reference, 1));
    }
}