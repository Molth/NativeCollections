using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Contains generic, low-level functionality for manipulating managed and unmanaged pointers.
    /// </summary>
    internal static unsafe class UnsafeHelpers
    {
        /// <summary>
        ///     Reinterprets the given value of type <typeparamref name="TFrom" />
        ///     as a value of type <typeparamref name="TTo" />.
        /// </summary>
        /// <typeparam name="TFrom">The type of the input value.</typeparam>
        /// <typeparam name="TTo">The type to reinterpret to.</typeparam>
        /// <param name="source">The value to reinterpret.</param>
        /// <returns>
        ///     The value of type <typeparamref name="TTo" /> reinterpreted from <paramref name="source" />.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown when the sizes of <typeparamref name="TFrom" /> and <typeparamref name="TTo" />
        ///     are not the same.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo BitCast<TFrom, TTo>(TFrom source) where TFrom : unmanaged where TTo : unmanaged
        {
#if NET8_0_OR_GREATER
            return Unsafe.BitCast<TFrom, TTo>(source);
#else
            ThrowHelpers.ThrowIfNotSameSizes<TFrom, TTo>();
            return Unsafe.ReadUnaligned<TTo>(ref Unsafe.As<TFrom, byte>(ref source));
#endif
        }

        /// <summary>
        ///     Determines the misalignment of the address with respect to the specified <paramref name="alignment" />.
        /// </summary>
        /// <typeparam name="T">The type of the value whose address is examined.</typeparam>
        /// <param name="address">The value whose address to examine.</param>
        /// <param name="alignment">The alignment boundary in bytes.</param>
        /// <returns>The misalignment of the address of <paramref name="address" />, in bytes.</returns>
        /// <remarks>
        ///     <paramref name="alignment" /> is not validated; callers must pass a non-zero power of two value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint OpportunisticMisalignment<T>(ref T address, nuint alignment) where T : unmanaged => Usize.FromIsizeUnchecked(NativeMemoryAllocator.AddressOf(ref address)) & alignment.WrappingSub(Usize.FromI32Unchecked(1));

        /// <summary>
        ///     Returns a pointer to the specified by-ref parameter.
        /// </summary>
        /// <typeparam name="T">The type of the value to get a pointer to.</typeparam>
        /// <param name="value">The value whose address to return.</param>
        /// <returns>A pointer to <paramref name="value" />.</returns>
        /// <remarks>
        ///     The returned pointer is not tracked by the garbage collector, so it is valid only while
        ///     <paramref name="value" /> refers to a location that is alive and is not relocated.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AsPointer<T>(ref T value) where T : unmanaged => (T*)Unsafe.AsPointer(ref value);

        /// <summary>
        ///     Reads a value of type <typeparamref name="T" /> from the given location.
        /// </summary>
        /// <typeparam name="T">The type of the value to read.</typeparam>
        /// <param name="source">The location from which to read the value.</param>
        /// <returns>The value read from <paramref name="source" />.</returns>
        /// <remarks>
        ///     <paramref name="source" /> is not validated; it must refer to an initialized location
        ///     of at least <see cref="Unsafe.SizeOf{T}()" /> bytes. The location does not have to be aligned.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadUnaligned<T>(ref T source) where T : unmanaged => Unsafe.ReadUnaligned<T>(ref Unsafe.As<T, byte>(ref source));

        /// <summary>
        ///     Writes a value of type <typeparamref name="T" /> to the given location.
        /// </summary>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        /// <param name="destination">The location to which to write the value.</param>
        /// <param name="value">The value to write.</param>
        /// <remarks>
        ///     <paramref name="destination" /> is not validated; it must refer to a writable
        ///     location of at least <see cref="Unsafe.SizeOf{T}()" /> bytes. The location does not have to be aligned.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnaligned<T>(ref T destination, T value) where T : unmanaged => Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref destination), value);

        /// <summary>
        ///     Adds an element offset to the given reference.
        /// </summary>
        /// <typeparam name="T">The type of the elements that <paramref name="source" /> refers to.</typeparam>
        /// <param name="source">The reference to add the offset to.</param>
        /// <param name="elementOffset">The offset to add, in elements of type <typeparamref name="T" />.</param>
        /// <returns>
        ///     A pointer that reflects the addition of <paramref name="elementOffset" /> to <paramref name="source" />.
        /// </returns>
        /// <remarks>
        ///     <paramref name="elementOffset" /> can be negative, and the resulting
        ///     address is not validated against the bounds of any allocation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Add<T>(ref T source, nint elementOffset) where T : unmanaged => AsPointer(ref Unsafe.Add(ref source, elementOffset));

        /// <summary>
        ///     Adds an element offset to the given pointer.
        /// </summary>
        /// <typeparam name="T">The type of the elements that <paramref name="source" /> points to.</typeparam>
        /// <param name="source">The pointer to add the offset to.</param>
        /// <param name="elementOffset">The offset to add, in elements of type <typeparamref name="T" />.</param>
        /// <returns>
        ///     A pointer that reflects the addition of <paramref name="elementOffset" /> to <paramref name="source" />.
        /// </returns>
        /// <remarks>
        ///     <paramref name="elementOffset" /> can be negative, and the resulting
        ///     address is not validated against the bounds of any allocation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Add<T>(void* source, nint elementOffset) where T : unmanaged => AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<T>(source), elementOffset));

        /// <summary>
        ///     Adds a byte offset to the given pointer.
        /// </summary>
        /// <typeparam name="T">The type of the elements that the returned pointer points to.</typeparam>
        /// <param name="source">The pointer to add the byte offset to.</param>
        /// <param name="byteOffset">The offset to add, in bytes.</param>
        /// <returns>
        ///     A pointer of type <typeparamref name="T" /> that reflects the addition
        ///     of <paramref name="byteOffset" /> to <paramref name="source" />.
        /// </returns>
        /// <remarks>
        ///     <paramref name="byteOffset" /> can be negative, and the resulting
        ///     address is not validated against the bounds of any allocation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AddByteOffset<T>(void* source, nint byteOffset) where T : unmanaged => (T*)Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(source), byteOffset));

        /// <summary>
        ///     Adds a byte offset to the given pointer.
        /// </summary>
        /// <param name="source">The pointer to add the byte offset to.</param>
        /// <param name="byteOffset">The offset to add, in bytes.</param>
        /// <returns>
        ///     A pointer that reflects the addition of <paramref name="byteOffset" /> to <paramref name="source" />.
        /// </returns>
        /// <remarks>
        ///     <paramref name="byteOffset" /> can be negative, and the resulting
        ///     address is not validated against the bounds of any allocation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AddByteOffset(void* source, nint byteOffset) => Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(source), byteOffset));

        /// <summary>
        ///     Subtracts a byte offset from the given pointer.
        /// </summary>
        /// <typeparam name="T">The type of the elements that the returned pointer points to.</typeparam>
        /// <param name="source">The pointer to subtract the byte offset from.</param>
        /// <param name="byteOffset">The offset to subtract, in bytes.</param>
        /// <returns>
        ///     A pointer of type <typeparamref name="T" /> that reflects the subtraction
        ///     of <paramref name="byteOffset" /> from <paramref name="source" />.
        /// </returns>
        /// <remarks>
        ///     <paramref name="byteOffset" /> can be negative, and the resulting
        ///     address is not validated against the bounds of any allocation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* SubtractByteOffset<T>(void* source, nint byteOffset) where T : unmanaged => (T*)Unsafe.AsPointer(ref Unsafe.SubtractByteOffset(ref Unsafe.AsRef<byte>(source), byteOffset));

        /// <summary>
        ///     Subtracts a byte offset from the given pointer.
        /// </summary>
        /// <param name="source">The pointer to subtract the byte offset from.</param>
        /// <param name="byteOffset">The offset to subtract, in bytes.</param>
        /// <returns>
        ///     A pointer that reflects the subtraction of
        ///     <paramref name="byteOffset" /> from <paramref name="source" />.
        /// </returns>
        /// <remarks>
        ///     <paramref name="byteOffset" /> can be negative, and the resulting
        ///     address is not validated against the bounds of any allocation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* SubtractByteOffset(void* source, nint byteOffset) => Unsafe.AsPointer(ref Unsafe.SubtractByteOffset(ref Unsafe.AsRef<byte>(source), byteOffset));

        /// <summary>
        ///     Determines the byte offset from origin to target from the given pointers.
        /// </summary>
        /// <param name="origin">The pointer to use as the origin of the offset.</param>
        /// <param name="target">The pointer whose offset from <paramref name="origin" /> is computed.</param>
        /// <returns>The byte offset from origin to target, that is, target - origin.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint ByteOffset(void* origin, void* target) => Unsafe.ByteOffset(ref Unsafe.AsRef<byte>(origin), ref Unsafe.AsRef<byte>(target));
    }
}