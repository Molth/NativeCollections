using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CA2231
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native temp buffer
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly struct NativeTempBuffer<T> : IDisposable where T : struct
    {
        /// <summary>
        ///     Array
        /// </summary>
        private readonly T[] _array;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _array != null;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length => _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeTempBuffer(int length)
        {
            _array = ArrayPool<T>.Shared.Rent(length);
            _length = length;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => ArrayPool<T>.Shared.Return(_array);

        /// <summary>
        ///     As span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => _array.AsSpan(0, _length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => _array.AsSpan(0, _length);

        /// <summary>
        ///     As memory
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory() => new(_array, 0, _length);

        /// <summary>
        ///     As readOnly memory
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> AsReadOnlyMemory() => new(_array, 0, _length);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => throw new NotSupportedException("CannotCallEquals");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => throw new NotSupportedException("CannotCallGetHashCode");

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeTempBuffer<{typeof(T).Name}>[{_length}]";

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeTempBuffer<T> Empty => new();
    }
}