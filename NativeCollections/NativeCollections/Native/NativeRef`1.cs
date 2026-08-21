using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a managed reference to a value.
    /// </summary>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeRef(Span<T> handle) => _handle = handle;

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
        public override string ToString() => SR.Format("NativeRef<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Reinterprets the given location as a reference to a value of type <typeparamref name="T" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AsRef() => ref MemoryMarshal.GetReference(_handle);

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeRef<T> Create(ref T reference) => new(MemoryMarshal.CreateSpan(ref reference, 1));

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeRef<T> Empty => default;
    }
}