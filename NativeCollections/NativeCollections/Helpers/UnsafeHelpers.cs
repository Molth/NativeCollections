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
        public static TTo BitCast<TFrom, TTo>(TFrom source) where TFrom : unmanaged where TTo : unmanaged
        {
            if (typeof(TFrom) == typeof(TTo))
                return Unsafe.As<TFrom, TTo>(ref source);
#if NET8_0_OR_GREATER
            return Unsafe.BitCast<TFrom, TTo>(source);
#else
            if (sizeof(TFrom) != sizeof(TTo))
                ThrowHelpers.ThrowNotSupportedException();
            return Unsafe.ReadUnaligned<TTo>(ref Unsafe.As<TFrom, byte>(ref source));
#endif
        }

        /// <summary>
        ///     Adds an element offset to the given pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Add<T>(ref T source, nint elementOffset) where T : unmanaged => (T*)Unsafe.AsPointer(ref Unsafe.Add(ref source, elementOffset));

        /// <summary>
        ///     Adds an element offset to the given pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Add<T>(void* source, nint elementOffset) where T : unmanaged => (T*)Unsafe.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<T>(source), elementOffset));

        /// <summary>
        ///     Adds a byte offset to the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AddByteOffset<T>(void* source, nint byteOffset) where T : unmanaged => (T*)Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(source), byteOffset));

        /// <summary>
        ///     Adds a byte offset to the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AddByteOffset(void* source, nint byteOffset) => Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(source), byteOffset));

        /// <summary>
        ///     Subtracts an element offset from the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Subtract<T>(void* source, nint elementOffset) where T : unmanaged => (T*)Unsafe.AsPointer(ref Unsafe.Subtract(ref Unsafe.AsRef<T>(source), elementOffset));

        /// <summary>
        ///     Subtracts a byte offset from the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* SubtractByteOffset<T>(void* source, nint byteOffset) where T : unmanaged => (T*)Unsafe.AsPointer(ref Unsafe.SubtractByteOffset(ref Unsafe.AsRef<byte>(source), byteOffset));

        /// <summary>
        ///     Subtracts a byte offset from the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* SubtractByteOffset(void* source, nint byteOffset) => Unsafe.AsPointer(ref Unsafe.SubtractByteOffset(ref Unsafe.AsRef<byte>(source), byteOffset));

        /// <summary>
        ///     Determines the byte offset from origin to target from the given references.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint ByteOffset(void* origin, void* target) => Unsafe.ByteOffset(ref Unsafe.AsRef<byte>(origin), ref Unsafe.AsRef<byte>(target));
    }
}