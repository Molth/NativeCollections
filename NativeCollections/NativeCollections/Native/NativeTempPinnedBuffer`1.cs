using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CA2231
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe temp pinned buffer
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public readonly unsafe struct NativeTempPinnedBuffer<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _buffer != null;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length => _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeTempPinnedBuffer(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, nameof(length));
            _buffer = ManagedMemoryAllocator.AlignedAlloc<T>((uint)length);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="zeroed">Zeroed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeTempPinnedBuffer(int length, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(length, nameof(length));
            _buffer = zeroed ? ManagedMemoryAllocator.AlignedAllocZeroed<T>((uint)length) : ManagedMemoryAllocator.AlignedAlloc<T>((uint)length);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeTempPinnedBuffer(int length, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(length, nameof(length));
            ThrowHelpers.ThrowIfNegative(alignment, nameof(alignment));
            ThrowHelpers.ThrowIfAlignmentNotBePow2((uint)alignment, nameof(alignment));
            ThrowHelpers.ThrowIfLessThan((uint)alignment, (uint)NativeMemoryAllocator.AlignOf<T>(), nameof(alignment));
            _buffer = (T*)ManagedMemoryAllocator.AlignedAlloc((uint)(length * sizeof(T)), (uint)alignment);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="alignment">Alignment</param>
        /// <param name="zeroed">Zeroed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeTempPinnedBuffer(int length, int alignment, bool zeroed)
        {
            ThrowHelpers.ThrowIfNegative(length, nameof(length));
            ThrowHelpers.ThrowIfNegative(alignment, nameof(alignment));
            ThrowHelpers.ThrowIfAlignmentNotBePow2((uint)alignment, nameof(alignment));
            ThrowHelpers.ThrowIfLessThan((uint)alignment, (uint)NativeMemoryAllocator.AlignOf<T>(), nameof(alignment));
            _buffer = zeroed ? ManagedMemoryAllocator.AlignedAllocZeroed<T>((uint)(length * sizeof(T))) : ManagedMemoryAllocator.AlignedAlloc<T>((uint)(length * sizeof(T)));
            _length = length;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var buffer = _buffer;
            if (buffer == null)
                return;
            ManagedMemoryAllocator.AlignedFree(buffer);
        }

        /// <summary>
        ///     As span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_buffer), _length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_buffer), _length);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeTempPinnedBuffer<{typeof(T).Name}>[{_length}]";

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeTempPinnedBuffer<T> Create(ReadOnlySpan<T> buffer)
        {
            var temp = new NativeTempPinnedBuffer<T>(buffer.Length);
            buffer.CopyTo(temp.AsSpan());
            return temp;
        }

        /// <summary>
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Create
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
        ///     Empty
        /// </summary>
        public static NativeTempPinnedBuffer<T> Empty => new();
    }
}