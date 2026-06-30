using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET9_0_OR_GREATER
using System.Collections;
#endif

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe value list builder
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    [IsReferenceOrContainsReferences]
    [BindingType(typeof(ArrayPool<>))]
    [IsAssignableTo(typeof(IIsCreated), typeof(IDisposable), typeof(IReadOnlyCollection<>))]
    public unsafe ref struct UnsafeListBuilder<T>
#if NET9_0_OR_GREATER
        : IIsCreated, IDisposable, IReadOnlyCollection<T>
#endif
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private Span<T> _buffer;

        /// <summary>
        ///     Array
        /// </summary>
        private T[]? _array;

        /// <summary>
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length
        {
            readonly get => _length;
            set => _length = value;
        }

        /// <summary>
        ///     Count
        /// </summary>
        public readonly int Count => _length;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _buffer.Length;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetCapacity(value);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[int index] => ref _buffer[index];

        /// <summary>
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => !Unsafe.IsNullRef(ref MemoryMarshal.GetReference(_buffer));

        /// <summary>
        ///     Is empty
        /// </summary>
        public readonly bool IsEmpty => _length == 0;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeListBuilder(Span<T> buffer)
        {
            _buffer = buffer;
            _array = null;
            _length = 0;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeListBuilder(Span<T> buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(length, buffer.Length, ExceptionArgument.length);
            _buffer = buffer;
            _array = null;
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeListBuilder(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            _buffer = _array = ArrayPool<T>.Shared.Rent(capacity);
            _length = 0;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeListBuilder(int capacity, int length)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(length, capacity, ExceptionArgument.length);
            _buffer = _array = ArrayPool<T>.Shared.Rent(capacity);
            _length = length;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var array = _array;
            if (array == null)
                return;
            _array = null;
            ArrayPool<T>.Shared.Return(array);
        }

        /// <summary>
        ///     As ref
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref UnsafeListBuilder<T> AsRef()
        {
#if NET9_0_OR_GREATER
            return ref Unsafe.AsRef(in this);
#else
            fixed (UnsafeListBuilder<T>* ptr = &this)
            {
                return ref *ptr;
            }
#endif
        }

        /// <summary>
        ///     As pointer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnsafeListBuilder<T>* AsPointer()
        {
#if NET9_0_OR_GREATER
            return UnsafeHelpers.AsPointer(ref Unsafe.AsRef(in this));
#else
            fixed (UnsafeListBuilder<T>* ptr = &this)
            {
                return ptr;
            }
#endif
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _length = 0;

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(bool clear)
        {
            if (clear)
                _buffer.Clear();
            _length = 0;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <returns>Equals</returns>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => SR.Format("UnsafeListBuilder<{0}>[{1}]", SR.GetTypeName(typeof(T)), _length);

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in T item)
        {
            var length = _length;
            var buffer = _buffer;
            if ((uint)length < (uint)buffer.Length)
            {
                buffer[length] = item;
                _length = length + 1;
            }
            else
            {
                Grow();
                _buffer[length] = item;
                _length = length + 1;
            }
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(ReadOnlySpan<T> source)
        {
            var length = _length;
            var buffer = _buffer;
            if (source.Length == 1 && (uint)length < (uint)buffer.Length)
            {
                buffer[length] = source[0];
                _length = length + 1;
            }
            else
            {
                if ((uint)(_length + source.Length) > (uint)_buffer.Length)
                    Grow(_buffer.Length - _length + source.Length);
                source.CopyTo(_buffer.Slice(_length));
                _length += source.Length;
            }
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan() => _buffer.Slice(0, _length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan() => _buffer.Slice(0, _length);

        /// <summary>
        ///     As memory
        /// </summary>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Memory<T> AsMemory() => new(_array, 0, _length);

        /// <summary>
        ///     As readOnly memory
        /// </summary>
        /// <returns>ReadOnlyMemory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlyMemory<T> AsReadOnlyMemory() => new(_array, 0, _length);

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (_buffer.Length < capacity)
                Grow(capacity - _buffer.Length);
            return _buffer.Length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = (int)(_buffer.Length * 0.9);
            if (_length < threshold)
                SetCapacity(_length);
            return _buffer.Length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (capacity < _length || capacity >= _buffer.Length)
                return _buffer.Length;
            SetCapacity(capacity);
            return _buffer.Length;
        }

        /// <summary>
        ///     Set capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(capacity, _length, ExceptionArgument.capacity);
            if (capacity != _buffer.Length)
            {
                var destination = ArrayPool<T>.Shared.Rent(capacity);
                if (_length > 0)
                    _buffer.Slice(0, _length).CopyTo(destination);
                var array = _array;
                _buffer = (Span<T>)(_array = destination);
                if (array == null)
                    return;
                ArrayPool<T>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Grow
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int additionalCapacityRequired = 1)
        {
            var minimumLength = Math.Max(_buffer.Length != 0 ? _buffer.Length * 2 : 4, _buffer.Length + additionalCapacityRequired);
            if ((uint)minimumLength > ArrayHelpers.MaxLength)
                minimumLength = Math.Max(Math.Max(_buffer.Length + 1, ArrayHelpers.MaxLength), _buffer.Length);
            SetCapacity(minimumLength);
        }

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeListBuilder<T> Create(ReadOnlySpan<T> buffer)
        {
            var temp = new UnsafeListBuilder<T>(buffer.Length, buffer.Length);
            buffer.CopyTo(temp.AsSpan());
            return temp;
        }

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeListBuilder<T> Create(in T arg0)
        {
            var temp = new UnsafeListBuilder<T>(1, 1);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            return temp;
        }

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1)
        {
            var temp = new UnsafeListBuilder<T>(2, 2);
            var buffer = temp.AsSpan();
            buffer[0] = arg0;
            buffer[1] = arg1;
            return temp;
        }

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2)
        {
            var temp = new UnsafeListBuilder<T>(3, 3);
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
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2, in T arg3)
        {
            var temp = new UnsafeListBuilder<T>(4, 4);
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
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4)
        {
            var temp = new UnsafeListBuilder<T>(5, 5);
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
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5)
        {
            var temp = new UnsafeListBuilder<T>(6, 6);
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
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6)
        {
            var temp = new UnsafeListBuilder<T>(7, 7);
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
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7)
        {
            var temp = new UnsafeListBuilder<T>(8, 8);
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
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8)
        {
            var temp = new UnsafeListBuilder<T>(9, 9);
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
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9)
        {
            var temp = new UnsafeListBuilder<T>(10, 10);
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
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9, in T arg10)
        {
            var temp = new UnsafeListBuilder<T>(11, 11);
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
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9, in T arg10, in T arg11)
        {
            var temp = new UnsafeListBuilder<T>(12, 12);
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
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9, in T arg10, in T arg11, in T arg12)
        {
            var temp = new UnsafeListBuilder<T>(13, 13);
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
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9, in T arg10, in T arg11, in T arg12, in T arg13)
        {
            var temp = new UnsafeListBuilder<T>(14, 14);
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
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9, in T arg10, in T arg11, in T arg12, in T arg13, in T arg14)
        {
            var temp = new UnsafeListBuilder<T>(15, 15);
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
        public static UnsafeListBuilder<T> Create(in T arg0, in T arg1, in T arg2, in T arg3, in T arg4, in T arg5, in T arg6, in T arg7, in T arg8, in T arg9, in T arg10, in T arg11, in T arg12, in T arg13, in T arg14, in T arg15)
        {
            var temp = new UnsafeListBuilder<T>(16, 16);
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
        public static UnsafeListBuilder<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        public readonly Span<T>.Enumerator GetEnumerator() => _buffer.GetEnumerator();

#if NET9_0_OR_GREATER
        /// <summary>
        ///     Get enumerator
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
#endif
    }
}