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
    ///     Native memory reader extensions
    /// </summary>
    public static unsafe class NativeMemoryReaderExtensions
    {
        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="nativeMemoryReader">Native memory reader</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(this ref NativeMemoryReader nativeMemoryReader) where T : unmanaged
        {
            if (nativeMemoryReader.Position + sizeof(T) > nativeMemoryReader.Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {sizeof(T)}, but buffer length is {nativeMemoryReader.Remaining}.");
            var obj = Unsafe.ReadUnaligned<T>(nativeMemoryReader.Array + nativeMemoryReader.Position);
            nativeMemoryReader.Position += sizeof(T);
            return obj;
        }

        /// <summary>
        ///     Try read
        /// </summary>
        /// <param name="nativeMemoryReader">Native memory reader</param>
        /// <param name="obj">object</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Read</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRead<T>(this ref NativeMemoryReader nativeMemoryReader, out T obj) where T : unmanaged
        {
            if (nativeMemoryReader.Position + sizeof(T) > nativeMemoryReader.Length)
            {
                obj = default;
                return false;
            }

            obj = Unsafe.ReadUnaligned<T>(nativeMemoryReader.Array + nativeMemoryReader.Position);
            nativeMemoryReader.Position += sizeof(T);
            return true;
        }
    }
}