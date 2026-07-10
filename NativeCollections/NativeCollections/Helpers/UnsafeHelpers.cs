using System.Runtime.CompilerServices;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe helpers
    /// </summary>
    internal static unsafe class UnsafeHelpers
    {
        /// <summary>
        ///     Returns if a given pointer is a null reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(void* ptr) => (nint)ptr == 0;

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AsPointer<T>(ref T value)
#if NET9_0_OR_GREATER
            where T : allows ref struct
#endif
            => (T*)Unsafe.AsPointer(ref value);

        /// <summary>
        ///     Reads a value of type <typeparamref name="T" /> from the given location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadUnaligned<T>(ref T source) where T : unmanaged => Unsafe.ReadUnaligned<T>(ref Unsafe.As<T, byte>(ref source));

        /// <summary>
        ///     Writes a value of type <typeparamref name="T" /> to the given location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnaligned<T>(ref T destination, in T value) where T : unmanaged => Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref destination), value);

        /// <summary>
        ///     Adds an element offset to the given pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Add<T>(ref T source, nint elementOffset) where T : unmanaged => AsPointer(ref Unsafe.Add(ref source, elementOffset));

        /// <summary>
        ///     Adds an element offset to the given pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Add<T>(void* source, nint elementOffset) where T : unmanaged => AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<T>(source), elementOffset));

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
        /// <returns>The byte offset from origin to target, that is, target - origin.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint ByteOffset(void* origin, void* target) => Unsafe.ByteOffset(ref Unsafe.AsRef<byte>(origin), ref Unsafe.AsRef<byte>(target));
    }
}