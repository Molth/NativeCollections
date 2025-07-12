using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe helpers
    /// </summary>
    internal static unsafe class UnsafeHelpers
    {
        /// <summary>
        ///     Reinterprets the given value of type <typeparamref name="TFrom" /> as a value of type <typeparamref name="TTo" />.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     The sizes of <typeparamref name="TFrom" /> and <typeparamref name="TTo" /> are not the same
        ///     or the type parameters are not <see langword="struct" />s.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo BitCast<TFrom, TTo>(in TFrom source) where TFrom : unmanaged where TTo : unmanaged
        {
            if (sizeof(TFrom) != sizeof(TTo))
                ThrowHelpers.ThrowNotSupportedException();
            return Unsafe.ReadUnaligned<TTo>(ref Unsafe.As<TFrom, byte>(ref Unsafe.AsRef(in source)));
        }

        /// <summary>Converts the value of a 32-bit signed integer to an <see cref="T:System.IntPtr" />.</summary>
        /// <param name="value">A 32-bit signed integer.</param>
        /// <returns>A new instance of <see cref="T:System.IntPtr" /> initialized to <paramref name="value" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint ToIntPtr(int value) => value;

        /// <summary>
        ///     Adds an element offset to the given pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Add<T>(void* source, nint elementOffset) where T : unmanaged => (T*)Unsafe.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<T>(source), elementOffset));

        /// <summary>
        ///     Adds a byte offset to the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AddByteOffset<T>(void* source, nint elementOffset) where T : unmanaged => (T*)Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(source), elementOffset));

        /// <summary>
        ///     Adds a byte offset to the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AddByteOffset(void* source, nint elementOffset) => Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(source), elementOffset));

        /// <summary>
        ///     Subtracts an element offset from the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Subtract<T>(void* source, nint elementOffset) where T : unmanaged => (T*)Unsafe.AsPointer(ref Unsafe.Subtract(ref Unsafe.AsRef<T>(source), elementOffset));

        /// <summary>
        ///     Subtracts a byte offset from the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* SubtractByteOffset<T>(void* source, nint elementOffset) where T : unmanaged => (T*)Unsafe.AsPointer(ref Unsafe.SubtractByteOffset(ref Unsafe.AsRef<byte>(source), elementOffset));

        /// <summary>
        ///     Subtracts a byte offset from the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* SubtractByteOffset(void* source, nint elementOffset) => Unsafe.AsPointer(ref Unsafe.SubtractByteOffset(ref Unsafe.AsRef<byte>(source), elementOffset));

        /// <summary>
        ///     Determines the byte offset from origin to target from the given references.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint ByteOffset(void* origin, void* target) => Unsafe.ByteOffset(ref Unsafe.AsRef<byte>(origin), ref Unsafe.AsRef<byte>(target));
    }
}