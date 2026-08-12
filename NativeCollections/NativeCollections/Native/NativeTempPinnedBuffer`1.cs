using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2231 // Overload operator equals on overriding ValueType.Equals
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a pinned temporary buffer for unmanaged elements allocated with the managed memory allocator.
    /// </summary>
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
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        public int Length => _length;

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified number of elements,
        ///     using the natural alignment of <typeparamref name="T" />
        ///     and without zero-initializing the allocated memory.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="length" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeTempPinnedBuffer(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            _buffer = ManagedMemoryAllocator.AlignedAlloc<T>((uint)length);
            _length = length;
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified number of elements,
        ///     using the natural alignment of <typeparamref name="T" /> and optionally zero-initializing the memory.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <param name="zeroed">
        ///     <see langword="true" /> to zero-initialize the allocated memory;
        ///     otherwise, the memory content is undefined.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="length" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeTempPinnedBuffer(int length, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            _buffer = zeroed ? ManagedMemoryAllocator.AlignedAllocZeroed<T>((uint)length) : ManagedMemoryAllocator.AlignedAlloc<T>((uint)length);
            _length = length;
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified number of elements and alignment,
        ///     without zero-initializing the allocated memory.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <param name="alignment">
        ///     The required alignment in bytes,
        ///     which must be a power of two and at least the natural alignment of <typeparamref name="T" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="length" /> or <paramref name="alignment" /> is negative,
        ///     or if <paramref name="alignment" /> is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="alignment" /> is not a power of two.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeTempPinnedBuffer(int length, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfLessThan((uint)alignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            _buffer = (T*)ManagedMemoryAllocator.AlignedAlloc((uint)(length * Unsafe.SizeOf<T>()), (uint)alignment);
            _length = length;
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified number of elements, alignment, and zero-initialization option.
        /// </summary>
        /// <param name="length">The number of elements to allocate.</param>
        /// <param name="alignment">
        ///     The required alignment in bytes,
        ///     which must be a power of two and at least the natural alignment of <typeparamref name="T" />.
        /// </param>
        /// <param name="zeroed">
        ///     <see langword="true" /> to zero-initialize the allocated memory;
        ///     otherwise, the memory content is undefined.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="length" /> or <paramref name="alignment" /> is negative,
        ///     or if <paramref name="alignment" /> is less than the natural alignment of <typeparamref name="T" />.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="alignment" /> is not a power of two.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeTempPinnedBuffer(int length, int alignment, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfLessThan((uint)alignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            _buffer = zeroed ? ManagedMemoryAllocator.AlignedAllocZeroed<T>((uint)(length * Unsafe.SizeOf<T>())) : ManagedMemoryAllocator.AlignedAlloc<T>((uint)(length * Unsafe.SizeOf<T>()));
            _length = length;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var buffer = _buffer;
            if (UnsafeHelpers.IsNull(buffer))
                return;
            ManagedMemoryAllocator.AlignedFree(buffer);
        }

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_buffer), _length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_buffer), _length);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <returns>Equals</returns>
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
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeTempPinnedBuffer<{0}>[{1}]", SR.GetTypeName(typeof(T)), _length);

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(ReadOnlySpan<T> buffer)
        {
            var temp = new NativeTempPinnedBuffer<T>(buffer.Length);
            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(temp.AsSpan())), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * Unsafe.SizeOf<T>()));
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0)
        {
            var temp = new NativeTempPinnedBuffer<T>(1);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1)
        {
            var temp = new NativeTempPinnedBuffer<T>(2);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2)
        {
            var temp = new NativeTempPinnedBuffer<T>(3);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            return temp;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2, in T arg3)
        {
            var temp = new NativeTempPinnedBuffer<T>(4);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4)
        {
            var temp = new NativeTempPinnedBuffer<T>(5);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5)
        {
            var temp = new NativeTempPinnedBuffer<T>(6);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6)
        {
            var temp = new NativeTempPinnedBuffer<T>(7);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7)
        {
            var temp = new NativeTempPinnedBuffer<T>(8);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8)
        {
            var temp = new NativeTempPinnedBuffer<T>(9);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9)
        {
            var temp = new NativeTempPinnedBuffer<T>(10);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9, in T arg10)
        {
            var temp = new NativeTempPinnedBuffer<T>(11);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9, in T arg10, in T arg11)
        {
            var temp = new NativeTempPinnedBuffer<T>(12);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9, in T arg10, in T arg11, in T arg12)
        {
            var temp = new NativeTempPinnedBuffer<T>(13);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9, in T arg10, in T arg11, in T arg12, in T arg13)
        {
            var temp = new NativeTempPinnedBuffer<T>(14);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9, in T arg10, in T arg11, in T arg12, in T arg13, in T arg14)
        {
            var temp = new NativeTempPinnedBuffer<T>(15);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9, in T arg10, in T arg11, in T arg12, in T arg13, in T arg14, in T arg15)
        {
            var temp = new NativeTempPinnedBuffer<T>(16);
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
        ///     Gets an empty instance.
        /// </summary>
        public static NativeTempPinnedBuffer<T> Empty => default;
    }
}