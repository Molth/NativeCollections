using System;
using System.Buffers;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native memory manager
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public sealed unsafe class NativeMemoryManager<T> : MemoryManager<T> where T : unmanaged
    {
        /// <summary>
        ///     Array
        /// </summary>
        private readonly NativeArray<T> _array;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="array">array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMemoryManager(NativeArray<T> array) => _array = array;

        /// <summary>
        ///     Array
        /// </summary>
        public NativeArray<T> Array
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array;
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativeMemoryManager<T> nativeMemoryManager) => nativeMemoryManager.Memory.Span;

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativeMemoryManager<T> nativeMemoryManager) => nativeMemoryManager.Memory.Span;

        /// <summary>
        ///     As memory
        /// </summary>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Memory<T>(NativeMemoryManager<T> nativeMemoryManager) => nativeMemoryManager.Memory;

        /// <summary>
        ///     As readOnly memory
        /// </summary>
        /// <returns>ReadOnlyMemory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>(NativeMemoryManager<T> nativeMemoryManager) => nativeMemoryManager.Memory;

        /// <summary>
        ///     As native array
        /// </summary>
        /// <returns>NativeArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<T>(NativeMemoryManager<T> nativeMemoryManager) => nativeMemoryManager._array;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Dispose(true);

        /// <summary>
        ///     Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Dispose(bool disposing) => _array.Dispose();

        /// <summary>
        ///     Get span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Span<T> GetSpan() => _array.AsSpan();

        /// <summary>
        ///     Pin
        /// </summary>
        /// <param name="elementIndex">Element index</param>
        /// <returns>MemoryHandle</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MemoryHandle Pin(int elementIndex = 0) => new(_array.Array + elementIndex);

        /// <summary>
        ///     Unpin
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Unpin()
        {
        }
    }
}