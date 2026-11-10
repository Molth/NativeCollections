using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2231 // Overload operator equals on overriding ValueType.Equals.
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a pinned temporary buffer for unmanaged elements allocated with the managed memory allocator.
    /// </summary>
    /// <typeparam name="T">The unmanaged element type of the pinned buffer.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [BindingType(typeof(ManagedMemoryAllocator))]
    public readonly unsafe struct NativeTempPinnedBuffer<T> : IIsCreated, IDisposable where T : unmanaged
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Initializes an instance from its final storage state.
        /// </summary>
        /// <param name="buffer">The allocated buffer.</param>
        /// <param name="length">The number of elements in the buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeTempPinnedBuffer(T* buffer, int length)
        {
            _buffer = buffer;
            _length = length;
        }

        /// <summary>
        ///     Creates a pinned temporary buffer with the specified length.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <returns>A pinned temporary buffer with the specified length.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            var buffer = ManagedMemoryAllocator.AlignedAlloc<T>(Usize.FromI32Unchecked(length));
            return new NativeTempPinnedBuffer<T>(buffer, length);
        }

        /// <summary>
        ///     Creates a pinned temporary buffer with the specified length and zeroing option.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory.</param>
        /// <returns>A pinned temporary buffer with the specified length.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(int length, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            var nativeLength = Usize.FromI32Unchecked(length);
            var buffer = zeroed ? ManagedMemoryAllocator.AlignedAllocZeroed<T>(nativeLength) : ManagedMemoryAllocator.AlignedAlloc<T>(nativeLength);
            return new NativeTempPinnedBuffer<T>(buffer, length);
        }

        /// <summary>
        ///     Creates a pinned temporary buffer with the specified length and alignment.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <param name="alignment">The required alignment in bytes.</param>
        /// <returns>A pinned temporary buffer with the specified length.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> or <paramref name="alignment" /> is negative,
        ///     or the alignment is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(int length, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            var nativeLength = Usize.FromI32Unchecked(length);
            var nativeAlignment = Usize.FromI32Unchecked(alignment);
            ThrowHelpers.ThrowIfLessThan(nativeAlignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            var byteCount = nativeLength * Usize.FromI32Unchecked(Unsafe.SizeOf<T>());
            var buffer = (T*)ManagedMemoryAllocator.AlignedAlloc(byteCount, nativeAlignment);
            return new NativeTempPinnedBuffer<T>(buffer, length);
        }

        /// <summary>
        ///     Creates a pinned temporary buffer with the specified length, alignment and zeroing option.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <param name="alignment">The required alignment in bytes.</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory.</param>
        /// <returns>A pinned temporary buffer with the specified length.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> or <paramref name="alignment" /> is negative,
        ///     or the alignment is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(int length, int alignment, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            var nativeLength = Usize.FromI32Unchecked(length);
            var nativeAlignment = Usize.FromI32Unchecked(alignment);
            ThrowHelpers.ThrowIfLessThan(nativeAlignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            var byteCount = nativeLength * Usize.FromI32Unchecked(Unsafe.SizeOf<T>());
            var buffer = zeroed ? (T*)ManagedMemoryAllocator.AlignedAllocZeroed(byteCount, nativeAlignment) : (T*)ManagedMemoryAllocator.AlignedAlloc(byteCount, nativeAlignment);
            return new NativeTempPinnedBuffer<T>(buffer, length);
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="buffer">The values to copy into this.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(ReadOnlySpan<T> buffer)
        {
            var temp = Create(buffer.Length);
            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(temp.AsSpan())), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI32Unchecked(buffer.Length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(T arg0)
        {
            var temp = Create(1);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1)
        {
            var temp = Create(2);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2)
        {
            var temp = Create(3);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2, T arg3)
        {
            var temp = Create(4);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4)
        {
            var temp = Create(5);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5)
        {
            var temp = Create(6);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6)
        {
            var temp = Create(7);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7)
        {
            var temp = Create(8);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8)
        {
            var temp = Create(9);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9)
        {
            var temp = Create(10);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <param name="arg10">The value to store at index 10 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9, T arg10)
        {
            var temp = Create(11);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            buffer[10] = arg10;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <param name="arg10">The value to store at index 10 in the buffer.</param>
        /// <param name="arg11">The value to store at index 11 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9, T arg10, T arg11)
        {
            var temp = Create(12);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            buffer[10] = arg10;
            buffer[11] = arg11;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <param name="arg10">The value to store at index 10 in the buffer.</param>
        /// <param name="arg11">The value to store at index 11 in the buffer.</param>
        /// <param name="arg12">The value to store at index 12 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9, T arg10, T arg11, T arg12)
        {
            var temp = Create(13);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            buffer[10] = arg10;
            buffer[11] = arg11;
            buffer[12] = arg12;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <param name="arg10">The value to store at index 10 in the buffer.</param>
        /// <param name="arg11">The value to store at index 11 in the buffer.</param>
        /// <param name="arg12">The value to store at index 12 in the buffer.</param>
        /// <param name="arg13">The value to store at index 13 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9, T arg10, T arg11, T arg12, T arg13)
        {
            var temp = Create(14);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            buffer[10] = arg10;
            buffer[11] = arg11;
            buffer[12] = arg12;
            buffer[13] = arg13;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <param name="arg10">The value to store at index 10 in the buffer.</param>
        /// <param name="arg11">The value to store at index 11 in the buffer.</param>
        /// <param name="arg12">The value to store at index 12 in the buffer.</param>
        /// <param name="arg13">The value to store at index 13 in the buffer.</param>
        /// <param name="arg14">The value to store at index 14 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9, T arg10, T arg11, T arg12, T arg13, T arg14)
        {
            var temp = Create(15);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            buffer[10] = arg10;
            buffer[11] = arg11;
            buffer[12] = arg12;
            buffer[13] = arg13;
            buffer[14] = arg14;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="arg0">The value to store at index 0 in the buffer.</param>
        /// <param name="arg1">The value to store at index 1 in the buffer.</param>
        /// <param name="arg2">The value to store at index 2 in the buffer.</param>
        /// <param name="arg3">The value to store at index 3 in the buffer.</param>
        /// <param name="arg4">The value to store at index 4 in the buffer.</param>
        /// <param name="arg5">The value to store at index 5 in the buffer.</param>
        /// <param name="arg6">The value to store at index 6 in the buffer.</param>
        /// <param name="arg7">The value to store at index 7 in the buffer.</param>
        /// <param name="arg8">The value to store at index 8 in the buffer.</param>
        /// <param name="arg9">The value to store at index 9 in the buffer.</param>
        /// <param name="arg10">The value to store at index 10 in the buffer.</param>
        /// <param name="arg11">The value to store at index 11 in the buffer.</param>
        /// <param name="arg12">The value to store at index 12 in the buffer.</param>
        /// <param name="arg13">The value to store at index 13 in the buffer.</param>
        /// <param name="arg14">The value to store at index 14 in the buffer.</param>
        /// <param name="arg15">The value to store at index 15 in the buffer.</param>
        /// <returns>A new instance containing the specified values.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static NativeTempPinnedBuffer<T> Create(T arg0, T arg1, T arg2, T arg3, T arg4, T arg5, T arg6, T arg7, T arg8, T arg9, T arg10, T arg11, T arg12, T arg13, T arg14, T arg15)
        {
            var temp = Create(16);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            buffer[10] = arg10;
            buffer[11] = arg11;
            buffer[12] = arg12;
            buffer[13] = arg13;
            buffer[14] = arg14;
            buffer[15] = arg15;
            return temp;
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _buffer != null;

        /// <summary>
        ///     Gets the number of elements in this buffer.
        /// </summary>
        public int Length => _length;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => ManagedMemoryAllocator.AlignedFree(_buffer);

        /// <summary>
        ///     Returns a span over the entire contents of this.
        /// </summary>
        /// <returns>A span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_buffer), _length);

        /// <summary>
        ///     Returns a read-only span over the entire contents of this.
        /// </summary>
        /// <returns>A read-only span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_buffer), _length);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeTempPinnedBuffer<{0}>[{1}]", SR.GetTypeName(typeof(T)), _length);
    }
}