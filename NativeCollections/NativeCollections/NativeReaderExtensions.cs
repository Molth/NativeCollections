using System.Runtime.CompilerServices;
#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native reader extensions
    /// </summary>
    public static unsafe class NativeReaderExtensions
    {
        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="nativeReader">Native reader</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(this ref NativeReader nativeReader) where T : unmanaged
        {
            if (nativeReader.Position + sizeof(T) > nativeReader.Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {sizeof(T)}, but buffer length is {nativeReader.Remaining}.");
            var obj = Unsafe.ReadUnaligned<T>(ref Unsafe.AddByteOffset(ref nativeReader.Array, nativeReader.Position));
            nativeReader.Position += sizeof(T);
            return obj;
        }

        /// <summary>
        ///     Try read
        /// </summary>
        /// <param name="nativeReader">Native reader</param>
        /// <param name="obj">object</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Read</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRead<T>(this ref NativeReader nativeReader, out T obj) where T : unmanaged
        {
            if (nativeReader.Position + sizeof(T) > nativeReader.Length)
            {
                obj = default;
                return false;
            }

            obj = Unsafe.ReadUnaligned<T>(ref Unsafe.AddByteOffset(ref nativeReader.Array, nativeReader.Position));
            nativeReader.Position += sizeof(T);
            return true;
        }
    }
}