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
        /// <param name="reader">Reader</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(this in NativeMemoryReader reader) where T : unmanaged
        {
            ref var value = ref Unsafe.AsRef(in reader);
            if (value.Position + sizeof(T) > value.Length)
                throw new ArgumentOutOfRangeException(nameof(T), $"Requires size is {sizeof(T)}, but buffer length is {value.Remaining}.");
            var obj = Unsafe.ReadUnaligned<T>(value.Array + value.Position);
            value.Position += sizeof(T);
            return obj;
        }

        /// <summary>
        ///     Try read
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="obj">object</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Read</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRead<T>(this in NativeMemoryReader reader, out T obj) where T : unmanaged
        {
            ref var value = ref Unsafe.AsRef(in reader);
            if (value.Position + sizeof(T) > value.Length)
            {
                obj = default;
                return false;
            }

            obj = Unsafe.ReadUnaligned<T>(value.Array + value.Position);
            value.Position += sizeof(T);
            return true;
        }
    }
}